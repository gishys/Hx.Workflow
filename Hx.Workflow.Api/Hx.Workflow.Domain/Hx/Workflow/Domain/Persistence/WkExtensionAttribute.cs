using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkExtensionAttribute : Entity<Guid>, IMultiTenant
    {
        public Guid ExecutionPointerId { get; protected set; }
        public WkExecutionPointer WkExecutionPointer { get; protected set; }
        public string AttributeKey { get; protected set; }
        public string AttributeValue { get; protected set; }
        public Guid? TenantId { get; protected set; }
        public WkExtensionAttribute()
        { }
        public WkExtensionAttribute(
            string attributeKey,
            string attributeValue,
            Guid? tenantId = null)
        {
            AttributeKey = attributeKey;
            AttributeValue = attributeValue;
            TenantId = tenantId;
        }
        public Task SetWkExecutionPointer(WkExecutionPointer wkExecutionPointer)
        {
            WkExecutionPointer = wkExecutionPointer;
            return Task.CompletedTask;
        }
        public Task SetAttributeKey(string attributeKey)
        {
            AttributeKey = attributeKey;
            return Task.CompletedTask;
        }
        public Task SetAttributeValue(string attributeValue)
        {
            AttributeValue = attributeValue;
            return Task.CompletedTask;
        }
    }
}
