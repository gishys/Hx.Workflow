namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 错误汇总：错误总数、涉及实例数（错误率由应用层计算）
    /// </summary>
    public class ErrorSummaryStat
    {
        public int TotalErrorCount { get; set; }
        public int AffectedInstanceCount { get; set; }
    }
}
