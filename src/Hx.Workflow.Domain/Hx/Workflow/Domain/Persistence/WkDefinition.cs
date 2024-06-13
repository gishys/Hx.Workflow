using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkDefinition : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; protected set; }
        /// <summary>
        /// 流程定义名称
        /// </summary>
        public string Title { get; protected set; }
        /// <summary>
        /// 限制时间（分钟）
        /// </summary>
        public int? LimitTime { get; protected set; }
        /// <summary>
        /// 状态
        /// </summary>
        public WkDefinitionState WkDefinitionState { get; protected set; }
        /// <summary>
        /// get or set icon
        /// </summary>
        public string Icon { get; protected set; }
        /// <summary>
        /// get or set color
        /// </summary>
        public string Color { get; protected set; }
        /// <summary>
        /// get or set group
        /// </summary>
        public Guid? GroupId { get; protected set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Discription { get; protected set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int SortNumber { get; protected set; }
        /// <summary>
        /// Get multi tenant id
        /// </summary>
        public Guid? TenantId { get; protected set; }
        /// <summary>
        /// 业务类型
        /// </summary>
        public string BusinessType {  get; protected set; }
        /// <summary>
        /// 流程类型
        /// </summary>
        public string ProcessType {  get; protected set; }
        /// <summary>
        /// 后选者
        /// </summary>
        public ICollection<WkCandidate> WkCandidates { get; protected set; }
        /// <summary>
        /// 节点集合
        /// </summary>
        public virtual ICollection<WkNode> Nodes { get; protected set; }
        public WkDefinition()
        { }
        public WkDefinition(
            string title,
            string icon,
            string color,
            int sortNumber,
            string description,
            string businessType,
            string processType,
            WkDefinitionState wkDefinationState = WkDefinitionState.Enable,
            int? limitTime = null,
            Guid? groupId = null,
            Guid? tenantId = null,
            int version = 1)
        {
            Version = version;
            Title = title;
            Icon = icon;
            Color = color;
            GroupId = groupId;
            LimitTime = limitTime;
            WkDefinitionState = wkDefinationState;
            Discription = description;
            BusinessType = businessType;
            ProcessType = processType;
            SortNumber = sortNumber;
            TenantId = tenantId;
            Nodes = new List<WkNode>();
            WkCandidates = new List<WkCandidate>();
        }
        public Task SetWkDefinitionName(string title)
        {
            Title = title;
            return Task.CompletedTask;
        }
        public Task SetLimitTime(int limitTime)
        {
            LimitTime = limitTime;
            return Task.CompletedTask;
        }
        public Task SetWkDefinitionState(WkDefinitionState wkDefinationState)
        {
            WkDefinitionState = wkDefinationState;
            return Task.CompletedTask;
        }
        public Task SetDiscription(string discription)
        {
            Discription = discription;
            return Task.CompletedTask;
        }
        public Task SetSortNumber(int sortNumber)
        {
            SortNumber = sortNumber;
            return Task.CompletedTask;
        }
        public Task AddWkNode(WkNode node)
        {
            Nodes.Add(node);
            return Task.CompletedTask;
        }
        public Task AddCandidate(WkCandidate input)
        {
            WkCandidates.Add(input);
            return Task.CompletedTask;
        }
        public Task AddCandidates(ICollection<WkCandidate> inputs)
        {
            WkCandidates = inputs;
            return Task.CompletedTask;
        }
    }
}