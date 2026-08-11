using Hx.Workflow.Application;
using Hx.Workflow.Application.StepBodys;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.BusinessModule;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.EntityFrameworkCore;
using System;
using System.Data;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
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
        public async Task UnconditionalForwardEdge_UsesNextNodeNameAsDecision()
        {
            var step = new WkNode("核定", "核定", StepNodeType.Activity, 1);
            await step.AddNextNode(new WkNodeRelation("登簿", WkRoleNodeType.Forward));

            Assert.True(BranchDecisionValidator.CanTransition(step, "登簿"));
            Assert.False(BranchDecisionValidator.CanTransition(step, "缮证"));
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

        private static WkSubscription Subscription(DateTime expiry)
            => new(
                Guid.NewGuid(), Guid.NewGuid(), 1, Guid.NewGuid(),
                "WorkflowCore.Activity", "activity", DateTime.UtcNow.AddHours(-1),
                null, "token", "worker", expiry);
    }
}
