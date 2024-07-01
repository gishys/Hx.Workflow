using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.BusinessModule
{
    public class WkPointerEventData
    {
        public string DecideBranching { get; set; }
        public StepExecutionType? ExecutionType { get; set; }
        public string Candidates { get; set; }
        public DateTime? CommitmentDeadline { get; set; }
    }
}
