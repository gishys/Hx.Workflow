using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Shared
{
    public enum ExePersonnelOperateType
    {
        /// <summary>
        /// 主办
        /// </summary>
        Host = 1,
        /// <summary>
        /// 抄送
        /// </summary>
        CarbonCopy = 2,
        /// <summary>
        /// 会签
        /// </summary>
        Countersign = 3,
        /// <summary>
        /// 委托
        /// </summary>
        Entrusted = 4,
        /// <summary>
        /// 通知
        /// </summary>
        Notify = 5,
    }
}