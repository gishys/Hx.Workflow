using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Domain.Shared
{
    public enum WkInstanceState
    {
        /// <summary>
        /// 处理中
        /// </summary>
        InProcess,
        /// <summary>
        /// 已终止
        /// </summary>
        Terminated,
        /// <summary>
        /// 中间状态
        /// </summary>
        IntermediateState,
        /// <summary>
        /// 已挂起
        /// </summary>
        Suspended,
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        /// <summary>
        /// 已废弃
        /// </summary>
        Obsolete,
        /// <summary>
        /// 已初始
        /// </summary>
        Initial
    }
}
