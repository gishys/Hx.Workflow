using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;

namespace Hx.Workflow.Application.Contracts
{
    public class WkDefinitionDto : ExtensibleEntityDto<Guid>, IHasCreationTime
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 流程定义名称
        /// </summary>
        public required string Title { get; set; }
        /// <summary>
        /// 限制时间
        /// </summary>
        public int? LimitTime { get; set; }
        /// <summary>
        /// get or set group()
        /// </summary>
        public Guid? GroupId { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int SortNumber { get; set; }
        /// <summary>
        /// Get multi tenant id
        /// </summary>
        public Guid? TenantId { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public required string BusinessType { get; set; }
        /// <summary>
        /// 流程类型
        /// </summary>
        public required string ProcessType { get; set; }
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; protected set; }
        /// <summary>
        /// 节点集合
        /// </summary>
        public required ICollection<WkNodeDto> Nodes { get; set; }
        /// <summary>
        /// 后选者
        /// </summary>
        public required ICollection<WkCandidateDto> WkCandidates { get; set; }
    }
}
