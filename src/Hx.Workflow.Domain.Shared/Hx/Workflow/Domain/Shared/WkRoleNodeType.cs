using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Shared
{
    /// <summary>
    /// 规则中的节点类型
    /// </summary>
    public enum WkRoleNodeType
    {
        /// <summary>
        /// 向前（发送）
        /// </summary>
        Forward = 1,
        /// <summary>
        /// 回退
        /// </summary>
        RolledBack = 2
    }
}
