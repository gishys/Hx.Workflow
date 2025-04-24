using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkSubscription : Entity<Guid>, IMultiTenant
    {
        public Guid? WorkflowId { get; protected set; }
        public int StepId { get; protected set; }
        public Guid ExecutionPointerId { get; protected set; }
        public string EventName { get; protected set; }
        public string EventKey { get; protected set; }
        public DateTime SubscribeAsOf { get; protected set; }
        public string? SubscriptionData { get; protected set; }
        public string? ExternalToken { get; protected set; }
        public string? ExternalWorkerId { get; protected set; }
        public DateTime? ExternalTokenExpiry { get; protected set; }
        public Guid? TenantId { get; protected set; }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkSubscription()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        { }
        public WkSubscription(
            Guid id,
            Guid? workflowId,
            int stepId,
            Guid executionPointerId,
            string eventName,
            string eventKey,
            DateTime subscribeAsOf,
            string? subscriptionData,
            string? externalToken,
            string? externalWorkerId,
            DateTime? externalTokenExpiry,
            Guid? tenantId = null)
        {
            Id = id;
            WorkflowId = workflowId;
            StepId = stepId;
            ExecutionPointerId = executionPointerId;
            EventName = eventName;
            EventKey = eventKey;
            SubscribeAsOf = subscribeAsOf;
            SubscriptionData = subscriptionData;
            ExternalToken = externalToken;
            ExternalWorkerId = externalWorkerId;
            ExternalTokenExpiry = externalTokenExpiry;
            TenantId = tenantId;
        }
        public Task SetExternalToken(string? externalToken)
        {
            ExternalToken = externalToken;
            return Task.CompletedTask;
        }
        public Task SetExternalWorkerId(string? externalWorkerId)
        {
            ExternalWorkerId = externalWorkerId;
            return Task.CompletedTask;
        }
        public Task SetExternalTokenExpiry(DateTime? externalTokenExpiry)
        {
            ExternalTokenExpiry = externalTokenExpiry;
            return Task.CompletedTask;
        }
    }
}
