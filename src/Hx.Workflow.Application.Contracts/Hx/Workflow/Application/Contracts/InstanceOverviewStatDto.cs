namespace Hx.Workflow.Application.Contracts
{
    public class InstanceOverviewStatDto
    {
        public int TotalCount { get; set; }
        public int RunningCount { get; set; }
        public int CompleteCount { get; set; }
        public int TerminatedCount { get; set; }
        public int SuspendedCount { get; set; }
    }
}
