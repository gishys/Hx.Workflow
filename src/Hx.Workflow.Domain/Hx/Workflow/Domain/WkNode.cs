using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkNode : Entity<Guid>, IHasExtraProperties
    {
        /// <summary>
        /// 步骤体Id
        /// </summary>
        public virtual Guid? WkStepBodyId { get; protected set; }
        /// <summary>
        /// 节点Id
        /// </summary>
        public virtual Guid? WkDefinitionId { get; protected set; }
        /// <summary>
        /// 步骤名称
        /// </summary>
        public virtual string Name { get; protected set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public virtual string DisplayName { get; protected set; }
        /// <summary>
        /// 步骤节点类型
        /// </summary>
        public virtual StepNodeType StepNodeType { get; protected set; }
        /// <summary>
        /// 步骤版本
        /// </summary>
        public virtual int Version { get; protected set; }
        /// <summary>
        /// 限制时间
        /// </summary>
        public virtual int? LimitTime { get; protected set; }
        /// <summary>
        /// 流程参数
        /// </summary>
        public virtual WkStepBody StepBody { get; protected set; }
        /// <summary>
        /// 扩展属性
        /// </summary>
        public virtual ExtraPropertyDictionary ExtraProperties { get; protected set; }
        /// <summary>
        /// 分支节点参数
        /// </summary>
        public virtual ICollection<WkNodePara> OutcomeSteps { get; protected set; }
        /// <summary>
        /// 节点执行者集合
        /// </summary>
        public virtual ICollection<WkNodeCandidate> WkCandidates { get; protected set; }
        /// <summary>
        /// 流程定义
        /// </summary>
        public virtual WkDefinition WkDefinition { get; protected set; }
        /// <summary>
        /// 表单集合
        /// </summary>
        public virtual ICollection<WkNode_ApplicationForms> ApplicationForms { get; protected set; }
        /// <summary>
        /// 节点条件
        /// </summary>
        public virtual ICollection<WkConditionNode> NextNodes { get; protected set; }
        /// <summary>
        /// 排序
        /// </summary>
        public virtual int SortNumber { get; protected set; }
        public virtual ICollection<WkParam> Params { get; protected set; } = new List<WkParam>();
        public virtual ICollection<WkNodeMaterials> Materials { get; protected set; } = new List<WkNodeMaterials>();
        public WkNode()
        { }
        public WkNode(
            string name,
            string displayName,
            StepNodeType stepNodeType,
            int version,
            int sortNumber,
            int? limitTime = null)
        {
            Name = name;
            DisplayName = displayName;
            StepNodeType = stepNodeType;
            Version = version;
            LimitTime = limitTime;
            SortNumber = sortNumber;
            NextNodes = new List<WkConditionNode>();
            OutcomeSteps = new List<WkNodePara>();
            WkCandidates = new List<WkNodeCandidate>();
            ApplicationForms = new List<WkNode_ApplicationForms>();
            Materials = new List<WkNodeMaterials>();
            ExtraProperties = new ExtraPropertyDictionary();
        }
        public WkNode(
            Guid id,
            string name,
            string displayName,
            StepNodeType stepNodeType,
            int version,
            int sortNumber,
            int? limitTime = null)
        {
            Id = id;
            Name = name;
            DisplayName = displayName;
            StepNodeType = stepNodeType;
            Version = version;
            LimitTime = limitTime;
            SortNumber = sortNumber;
            NextNodes = new List<WkConditionNode>();
            OutcomeSteps = new List<WkNodePara>();
            WkCandidates = new List<WkNodeCandidate>();
            ApplicationForms = new List<WkNode_ApplicationForms>();
            Materials = new List<WkNodeMaterials>();
            ExtraProperties = new ExtraPropertyDictionary();
        }
        public Task SetStepName(string name)
        {
            Name = name;
            return Task.CompletedTask;
        }
        public Task AddOutcomeSteps(WkNodePara para)
        {
            OutcomeSteps.Add(para);
            return Task.CompletedTask;
        }
        public Task AddCandidates(WkNodeCandidate para)
        {
            WkCandidates.Add(para);
            return Task.CompletedTask;
        }
        public Task AddMaterials(WkNodeMaterials para)
        {
            Materials.Add(para);
            return Task.CompletedTask;
        }
        public Task AddApplicationForms(Guid applicationId, int sequenceNumber, ICollection<WkParam> ps)
        {
            ApplicationForms.Add(new WkNode_ApplicationForms(applicationId, sequenceNumber, ps));
            return Task.CompletedTask;
        }
        public Task SetWkStepBody(WkStepBody stepBody)
        {
            StepBody = stepBody;
            return Task.CompletedTask;
        }
        public Task AddNextNode(WkConditionNode node)
        {
            NextNodes.Add(node);
            return Task.CompletedTask;
        }
        public Task AddParam(WkParam param)
        {
            Params.Add(param);
            return Task.CompletedTask;
        }
        public Task AddMaterails(WkNodeMaterials materials)
        {
            Materials.Add(materials);
            return Task.CompletedTask;
        }
        public Task UpdateFrom(WkNode node)
        {
            Name = node.Name;
            DisplayName = node.DisplayName;
            LimitTime = node.LimitTime;
            Version = node.Version;
            StepNodeType = node.StepNodeType;
            OutcomeSteps = node.OutcomeSteps;
            WkCandidates = node.WkCandidates;
            ApplicationForms = node.ApplicationForms;
            StepBody = node.StepBody;
            NextNodes = node.NextNodes;
            Params = node.Params;
            Materials = node.Materials;
            return Task.CompletedTask;
        }
    }
}