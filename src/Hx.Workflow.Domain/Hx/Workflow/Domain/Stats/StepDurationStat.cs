namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 节点/步骤处理时长统计：平均处理时长、经过次数
    /// </summary>
    public class StepDurationStat
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public double AvgDurationMinutes { get; set; }
        public int PassCount { get; set; }
    }
}
