using System;

namespace Hx.Workflow.Application.Contracts
{
    public class OverdueStatDto
    {
        public Guid? DefinitionId { get; set; }
        public string? DefinitionTitle { get; set; }
        public int OverdueCount { get; set; }
        public int TotalCount { get; set; }
        public double OverdueRate { get; set; }
    }
}
