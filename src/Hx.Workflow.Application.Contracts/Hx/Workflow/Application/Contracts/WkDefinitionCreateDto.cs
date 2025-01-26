using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.ObjectExtending;

namespace Hx.Workflow.Application.Contracts
{
    public class WkDefinitionCreateDto : ExtensibleObject
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 流程定义名称
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 限制时间（分钟）
        /// </summary>
        public int? LimitTime { get; set; }
        /// <summary>
        /// get or set icon
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// get or set color
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Discription { get; set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType { get; set; }
        /// <summary>
        /// 流程类型
        /// </summary>
        public string ProcessType { get; set; }
        public ICollection<WkNodeCreateDto> Nodes { get; set; }
        public ICollection<WkCandidateCreateDto> WkCandidates { get; set; }
    }
}