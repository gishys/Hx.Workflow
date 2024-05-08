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
        public virtual ICollection<WkConNodeCondition> WkConNodeConditions { get; protected set; }
        public WkConditionNode()
        { }
        public WkConditionNode(
            string nextNodeName,
            string label = "")
        {
            Label = label;
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