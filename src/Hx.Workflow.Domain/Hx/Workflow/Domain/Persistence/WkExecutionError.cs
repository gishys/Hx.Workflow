using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkExecutionError : Entity<Guid>, IMultiTenant
    {
        public Guid WkInstanceId { get; protected set; }
        public Guid WkExecutionPointerId { get; protected set; }
        public DateTime ErrorTime { get; protected set; }
        public string Message { get; protected set; }
        public Guid? TenantId { get; protected set; }
        public WkExecutionError()
        { }
        public WkExecutionError(
            Guid wkInstanceId,
            Guid wkExecutionPointerId,
            DateTime errorTime,
            string message,
            Guid? tenantId = null
            )
        {
            WkInstanceId = wkInstanceId;
            WkExecutionPointerId = wkExecutionPointerId;
            ErrorTime = errorTime;
            Message = message;
            TenantId = tenantId;
        }
    }
}
