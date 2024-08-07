﻿using Hx.Workflow.Domain.Shared;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkNodeCreateDto
    {
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
        /// 步骤体Id
        /// </summary>
        public string WkStepBodyId { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int SortNumber { get; set; }
        /// <summary>
        /// 限制时间
        /// </summary>
        public int? LimitTime { get; set; }
        /// <summary>
        /// 位置信息
        /// </summary>
        public ICollection<WkPointCreateDto> Position { get; set; }
        /// <summary>
        /// 节点窗体类型
        /// </summary>
        public NodeFormType NodeFormType { get; set; }
        public ICollection<WkConditionNodeCreateDto> NextNodes { get; set; }
        public ICollection<WkNodeParaCreateDto> OutcomeSteps { get; set; }
        public ICollection<WkCandidateCreateDto> WkCandidates {  get; set; }
        public ICollection<ApplicationFormCreateDto> ApplicationForms { get; set; }
        public ICollection<WkParamCreateDto> Params { get; set; }
    }
}