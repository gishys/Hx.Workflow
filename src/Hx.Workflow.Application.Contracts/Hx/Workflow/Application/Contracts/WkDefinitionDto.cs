using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkDefinitionDto
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 流程定义名称
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 限制时间
        /// </summary>
        public int? LimitTime { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public WkDefinitionState WkDefinitionState { get; set; }
        /// <summary>
        /// get or set icon
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// get or set color
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// get or set group()
        /// </summary>
        public Guid? GroupId { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Discription { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int SortNumber { get; set; }
        /// <summary>
        /// Get multi tenant id
        /// </summary>
        public Guid? TenantId { get; set; }
        /// <summary>
        /// 节点集合
        /// </summary>
        public virtual ICollection<WkNodeDto> Nodes { get; set; }
        /// <summary>
        /// 后选者
        /// </summary>
        public ICollection<WkCandidateDto> WkCandidates { get; set; }
    }
}
