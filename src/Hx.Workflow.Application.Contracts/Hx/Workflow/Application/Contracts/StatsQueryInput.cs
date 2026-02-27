using System;

namespace Hx.Workflow.Application.Contracts
{
    /// <summary>
    /// 统计查询通用参数：时间范围、模板、租户等
    /// </summary>
    public class StatsQueryInput
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Guid? DefinitionId { get; set; }
        public Guid? CreatorId { get; set; }
        /// <summary>day | week | month</summary>
        public string? Granularity { get; set; }
        /// <summary>created | completed | both</summary>
        public string? TrendType { get; set; }
        /// <summary>None | Definition | BusinessType</summary>
        public string? GroupBy { get; set; }
    }
}
