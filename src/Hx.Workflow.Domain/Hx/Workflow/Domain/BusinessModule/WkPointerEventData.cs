using Hx.Workflow.Domain.Shared;
using System;

namespace Hx.Workflow.Domain.BusinessModule
{
    public class WkPointerEventData
    {
        public string? DecideBranching { get; set; }
        public StepExecutionType? ExecutionType { get; set; }
        public string? Candidates { get; set; }
        public DateTime? CommitmentDeadline { get; set; }
    }
}
