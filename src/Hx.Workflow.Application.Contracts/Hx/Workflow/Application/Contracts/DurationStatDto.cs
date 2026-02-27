using System;

namespace Hx.Workflow.Application.Contracts
{
    public class DurationStatDto
    {
        public Guid? DefinitionId { get; set; }
        public string? DefinitionTitle { get; set; }
        public string? BusinessType { get; set; }
        public double AvgDurationMinutes { get; set; }
        public double MedianDurationMinutes { get; set; }
        public int CompletedCount { get; set; }
    }
}
