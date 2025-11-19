using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// WkDefinition 的版本号（用于外键关联，不作为主键）
        /// </summary>
        public virtual int WkDefinitionVersion { get; protected set; }
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
        public virtual ICollection<WkParam> Params { get; protected set; } = [];
        public virtual ICollection<WkNodeMaterials> Materials { get; protected set; } = [];
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
            int sortNumber,
            int? limitTime = null,
            Guid? id = null)
        {
            Name = name;
            DisplayName = displayName;
            StepNodeType = stepNodeType;
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
        public Task SetDisplayName(string displayName)
        {
            DisplayName = displayName;
            return Task.CompletedTask;
        }
        public Task SetStepNodeType(StepNodeType stepNodeType)
        {
            StepNodeType = stepNodeType;
            return Task.CompletedTask;
        }
        public Task SetLimitTime(int? limitTime)
        {
            LimitTime = limitTime;
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
        /// <summary>
        /// 设置节点的定义ID和版本（必须同时设置，因为它们作为外键关联到 WkDefinition）
        /// </summary>
        /// <param name="definitionId">定义ID</param>
        /// <param name="version">定义版本</param>
        public Task SetDefinition(Guid definitionId, int version)
        {
            WkDefinitionId = definitionId;
            WkDefinitionVersion = version;
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
        /// <summary>
        /// 从另一个节点更新属性（不包括 WkDefinitionId 和 WkDefinitionVersion，这些必须通过 SetDefinition 或 AddWkNode 设置）
        /// </summary>
        /// <param name="node">源节点</param>
        public Task UpdateFrom(WkNode node)
        {
            Name = node.Name;
            DisplayName = node.DisplayName;
            LimitTime = node.LimitTime;
            StepNodeType = node.StepNodeType;
            OutcomeSteps = node.OutcomeSteps;
            WkCandidates = node.WkCandidates;
            ApplicationForms = node.ApplicationForms;
            StepBody = node.StepBody;
            Params = node.Params;
            Materials = node.Materials;
            NextNodes = node.NextNodes;
            //UpdateNextNodes(node.NextNodes);
            // 注意：WkDefinitionId 和 WkDefinitionVersion 不在这里更新
            // 它们必须通过 SetDefinition 方法或 AddWkNode 方法设置
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
