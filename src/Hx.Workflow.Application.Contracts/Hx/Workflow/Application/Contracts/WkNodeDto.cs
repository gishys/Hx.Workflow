using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using Volo.Abp.Data;

namespace Hx.Workflow.Application.Contracts
{
    public class WkNodeDto
    {
        /// <summary>
        /// 步骤体Id
        /// </summary>
        public Guid? WkStepBodyId { get; set; }
        /// <summary>
        /// 节点Id
        /// </summary>
        public Guid? WkDefinitionId { get; set; }
        /// <summary>
        /// 步骤名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 步骤节点类型
        /// </summary>
        public StepNodeType StepNodeType { get; set; }
        /// <summary>
        /// 步骤版本
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 限制时间
        /// </summary>
        public int? LimitTime { get; set; }
        /// <summary>
        /// 扩展属性
        /// </summary>
        public ExtraPropertyDictionary ExtraProperties { get; set; }
        /// <summary>
        /// 流程参数
        /// </summary>
        public WkStepBodyDto StepBody { get; set; }
        /// <summary>
        /// 表单集合
        /// </summary>
        public virtual ICollection<ApplicationFormDto> ApplicationForms { get; set; }
        /// <summary>
        /// 节点条件
        /// </summary>
        public virtual ICollection<WkConditionNodeDto> NextNodes { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int SortNumber { get; set; }
        /// <summary>
        /// 节点执行者集合
        /// </summary>
        public virtual ICollection<WkCandidateDto> WkCandidates { get; set; }
        public virtual ICollection<WkParamDto> Params { get; set; }
    }
}
