using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    /// <summary>
    /// 仪表盘汇总：概览 + 按定义 Top N + 超期率 + 近期趋势
    /// </summary>
    public class DashboardStatDto
    {
        public InstanceOverviewStatDto Overview { get; set; } = new();
        public List<DefinitionStatDto> DefinitionTopN { get; set; } = [];
        public List<OverdueStatDto> OverdueSummary { get; set; } = [];
        public List<TrendStatDto> RecentTrend { get; set; } = [];
    }
}
