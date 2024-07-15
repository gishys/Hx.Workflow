using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class InstanceBusinessDataInput
    {
        public Guid WorkflowId { get; set; }
        public string ProcessName { get; set; }
        public string Located { get; set; }
    }
}