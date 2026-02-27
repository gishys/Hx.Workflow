using System;

namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 超期/SLA 统计：按模板分项
    /// </summary>
    public class OverdueStat
    {
        public Guid? DefinitionId { get; set; }
        public string? DefinitionTitle { get; set; }
        public int OverdueCount { get; set; }
        public int TotalCount { get; set; }
        public double OverdueRate { get; set; }
    }
}
