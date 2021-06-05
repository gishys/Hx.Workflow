using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hx.Workflow.Domain.StepBodys
{
    public class WkAuditor : FullAuditedEntity<Guid>, IMultiTenant
    {
        public Guid WorkflowId { get; protected set; }
        public WkInstance Workflow { get; protected set; }
        public Guid ExecutionPointerId { get; protected set; }
        public WkExecutionPointer ExecutionPointer { get; protected set; }
        public EnumAuditStatus Status { get; protected set; }
        public DateTime? AuditTime { get; protected set; }
        public string Remark { get; protected set; }
        public Guid? UserId { get; protected set; }
        public string UserName { get; protected set; }
        public Guid? TenantId { get; protected set; }
        public WkAuditor()
        { }
        public WkAuditor(
            Guid workflowId,
            Guid executionPointerId,
            string userName,
                        Guid? userId = null,
            EnumAuditStatus status = EnumAuditStatus.UnAudited,
            Guid? tenantId = null)
        {
            WorkflowId = workflowId;
            ExecutionPointerId = executionPointerId;
            UserId = userId;
            UserName = userName;
            Status = status;
            TenantId = tenantId;
        }
        public virtual Task Audit(
            EnumAuditStatus status,
            DateTime? auditTime,
            string remark
            )
        {
            Status = status;
            AuditTime = auditTime;
            Remark = remark;
            return Task.CompletedTask;
        }
    }
}