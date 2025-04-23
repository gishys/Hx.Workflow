using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkAuditorDto
    {
        public Guid Id { get; set; }
        public Guid WorkflowId { get; set; }
        public Guid ExecutionPointerId { get; set; }
        public EnumAuditStatus Status { get; set; }
        public DateTime? AuditTime { get; set; }
        public string? Remark { get; set; }
        public Guid? UserId { get; set; }
        public required string UserName { get; set; }
        public Guid? TenantId { get; set; }
    }
}
