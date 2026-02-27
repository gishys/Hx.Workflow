using System;

namespace Hx.Workflow.Application.Contracts
{
    public class DefinitionStatDto
    {
        public Guid DefinitionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Version { get; set; }
        public int TotalCount { get; set; }
        public int RunningCount { get; set; }
        public int CompleteCount { get; set; }
        public int TerminatedCount { get; set; }
    }
}
