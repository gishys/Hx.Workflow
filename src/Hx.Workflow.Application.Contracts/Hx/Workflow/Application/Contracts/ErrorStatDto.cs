using System;

namespace Hx.Workflow.Application.Contracts
{
    public class ErrorStatDto
    {
        public Guid? DefinitionId { get; set; }
        public string? DefinitionTitle { get; set; }
        public int ErrorCount { get; set; }
    }
}
