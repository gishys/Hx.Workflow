using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkConditionNode : Entity<Guid>
    {
        public virtual Guid WkNodeId { get; protected set; }
        public virtual string? Label { get; protected set; }
        public virtual string NextNodeName { get; protected set; }
        public virtual WkRoleNodeType NodeType { get; protected set; }
        public virtual ICollection<WkConNodeCondition> WkConNodeConditions { get; protected set; }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkConditionNode()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        { }
        public WkConditionNode(
            string nextNodeName,
            WkRoleNodeType nodeType,
            string label = "")
        {
            Label = label;
            NodeType = nodeType;
            NextNodeName = nextNodeName;
            WkConNodeConditions = [];
        }
        public WkConditionNode(
            Guid id,
            string nextNodeName,
            WkRoleNodeType nodeType,
            string label = "")
        {
            Id = id;
            Label = label;
            NodeType = nodeType;
            NextNodeName = nextNodeName;
            WkConNodeConditions = [];
        }
        public Task AddConNodeCondition(WkConNodeCondition condition)
        {
            WkConNodeConditions.Add(condition);
            return Task.CompletedTask;
        }
        public void Update(string nextNodeName, WkRoleNodeType nodeType)
        {
            NextNodeName = nextNodeName;
            NodeType = nodeType;
        }
    }
}