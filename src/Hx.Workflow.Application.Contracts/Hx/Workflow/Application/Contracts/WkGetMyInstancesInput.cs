using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    /// <summary>
    /// 获取我的实例输入参数
    /// </summary>
    public class WkGetMyInstancesInput
    {
        /// <summary>
        /// 创建者ID列表
        /// </summary>
        public ICollection<Guid>? CreatorIds { get; set; }
        
        /// <summary>
        /// 模板ID列表
        /// </summary>
        public ICollection<Guid>? DefinitionIds { get; set; }
        
        /// <summary>
        /// 模板版本列表（可选，null表示所有版本）
        /// </summary>
        public ICollection<int>? DefinitionVersions { get; set; }
        
        /// <summary>
        /// 实例数据
        /// </summary>
        public IDictionary<string, object>? InstanceData { get; set; }
        
        /// <summary>
        /// 用户ID列表
        /// </summary>
        public ICollection<Guid>? userIds { get; set; }
    }
}