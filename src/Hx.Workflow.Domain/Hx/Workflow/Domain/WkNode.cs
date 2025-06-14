﻿using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkNode : Entity<Guid>, IHasExtraProperties
    {
        /// <summary>
        /// 步骤体Id
        /// </summary>
        public virtual Guid WkStepBodyId { get; protected set; }
        /// <summary>
        /// 节点Id
        /// </summary>
        public virtual Guid WkDefinitionId { get; protected set; }
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
        public virtual ICollection<WkNodeRelation> NextNodes { get; protected set; }
        /// <summary>
        /// 排序
        /// </summary>
        public virtual int SortNumber { get; protected set; }
        public virtual ICollection<WkParam> Params { get; protected set; } = new List<WkParam>();
        public virtual ICollection<WkNodeMaterials> Materials { get; protected set; } = new List<WkNodeMaterials>();
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNode()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        { }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkNode(
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            string name,
            string displayName,
            StepNodeType stepNodeType,
            int version,
            int sortNumber,
            int? limitTime = null,
            Guid? id = null)
        {
            Name = name;
            DisplayName = displayName;
            StepNodeType = stepNodeType;
            Version = version;
            LimitTime = limitTime;
            SortNumber = sortNumber;
            NextNodes = [];
            OutcomeSteps = [];
            WkCandidates = [];
            ApplicationForms = [];
            Materials = [];
            ExtraProperties = [];
            if (id.HasValue)
            {
                Id = id.Value;
            }
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
        public Task AddNextNode(WkNodeRelation node)
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
            Params = node.Params;
            Materials = node.Materials;
            NextNodes = node.NextNodes;
            //UpdateNextNodes(node.NextNodes);
            return Task.CompletedTask;
        }
        public void UpdateNextNodes(IEnumerable<WkNodeRelation> newNextNodes)
        {
            var existingDict = NextNodes.ToDictionary(n => n.NextNodeName);
            foreach (var newNode in newNextNodes)
            {
                if (existingDict.TryGetValue(newNode.NextNodeName, out var existingNode))
                {
                    // 更新现有节点属性（根据业务需求实现）
                    existingNode.Update(newNode.NextNodeName, newNode.NodeType);
                    existingDict.Remove(newNode.NextNodeName);
                }
                else
                {
                    // 添加新节点（自动生成新ID）
                    NextNodes.Add(newNode);
                }
            }
            // 删除未被匹配的旧节点
            foreach (var removedNode in existingDict.Values)
            {
                NextNodes.Remove(removedNode);
            }
        }
    }
}