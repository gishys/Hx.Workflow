using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkConditionNode : Entity<Guid>
    {
        public Guid WkNodeId { get; protected set; }
        public string Label { get; protected set; }
        public string NextNodeName { get; protected set; }
        public WkRoleNodeType NodeType { get; protected set; }
        public virtual ICollection<WkConNodeCondition> WkConNodeConditions { get; protected set; }
        public WkConditionNode()
        { }
        public WkConditionNode(
            string nextNodeName,
            WkRoleNodeType nodeType,
            string label = "")
        {
            Label = label;
            NodeType = nodeType;
            NextNodeName = nextNodeName;
            WkConNodeConditions = new List<WkConNodeCondition>();
        }
        public Task AddConNodeCondition(WkConNodeCondition condition)
        {
            WkConNodeConditions.Add(condition);
            return Task.CompletedTask;
        }
    }
}