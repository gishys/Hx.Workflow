using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using WorkflowCore.Models;

namespace Hx.Workflow.Application
{
    public class WkActivitySubmissionProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<WkActivitySubmissionProcessor> logger) : BackgroundService
    {
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
        private static readonly TimeSpan ProcessingLease = TimeSpan.FromMinutes(2);
        private const int MaxAttempts = 5;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var submissions = await GetProcessableAsync(stoppingToken);
                    foreach (var submission in submissions)
                    {
                        await ProcessAsync(submission, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to poll workflow activity submissions.");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private async Task<List<WkActivitySubmission>> GetProcessableAsync(CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var dataFilter = scope.ServiceProvider.GetRequiredService<IDataFilter>();
            var repository = scope.ServiceProvider.GetRequiredService<IWkActivitySubmissionRepository>();
            var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
            using (dataFilter.Disable<IMultiTenant>())
            using (var uow = uowManager.Begin(requiresNew: true, isTransactional: false))
            {
                var result = await repository.GetProcessableAsync(DateTime.UtcNow, 50, cancellationToken);
                await uow.CompleteAsync(cancellationToken);
                return result;
            }
        }

        private async Task ProcessAsync(WkActivitySubmission submission, CancellationToken cancellationToken)
        {
            if (submission.Status == WkActivitySubmissionStatus.EventPublished)
            {
                await ReconcileAsync(submission, cancellationToken);
                return;
            }

            if (!await TryClaimAsync(submission, cancellationToken))
            {
                return;
            }

            try
            {
                using var scope = scopeFactory.CreateScope();
                var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
                using (currentTenant.Change(submission.TenantId))
                {
                    var eventRepository = scope.ServiceProvider.GetRequiredService<IWkEventRepository>();
                    var manager = scope.ServiceProvider.GetRequiredService<HxWorkflowManager>();
                    var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

                    using (var readUow = uowManager.Begin(requiresNew: true, isTransactional: false))
                    {
                        var existingEvent = await eventRepository.GetByEventKeyAsync(submission.ActivityName);
                        await readUow.CompleteAsync(cancellationToken);
                        if (!WkActivitySubmissionPolicies.ShouldPublishEvent(existingEvent))
                        {
                            await MarkEventPublishedAsync(submission, cancellationToken);
                            return;
                        }
                    }

                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(submission.Payload);
                    await manager.StartActivityAsync(
                        submission.ActivityName,
                        submission.WorkflowId.ToString(),
                        data);
                    await MarkEventPublishedAsync(submission, cancellationToken);
                }
            }
            catch (BusinessException ex)
            {
                await MarkRejectedAsync(submission, ex.Message, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to process activity submission {SubmissionId} (attempt {Attempt}).",
                    submission.Id,
                    submission.AttemptCount + 1);
                if (submission.AttemptCount + 1 >= MaxAttempts)
                {
                    await MarkFailedAsync(submission, ex.Message, cancellationToken);
                }
                else
                {
                    await ReleaseForRetryAsync(submission, ex.Message, cancellationToken);
                }
            }
        }

        private async Task<bool> TryClaimAsync(WkActivitySubmission submission, CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
            using (currentTenant.Change(submission.TenantId))
            {
                var repository = scope.ServiceProvider.GetRequiredService<IWkActivitySubmissionRepository>();
                var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
                var now = DateTime.UtcNow;
                var claimed = await repository.TryClaimAsync(
                    submission.Id, now, now.Add(ProcessingLease), cancellationToken);
                await uow.CompleteAsync(cancellationToken);
                return claimed;
            }
        }

        private async Task ReconcileAsync(WkActivitySubmission submission, CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
            using (currentTenant.Change(submission.TenantId))
            {
                var instanceRepository = scope.ServiceProvider.GetRequiredService<IWkInstanceRepository>();
                var errorRepository = scope.ServiceProvider.GetRequiredService<IWkErrorRepository>();
                var submissionRepository = scope.ServiceProvider.GetRequiredService<IWkActivitySubmissionRepository>();
                var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);

                if (!Guid.TryParse(submission.ActivityName, out var activityId))
                {
                    await submissionRepository.MarkRejectedAsync(
                        submission.Id, "activityName 不是有效的 GUID。", DateTime.UtcNow, cancellationToken);
                }
                else
                {
                    var pointer = await instanceRepository.GetPointerAsync(activityId);
                    if (pointer == null || pointer.WkInstanceId != submission.WorkflowId)
                    {
                        await submissionRepository.MarkRejectedAsync(
                            submission.Id, "活动不属于指定流程实例。", DateTime.UtcNow, cancellationToken);
                    }
                    else if (pointer.Status == PointerStatus.Complete)
                    {
                        await submissionRepository.MarkSucceededAsync(submission.Id, DateTime.UtcNow, cancellationToken);
                    }
                    else if (pointer.Status == PointerStatus.Failed)
                    {
                        var errors = await errorRepository.GetListByIdAsync(submission.WorkflowId, activityId);
                        var message = errors.OrderByDescending(x => x.ErrorTime).FirstOrDefault()?.Message
                            ?? "活动执行失败。";
                        await submissionRepository.MarkFailedAsync(submission.Id, message, DateTime.UtcNow, cancellationToken);
                    }
                }

                await uow.CompleteAsync(cancellationToken);
            }
        }

        private Task MarkEventPublishedAsync(WkActivitySubmission submission, CancellationToken cancellationToken)
            => UpdateAsync(submission, (r, now) => r.MarkEventPublishedAsync(submission.Id, now, cancellationToken));

        private Task MarkRejectedAsync(WkActivitySubmission submission, string error, CancellationToken cancellationToken)
            => UpdateAsync(submission, (r, now) => r.MarkRejectedAsync(submission.Id, error, now, cancellationToken));

        private Task MarkFailedAsync(WkActivitySubmission submission, string error, CancellationToken cancellationToken)
            => UpdateAsync(submission, (r, now) => r.MarkFailedAsync(submission.Id, error, now, cancellationToken));

        private Task ReleaseForRetryAsync(WkActivitySubmission submission, string error, CancellationToken cancellationToken)
            => UpdateAsync(submission, (r, now) => r.ReleaseForRetryAsync(submission.Id, error, now, cancellationToken));

        private async Task UpdateAsync(
            WkActivitySubmission submission,
            Func<IWkActivitySubmissionRepository, DateTime, Task> update)
        {
            using var scope = scopeFactory.CreateScope();
            var currentTenant = scope.ServiceProvider.GetRequiredService<ICurrentTenant>();
            using (currentTenant.Change(submission.TenantId))
            {
                var repository = scope.ServiceProvider.GetRequiredService<IWkActivitySubmissionRepository>();
                var uowManager = scope.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
                using var uow = uowManager.Begin(requiresNew: true, isTransactional: true);
                await update(repository, DateTime.UtcNow);
                await uow.CompleteAsync();
            }
        }
    }
}
