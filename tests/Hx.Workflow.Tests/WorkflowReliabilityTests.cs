using Autofac.Extensions.DependencyInjection;
using Hx.Workflow.Application;
using Hx.Workflow.Application.StepBodys;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.BusinessModule;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using Xunit;

namespace Hx.Workflow.Tests
{
    public class WorkflowReliabilityTests
    {
        [Fact]
        public void SameActivityRequest_CanBeRetriedIdempotently()
        {
            const string payload = "{\"DecideBranching\":\"登簿\",\"Remark\":\"同意\"}";

            var first = WkActivitySubmissionPolicies.ComputeRequestHash(payload);
            var retry = WkActivitySubmissionPolicies.ComputeRequestHash(
                "{\"Remark\":\"同意\",\"DecideBranching\":\"登簿\"}");
            var changed = WkActivitySubmissionPolicies.ComputeRequestHash(
                "{\"DecideBranching\":\"缮证\"}");

            Assert.Equal(first, retry);
            Assert.NotEqual(first, changed);
        }

        [Fact]
        public void SavedEvent_AfterLostHttpResponse_IsNotPublishedAgain()
        {
            var savedEvent = new WkEvent(
                Guid.NewGuid(), "WorkflowCore.Activity", Guid.NewGuid().ToString(), "null",
                DateTime.UtcNow, false);

            Assert.False(WkActivitySubmissionPolicies.ShouldPublishEvent(savedEvent));
            Assert.True(WkActivitySubmissionPolicies.ShouldPublishEvent(null));
        }

        [Fact]
        public void ExpiredSubscriptionToken_CanBeClaimedAgain()
        {
            var now = DateTime.UtcNow;
            var expired = Subscription(now.AddMinutes(-1));
            var active = Subscription(now.AddMinutes(1));
            var predicate = WkSubscriptionQueries
                .OpenForEvent("WorkflowCore.Activity", "activity", now)
                .Compile();

            Assert.True(predicate(expired));
            Assert.False(predicate(active));
            Assert.True(WkSubscriptionQueries
                .ForEvent("WorkflowCore.Activity", "activity", now)
                .Compile()(active));
        }

        [Fact]
        public void EventAndSubscriptionTimes_SurvivePersistenceMapping()
        {
            var eventTime = DateTime.UtcNow.AddMinutes(-2);
            var subscribeAsOf = DateTime.UtcNow.AddMinutes(-1);
            var persistedEvent = new WkEvent(
                Guid.NewGuid(), "WorkflowCore.Activity", "activity", "null", eventTime, false);
            var persistedSubscription = new WkSubscription(
                Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(),
                "WorkflowCore.Activity", "activity", subscribeAsOf, "null", null, null, null);

            Event restoredEvent = persistedEvent.ToEvent();
            EventSubscription restoredSubscription = persistedSubscription.ToEventSubscription();

            Assert.Equal(eventTime, restoredEvent.EventTime);
            Assert.Equal(subscribeAsOf, restoredSubscription.SubscribeAsOf);
        }

        [Fact]
        public async Task LegacyInfiniteSubscriptionTime_CanBeRepairedFromEventTime()
        {
            var eventTime = DateTime.UtcNow.AddMinutes(-1);
            var subscription = new WkSubscription(
                Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(),
                "WorkflowCore.Activity", "activity", DateTime.MinValue,
                null, null, null, null);

            Assert.True(WkSubscriptionQueries.NeedsSubscribeAsOfRepair(subscription.SubscribeAsOf));

            await subscription.SetSubscribeAsOf(eventTime);

            Assert.Equal(eventTime, subscription.SubscribeAsOf);
            Assert.False(WkSubscriptionQueries.NeedsSubscribeAsOfRepair(subscription.SubscribeAsOf));
        }

        [Fact]
        public async Task UnconditionalForwardEdge_UsesNextNodeNameAsDecision()
        {
            var step = new WkNode("核定", "核定", StepNodeType.Activity, 1);
            await step.AddNextNode(new WkNodeRelation("登簿", WkRoleNodeType.Forward));

            Assert.True(BranchDecisionValidator.CanTransition(step, "登簿"));
            Assert.False(BranchDecisionValidator.CanTransition(step, "缮证"));
        }

        [Fact]
        public async Task CrossStepActivitySubmission_IsRejectedBeforeIdempotencyReservation()
        {
            var definition = Definition();
            var currentNode = new WkNode("current", "current", StepNodeType.Activity, 1);
            await currentNode.AddNextNode(new WkNodeRelation("next", WkRoleNodeType.Forward));
            await definition.AddWkNode(currentNode);

            var error = ActivitySubmissionValidator.Validate(
                new WkPointerEventData
                {
                    Step = "stale-step",
                    DecideBranching = "wrong-next",
                    ExecutionType = StepExecutionType.Forward
                },
                "current",
                definition);

            Assert.NotNull(error);
            Assert.Contains("stale-step", error);
            Assert.Contains("current", error);
        }

        [Fact]
        public async Task InvalidForwardBranch_IsRejectedBeforeIdempotencyReservation()
        {
            var definition = Definition();
            var currentNode = new WkNode("current", "current", StepNodeType.Activity, 1);
            await currentNode.AddNextNode(new WkNodeRelation("next", WkRoleNodeType.Forward));
            await definition.AddWkNode(currentNode);

            var error = ActivitySubmissionValidator.Validate(
                new WkPointerEventData
                {
                    Step = "current",
                    DecideBranching = "wrong-next",
                    ExecutionType = StepExecutionType.Forward
                },
                "current",
                definition);

            Assert.NotNull(error);
            Assert.Contains("wrong-next", error);
            Assert.Contains("current", error);
        }

        [Fact]
        public void NullTenantId_UsesTypedPostgreSqlUuidParameter()
        {
            var parameter = WkActivitySubmissionParameterFactory.GetTenantParameterSpecification(null);

            Assert.Equal(DbType.Guid, parameter.DbType);
            Assert.Equal(DBNull.Value, parameter.Value);
        }

        [Fact]
        public void ActivityPayload_JsonElements_SurviveEventPersistence()
        {
            const string payload = "{\"DecideBranching\":\"核定\",\"ExecutionType\":1,\"Remark\":\"同意\"}";
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(payload)!;
            var activity = new ActivityResult
            {
                Status = ActivityResult.StatusType.Success,
                SubscriptionId = Guid.NewGuid().ToString(),
                Data = data
            };
            var workflowEvent = new Event
            {
                Id = Guid.NewGuid().ToString(),
                EventName = "WorkflowCore.Activity",
                EventKey = Guid.NewGuid().ToString(),
                EventTime = DateTime.UtcNow,
                EventData = activity
            };

            var restored = (ActivityResult)workflowEvent.ToPersistable().ToEvent().EventData;
            var restoredData = JsonSerializer.Deserialize<WkPointerEventData>(
                JsonSerializer.Serialize(restored.Data));

            Assert.Equal("核定", restoredData!.DecideBranching);
            Assert.Equal(StepExecutionType.Forward, restoredData.ExecutionType);
        }

        [Fact]
        public async Task DuplicateRuntimeLifecycleCalls_AreExecutedOnlyOnce()
        {
            var guard = new WorkflowRuntimeGuard();
            var initializeCount = 0;
            var startCount = 0;
            var stopCount = 0;

            await Task.WhenAll(
                guard.InitializeOnceAsync(() => IncrementAsync(() => Interlocked.Increment(ref initializeCount))),
                guard.InitializeOnceAsync(() => IncrementAsync(() => Interlocked.Increment(ref initializeCount))));

            await Task.WhenAll(
                guard.StartOnceAsync(() => IncrementAsync(() => Interlocked.Increment(ref startCount))),
                guard.StartOnceAsync(() => IncrementAsync(() => Interlocked.Increment(ref startCount))));

            await Task.WhenAll(
                guard.StopOnceAsync(() => IncrementAsync(() => Interlocked.Increment(ref stopCount))),
                guard.StopOnceAsync(() => IncrementAsync(() => Interlocked.Increment(ref stopCount))));

            Assert.Equal(1, initializeCount);
            Assert.Equal(1, startCount);
            Assert.Equal(1, stopCount);
        }

        [Fact]
        public async Task FailedRuntimeStart_CanBeRetried()
        {
            var guard = new WorkflowRuntimeGuard();
            var attempts = 0;

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                guard.StartOnceAsync(() =>
                {
                    attempts++;
                    throw new InvalidOperationException("simulated startup failure");
                }));

            await guard.StartOnceAsync(() =>
            {
                attempts++;
                return Task.CompletedTask;
            });

            Assert.Equal(2, attempts);
        }

        [Fact]
        public void WorkflowHost_IsEnabledByDefaultForBackwardCompatibility()
        {
            Assert.True(new HxWorkflowRuntimeOptions().RunHost);
            Assert.Equal("WorkflowCore", HxWorkflowRuntimeOptions.SectionName);
        }

        [Fact]
        public async Task AutofacChildScope_CannotDisposeWorkflowLifeCyclePublisher()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            services.AddWorkflow();

            var customTaskState = new BackgroundTaskState();
            var customTaskDescriptor = ServiceDescriptor.Transient<IBackgroundTask>(
                _ => new TrackingBackgroundTask(customTaskState));
            services.Add(customTaskDescriptor);

            var workflowCoreAssembly = typeof(IWorkflowHost).Assembly;
            var originalTaskDescriptorCount = services.Count(
                descriptor => descriptor.ServiceType == typeof(IBackgroundTask));
            var workflowCoreWorkerTypes = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(IBackgroundTask) &&
                    descriptor.ImplementationType != null)
                .Select(descriptor => descriptor.ImplementationType!)
                .ToArray();

            Assert.Single(services.Where(descriptor =>
                descriptor.ServiceType == typeof(IBackgroundTask) &&
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ImplementationFactory?.Method.DeclaringType?.Assembly ==
                    workflowCoreAssembly));

            Assert.True(WorkflowCoreServiceCollectionCompatibility
                .ReplaceLifeCyclePublisherBackgroundTaskAlias(services));

            Assert.Contains(customTaskDescriptor, services);
            Assert.Equal(
                originalTaskDescriptorCount,
                services.Count(descriptor =>
                    descriptor.ServiceType == typeof(IBackgroundTask)));
            Assert.Empty(services.Where(descriptor =>
                descriptor.ServiceType == typeof(IBackgroundTask) &&
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ImplementationFactory?.Method.DeclaringType?.Assembly ==
                    workflowCoreAssembly));

            var providerFactory = new AutofacServiceProviderFactory();
            var containerBuilder = providerFactory.CreateBuilder(services);
            var serviceProvider = providerFactory.CreateServiceProvider(containerBuilder);

            try
            {
                var publisher = serviceProvider
                    .GetRequiredService<ILifeCycleEventPublisher>();

                using (var childScope = serviceProvider.CreateScope())
                {
                    var childTasks = childScope.ServiceProvider
                        .GetServices<IBackgroundTask>()
                        .ToArray();

                    Assert.Equal(originalTaskDescriptorCount, childTasks.Length);
                    Assert.DoesNotContain(
                        childTasks,
                        task => ReferenceEquals(task, publisher));
                    Assert.Single(childTasks.OfType<TrackingBackgroundTask>());
                    Assert.Equal(
                        childTasks.Length,
                        childTasks.Select(task => task.GetType()).Distinct().Count());

                    foreach (var workerType in workflowCoreWorkerTypes)
                    {
                        Assert.Single(childTasks.Where(task =>
                            task.GetType() == workerType));
                    }
                }

                Assert.Same(
                    publisher,
                    serviceProvider.GetRequiredService<ILifeCycleEventPublisher>());

                var host = serviceProvider.GetRequiredService<IWorkflowHost>();
                await host.StartAsync(CancellationToken.None);
                await host.StopAsync(CancellationToken.None);

                Assert.Equal(1, customTaskState.StartCount);
                Assert.Equal(1, customTaskState.StopCount);
            }
            finally
            {
                (serviceProvider as IDisposable)?.Dispose();
            }
        }

        private static async Task IncrementAsync(Action increment)
        {
            await Task.Delay(10);
            increment();
        }

        private static WkSubscription Subscription(DateTime expiry)
            => new(
                Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(),
                "WorkflowCore.Activity", "activity", DateTime.UtcNow.AddHours(-1),
                null, "token", "worker", expiry);

        private static WkDefinition Definition()
            => new(
                Guid.NewGuid(), "test", 1, null, "test", "test");

        private sealed class BackgroundTaskState
        {
            public int StartCount;
            public int StopCount;
        }

        private sealed class TrackingBackgroundTask(BackgroundTaskState state)
            : IBackgroundTask
        {
            public void Start()
            {
                Interlocked.Increment(ref state.StartCount);
            }

            public void Stop()
            {
                Interlocked.Increment(ref state.StopCount);
            }
        }
    }
}
