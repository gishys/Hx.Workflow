namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 按节点（步骤）分组的错误统计
    /// </summary>
    public class ErrorByStepStat
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int ErrorCount { get; set; }
    }
}
