namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 实例概览统计：总数、运行中、已完成、已终止、挂起
    /// </summary>
    public class InstanceOverviewStat
    {
        public int TotalCount { get; set; }
        public int RunningCount { get; set; }
        public int CompleteCount { get; set; }
        public int TerminatedCount { get; set; }
        public int SuspendedCount { get; set; }
    }
}
