using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkActivitySubmissionRepository(
        IDbContextProvider<WkDbContext> dbContextProvider)
        : EfCoreRepository<WkDbContext, WkActivitySubmission, Guid>(dbContextProvider),
          IWkActivitySubmissionRepository
    {
        public async Task<WkActivitySubmission?> FindByKeyAsync(
            Guid workflowId,
            string activityName,
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .FirstOrDefaultAsync(
                    x => x.WorkflowId == workflowId && x.ActivityName == activityName,
                    cancellationToken);
        }

        public async Task<(WkActivitySubmission Submission, bool Created)> GetOrCreateAsync(
            WkActivitySubmission submission,
            CancellationToken cancellationToken = default)
        {
            var dbContext = await GetDbContextAsync();
            var entityType = dbContext.Model.FindEntityType(typeof(WkActivitySubmission))!;
            var sqlHelper = dbContext.GetService<ISqlGenerationHelper>();
            var table = sqlHelper.DelimitIdentifier(entityType.GetTableName()!, entityType.GetSchema());
            // The only interpolated fragment is an identifier obtained from EF metadata and
            // quoted by the provider. All request values remain database parameters below.
            var commandText = $@"
                INSERT INTO {table}
                    (""ID"", ""WORKFLOWID"", ""ACTIVITYNAME"", ""PAYLOAD"", ""REQUESTHASH"",
                     ""STATUS"", ""CREATIONTIME"", ""LASTMODIFICATIONTIME"", ""ATTEMPTCOUNT"", ""TENANTID"")
                VALUES
                    ({{0}}, {{1}}, {{2}}, {{3}}, {{4}}, {{5}}, {{6}}, {{7}}, {{8}}, {{9}})
                ON CONFLICT (""WORKFLOWID"", ""ACTIVITYNAME"") DO NOTHING";
            var tenantParameter = WkActivitySubmissionParameterFactory.CreateTenantParameter(submission.TenantId);
            var affectedRows = await dbContext.Database.ExecuteSqlRawAsync(
                commandText,
                [
                    submission.Id,
                    submission.WorkflowId,
                    submission.ActivityName,
                    submission.Payload,
                    submission.RequestHash,
                    (int)submission.Status,
                    submission.CreationTime,
                    submission.LastModificationTime,
                    submission.AttemptCount,
                    tenantParameter
                ],
                cancellationToken);

            var entity = await (await GetDbSetAsync())
                .AsNoTracking()
                .SingleAsync(
                    x => x.WorkflowId == submission.WorkflowId && x.ActivityName == submission.ActivityName,
                    cancellationToken);
            return (entity, affectedRows == 1);
        }

        public async Task<List<WkActivitySubmission>> GetProcessableAsync(
            DateTime asOf,
            int maxCount,
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .AsNoTracking()
                .Where(x =>
                    x.Status == WkActivitySubmissionStatus.Accepted ||
                    x.Status == WkActivitySubmissionStatus.EventPublished ||
                    (x.Status == WkActivitySubmissionStatus.Processing &&
                     (!x.LockedUntil.HasValue || x.LockedUntil <= asOf)))
                .OrderBy(x => x.CreationTime)
                .Take(maxCount)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> TryClaimAsync(
            Guid id,
            string expectedRequestHash,
            DateTime asOf,
            DateTime lockedUntil,
            CancellationToken cancellationToken = default)
        {
            var affectedRows = await (await GetDbSetAsync())
                .Where(x => x.Id == id &&
                    x.RequestHash == expectedRequestHash &&
                    (x.Status == WkActivitySubmissionStatus.Accepted ||
                     (x.Status == WkActivitySubmissionStatus.Processing &&
                      (!x.LockedUntil.HasValue || x.LockedUntil <= asOf))))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, WkActivitySubmissionStatus.Processing)
                    .SetProperty(x => x.LockedUntil, lockedUntil)
                    .SetProperty(x => x.LastModificationTime, asOf)
                    .SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1), cancellationToken);
            return affectedRows == 1;
        }

        public async Task<bool> TryReplaceAcceptedAsync(
            Guid id,
            string expectedRequestHash,
            string payload,
            string requestHash,
            DateTime asOf,
            CancellationToken cancellationToken = default)
        {
            var affectedRows = await (await GetDbSetAsync())
                .Where(x =>
                    x.Id == id &&
                    x.Status == WkActivitySubmissionStatus.Accepted &&
                    x.AttemptCount == 0 &&
                    x.RequestHash == expectedRequestHash)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Payload, payload)
                    .SetProperty(x => x.RequestHash, requestHash)
                    .SetProperty(x => x.Error, (string?)null)
                    .SetProperty(x => x.LastModificationTime, asOf), cancellationToken);
            return affectedRows == 1;
        }

        public Task MarkEventPublishedAsync(Guid id, DateTime asOf, CancellationToken cancellationToken = default)
            => SetStatusAsync(id, WkActivitySubmissionStatus.EventPublished, null, asOf, cancellationToken);

        public Task MarkSucceededAsync(Guid id, DateTime asOf, CancellationToken cancellationToken = default)
            => SetStatusAsync(id, WkActivitySubmissionStatus.Succeeded, null, asOf, cancellationToken);

        public Task MarkFailedAsync(Guid id, string error, DateTime asOf, CancellationToken cancellationToken = default)
            => SetStatusAsync(id, WkActivitySubmissionStatus.Failed, error, asOf, cancellationToken);

        public Task MarkRejectedAsync(Guid id, string error, DateTime asOf, CancellationToken cancellationToken = default)
            => SetStatusAsync(id, WkActivitySubmissionStatus.Rejected, error, asOf, cancellationToken);

        public Task ReleaseForRetryAsync(Guid id, string error, DateTime asOf, CancellationToken cancellationToken = default)
            => SetStatusAsync(id, WkActivitySubmissionStatus.Accepted, error, asOf, cancellationToken);

        private async Task SetStatusAsync(
            Guid id,
            WkActivitySubmissionStatus status,
            string? error,
            DateTime asOf,
            CancellationToken cancellationToken)
        {
            await (await GetDbSetAsync())
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, status)
                    .SetProperty(x => x.Error, error)
                    .SetProperty(x => x.LockedUntil, (DateTime?)null)
                    .SetProperty(x => x.LastModificationTime, asOf), cancellationToken);
        }
    }
}
