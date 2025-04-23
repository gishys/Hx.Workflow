namespace Hx.Workflow.Application.Contracts
{
    public class ProcessTypeStatDto
    {
        /// <summary>
        /// 一级分类
        /// </summary>
        public required string PClassification { get; set; }
        /// <summary>
        /// 二级分类
        /// </summary>
        public string? SClassification { get; set; }
        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }
    }
}
