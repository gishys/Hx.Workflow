using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkEvent : CreationAuditedEntity<Guid>, IMultiTenant
    {
        public string Name { get;protected set; }
        public string Key { get; protected set; }
        public string Data { get; protected set; }
        public DateTime Time { get; protected set; }
        public bool IsProcessed { get; protected set; }
        public Guid? TenantId { get; protected set; }
        public WkEvent()
        { }
        public WkEvent(
            Guid id,
            string name,
            string key,
            string data,
            DateTime time,
            bool isProcessed,
            Guid? tenantId = null)
        {
            Id = id;
            Name = name;
            Key = key;
            Data = data;
            Time = time;
            IsProcessed = isProcessed;
            TenantId = tenantId;
        }
        public Task SetProcessed(bool processed)
        {
            IsProcessed = processed;
            return Task.CompletedTask;
        }
    }
}
