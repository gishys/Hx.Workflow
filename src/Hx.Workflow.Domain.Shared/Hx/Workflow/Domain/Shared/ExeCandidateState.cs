using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Shared
{
    public enum ExeCandidateState
    {
        /// <summary>
        /// 被回退
        /// </summary>
        BeRolledBack = 1,
        /// <summary>
        /// 待接收
        /// </summary>
        WaitingReceipt = 2,
        /// <summary>
        /// 待完成
        /// </summary>
        Pending = 3,
        /// <summary>
        /// 已终止
        /// </summary>
        Terminated = 4,
        /// <summary>
        /// 已挂起
        /// </summary>
        Suspended = 5,
        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 6,
        /// <summary>
        /// 等待中
        /// </summary>
        Waiting = 7,
    }
}