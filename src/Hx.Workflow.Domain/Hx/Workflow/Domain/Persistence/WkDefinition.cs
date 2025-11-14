using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkDefinition : FullAuditedAggregateRoot, IMultiTenant
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        public virtual Guid Id { get; protected set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public virtual int Version { get; protected set; }
        /// <summary>
        /// 流程定义名称
        /// </summary>
        public string Title { get; protected set; }
        /// <summary>
        /// 限制时间（分钟）
        /// </summary>
        public int? LimitTime { get; protected set; }
        /// <summary>
        /// get or set group
        /// </summary>
        public Guid? GroupId { get; protected set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; protected set; }
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
        public string BusinessType { get; protected set; }
        /// <summary>
        /// 流程类型
        /// </summary>
        public string ProcessType { get; protected set; }
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; protected set; }
        /// <summary>
        /// 后选者
        /// </summary>
        public ICollection<DefinitionCandidate> WkCandidates { get; protected set; }
        /// <summary>
        /// 节点集合
        /// </summary>
        public virtual ICollection<WkNode> Nodes { get; protected set; }
        
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkDefinition()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        { }
        public WkDefinition(
            Guid id,
            string title,
            int sortNumber,
            string? description,
            string businessType,
            string processType,
            bool isEnabled = true,
            int? limitTime = null,
            Guid? groupId = null,
            Guid? tenantId = null,
            int version = 1)
        {
            Id = id;
            Version = version;
            Title = title;
            GroupId = groupId;
            LimitTime = limitTime;
            IsEnabled = isEnabled;
            Description = description;
            BusinessType = businessType;
            ProcessType = processType;
            SortNumber = sortNumber;
            TenantId = tenantId;
            Nodes = [];
            WkCandidates = [];
        }
        public Task SetVersion(int version)
        {
            Version = version;
            return Task.CompletedTask;
        }
        public Task SetTitle(string title)
        {
            Title = title;
            return Task.CompletedTask;
        }
        public Task SetLimitTime(int? limitTime)
        {
            LimitTime = limitTime;
            return Task.CompletedTask;
        }
        public Task SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            return Task.CompletedTask;
        }
        public Task SetDescription(string? discription)
        {
            Description = discription;
            return Task.CompletedTask;
        }
        public Task SetBusinessType(string businessType)
        {
            BusinessType = businessType;
            return Task.CompletedTask;
        }
        public Task SetProcessType(string processType)
        {
            ProcessType = processType;
            return Task.CompletedTask;
        }
        public Task SetSortNumber(int sortNumber)
        {
            SortNumber = sortNumber;
            return Task.CompletedTask;
        }
        public async Task AddWkNode(WkNode node)
        {
            // 自动设置节点的 WkDefinitionId 和 WkDefinitionVersion
            await node.SetDefinition(Id, Version);
            Nodes.Add(node);
        }
        public Task AddCandidate(DefinitionCandidate input)
        {
            WkCandidates.Add(input);
            return Task.CompletedTask;
        }
        public Task AddCandidates(ICollection<DefinitionCandidate> inputs)
        {
            WkCandidates = inputs;
            return Task.CompletedTask;
        }
        public async Task UpdateNodes(IEnumerable<WkNode> newNodes)
        {
            var nodeComparer = EqualityComparer<Guid>.Default;
            var existingDict = Nodes.ToDictionary(n => n.Name);

            // 删除不存在的节点
            var nodesToRemove = Nodes.Where(existing => !newNodes.Any(newNode => nodeComparer.Equals(newNode.Id, existing.Id))).ToList();
            foreach (var node in nodesToRemove)
            {
                Nodes.Remove(node);
            }

            // 更新或添加节点
            foreach (var newNode in newNodes)
            {
                if (existingDict.TryGetValue(newNode.Name, out var existingNode))
                {
                    // 更新现有节点的属性（不包括 WkDefinitionId 和 WkDefinitionVersion）
                    // 注意：现有节点已经属于当前定义，不需要更新 WkDefinitionId 和 WkDefinitionVersion
                    await existingNode.UpdateFrom(newNode);
                }
                else
                {
                    // 添加新节点（使用 AddWkNode 自动设置 WkDefinitionId 和 WkDefinitionVersion）
                    await AddWkNode(newNode);
                }
            }
        }

        public override object[] GetKeys()
        {
            return [Id, Version];
        }
    }
}
