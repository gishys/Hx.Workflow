using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    /// <summary>
    /// 错误统计完整返回：汇总 + 按定义分组 + 按节点分组
    /// </summary>
    public class ErrorsStatResultDto
    {
        public int TotalErrorCount { get; set; }
        public int AffectedInstanceCount { get; set; }
        public double ErrorRate { get; set; }
        public List<ErrorStatDto> ByDefinition { get; set; } = new();
        public List<ErrorByStepStatDto> ByStep { get; set; } = new();
    }
}
