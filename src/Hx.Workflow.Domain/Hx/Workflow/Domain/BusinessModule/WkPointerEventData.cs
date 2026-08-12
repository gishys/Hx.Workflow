using Hx.Workflow.Domain.Shared;
using System;
using System.Text.Json.Serialization;

namespace Hx.Workflow.Domain.BusinessModule
{
    public class WkPointerEventData
    {
        [JsonPropertyName("step")]
        public string? Step { get; set; }
        public string? DecideBranching { get; set; }
        public StepExecutionType? ExecutionType { get; set; }
        public string? Candidates { get; set; }
        public DateTime? CommitmentDeadline { get; set; }
    }
}
