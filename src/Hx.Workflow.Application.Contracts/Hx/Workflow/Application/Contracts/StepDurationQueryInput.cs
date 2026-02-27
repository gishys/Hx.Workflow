using System;

namespace Hx.Workflow.Application.Contracts
{
    public class StepDurationQueryInput
    {
        public Guid DefinitionId { get; set; }
        public int Version { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
}
