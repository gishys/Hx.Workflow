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
        public string? Remark { get; protected set; }
        public Guid UserId { get; protected set; }
        public string UserName { get; protected set; }
        public Guid? TenantId { get; protected set; }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkAuditor()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        { }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkAuditor(
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            Guid workflowId,
            Guid executionPointerId,
            string userName,
            Guid userId,
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
        public virtual Task Audit(EnumAuditStatus status)
        {
            Status = status;
            return Task.CompletedTask;
        }
        public virtual Task Audit(
            DateTime auditTime,
            string? remark
            )
        {
            AuditTime = auditTime;
            Remark = remark;
            return Task.CompletedTask;
        }
    }
}