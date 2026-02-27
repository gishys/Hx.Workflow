using System;

namespace Hx.Workflow.Application.Contracts
{
    public class TrendStatDto
    {
        public DateTime PeriodStart { get; set; }
        public int CreatedCount { get; set; }
        public int CompletedCount { get; set; }
    }
}
