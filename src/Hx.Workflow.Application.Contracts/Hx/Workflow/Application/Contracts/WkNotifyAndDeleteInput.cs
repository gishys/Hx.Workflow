using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    /// <summary>
    /// 通知外部系统并删除流程实例的输入参数
    /// </summary>
    public class WkNotifyAndDeleteInput
    {
        /// <summary>
        /// 流程实例 ID
        /// </summary>
        public Guid WorkflowId { get; set; }

        /// <summary>
        /// 扩展数据，App 端可传入驳回原因（RejectReason）、驳回人（Rejector）等
        /// </summary>
        public Dictionary<string, string> ExtraData { get; set; } = [];
    }
}
