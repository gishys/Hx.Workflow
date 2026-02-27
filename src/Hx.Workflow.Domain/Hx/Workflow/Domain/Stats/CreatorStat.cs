using System;

namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 发起人统计：发起数量、已完成数量、完成率
    /// </summary>
    public class CreatorStat
    {
        public Guid CreatorId { get; set; }
        public int CreatedCount { get; set; }
        public int CompletedCount { get; set; }
        public double CompletedRate { get; set; }
    }
}
