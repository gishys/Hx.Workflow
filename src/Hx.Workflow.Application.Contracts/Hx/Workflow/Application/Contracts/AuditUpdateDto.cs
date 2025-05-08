using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class AuditUpdateDto
    {
        public Guid WkInstanceId { get; set; }
        public Guid ExecutionPointerId { get; set; }
        public required string Remark { get; set; }
    }
}
