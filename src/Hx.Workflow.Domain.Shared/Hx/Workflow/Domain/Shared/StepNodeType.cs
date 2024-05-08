namespace Hx.Workflow.Domain.Shared
{
    public enum StepNodeType
    {
        /// <summary>
        /// 开始
        /// </summary>
        Start = 1,
        /// <summary>
        /// 活动步骤节点
        /// </summary>
        Activity = 2,
        /// <summary>
        /// 结束
        /// </summary>
        End = 3
    }
}
