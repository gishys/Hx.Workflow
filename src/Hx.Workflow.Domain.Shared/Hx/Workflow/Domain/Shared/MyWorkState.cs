using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Shared
{
    public enum MyWorkState
    {
        /// <summary>
        /// 我的在办件
        /// </summary>
        BeingProcessed = 1,
        /// <summary>
        /// 我的待收件
        /// </summary>
        WaitingReceipt = 2,
        /// <summary>
        /// 我的待办件
        /// </summary>
        Pending = 3,
        /// <summary>
        /// 我的参与件
        /// </summary>
        Participation = 4,
        /// <summary>
        /// 我的受托件
        /// </summary>
        Entrusted = 5,
        /// <summary>
        /// 我的受理件
        /// </summary>
        Handled = 6,
        /// <summary>
        /// 我的关注件
        /// </summary>
        Follow = 7,
        /// <summary>
        /// 我得挂起件
        /// </summary>
        Suspended = 8,
        /// <summary>
        /// 我的会签件
        /// </summary>
        Countersign = 9,
        /// <summary>
        /// 我的抄送件
        /// </summary>
        CarbonCopy = 10,
        /// <summary>
        /// 异常审批件
        /// </summary>
        Abnormal = 11,
    }
}