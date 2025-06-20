﻿using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Hx.Workflow.Application.Contracts
{
    public class WkNodeCreateDto : ExtensibleObject
    {
        public Guid? Id { get; set; }
        /// <summary>
        /// 步骤名称
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public required string DisplayName { get; set; }
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
        public string? WkStepBodyId { get; set; }
        /// <summary>
        /// 限制时间
        /// </summary>
        public int? LimitTime { get; set; }
        public ICollection<WkConditionNodeCreateDto>? NextNodes { get; set; }
        public ICollection<WkNodeParaCreateDto>? OutcomeSteps { get; set; }
        public ICollection<WkCandidateCreateDto>? WkCandidates { get; set; }
        public ICollection<NodeAddApplicationFormDto>? ApplicationForms { get; set; }
        public ICollection<WkParamCreateDto>? Params { get; set; }
        public ICollection<WkNodeMaterialsDto>? Materials { get; set; }
    }
}