using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkDefinitionUpdateDto
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 流程定义名称
        /// </summary>
        public required string Title { get; set; }
        /// <summary>
        /// 限制时间（分钟）
        /// </summary>
        public int? LimitTime { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public required string BusinessType { get; set; }
        /// <summary>
        /// 流程类型
        /// </summary>
        public required string ProcessType { get; set; }
        /// <summary>
        /// 模板组Id
        /// </summary>
        public Guid? GroupId { get; set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
        public ICollection<WkCandidateCreateDto>? WkCandidates { get; set; }
    }
}
