using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkExecutionErrorDto
    {
        public Guid WkInstanceId { get; set; }
        public Guid WkExecutionPointerId { get; set; }
        public DateTime ErrorTime { get; set; }
        public required string Message { get; set; }
        public Guid? TenantId { get; set; }
    }
}
