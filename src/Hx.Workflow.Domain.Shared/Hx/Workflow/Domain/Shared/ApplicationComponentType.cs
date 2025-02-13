using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Shared
{
    public enum ApplicationComponentType
    {
        /// <summary>
        /// 路由组件
        /// </summary>
        RouteComponent = 1,
        /// <summary>
        /// 源码组件
        /// </summary>
        SourceCodeComponent = 2,
        /// <summary>
        /// Url组件
        /// </summary>
        UrlComponent = 3,
    }
}
