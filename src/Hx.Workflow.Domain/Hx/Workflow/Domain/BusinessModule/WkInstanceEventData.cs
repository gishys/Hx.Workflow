using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.BusinessModule
{
    public class WkInstanceEventData
    {
        public string ProcessName {  get; set; }
        public string Located {  get; set; }
        public DateTime BusinessCommitmentDeadline {  get; set; }
        public string DecideBranching {  get; set; }
        public StepExecutionType? ExecutionType {  get; set; }
        public string Candidates {  get; set; }
    }
}
