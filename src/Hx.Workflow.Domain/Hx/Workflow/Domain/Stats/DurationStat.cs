using System;

namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 流程耗时统计：平均/中位数完成时长，按模板或业务类型分组
    /// </summary>
    public class DurationStat
    {
        public Guid? DefinitionId { get; set; }
        public string? DefinitionTitle { get; set; }
        public string? BusinessType { get; set; }
        public double AvgDurationMinutes { get; set; }
        public double MedianDurationMinutes { get; set; }
        public int CompletedCount { get; set; }
    }
}
