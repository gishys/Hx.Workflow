using System;

namespace Hx.Workflow.Application.Contracts
{
    public class CreatorStatDto
    {
        public Guid CreatorId { get; set; }
        public int CreatedCount { get; set; }
        public int CompletedCount { get; set; }
        public double CompletedRate { get; set; }
    }
}
