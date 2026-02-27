using System;

namespace Hx.Workflow.Domain.Stats
{
    /// <summary>
    /// 错误统计（按定义分组）：每一定义一条
    /// </summary>
    public class ErrorStat
    {
        public Guid? DefinitionId { get; set; }
        public string? DefinitionTitle { get; set; }
        public int ErrorCount { get; set; }
    }
}
