using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Domain.Shared
{
    public enum StepInstanceState
    {
        /// <summary>
        /// 被回退
        /// </summary>
        BeBackedDown,
        /// <summary>
        /// 待完成
        /// </summary>
        ToBeCompleted,
        /// <summary>
        /// 待接收
        /// </summary>
        ToBeReceived,
        /// <summary>
        /// 已召回
        /// </summary>
        ReCalled,
        /// <summary>
        /// 已完成
        /// </summary>
        Completed,
        /// <summary>
        /// 已废弃
        /// </summary>
        Obselet
    }
}
