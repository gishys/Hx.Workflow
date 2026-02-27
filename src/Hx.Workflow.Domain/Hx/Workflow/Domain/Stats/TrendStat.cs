using System;

namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 趋势统计（时间序列）：按时间桶的创建数、完成数
    /// </summary>
    public class TrendStat
    {
        public DateTime PeriodStart { get; set; }
        public int CreatedCount { get; set; }
        public int CompletedCount { get; set; }
    }
}
