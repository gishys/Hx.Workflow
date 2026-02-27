using System;

namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 按模板（定义）统计：各定义的实例总数、运行中、完成、终止数
    /// </summary>
    public class DefinitionStat
    {
        public Guid DefinitionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Version { get; set; }
        public int TotalCount { get; set; }
        public int RunningCount { get; set; }
        public int CompleteCount { get; set; }
        public int TerminatedCount { get; set; }
    }
}
