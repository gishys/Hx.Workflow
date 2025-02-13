using System;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkNode_ApplicationForms : Entity
    {
        public WkNode_ApplicationForms() { }
        public Guid NodeId { get; set; }
        public Guid ApplicationId { get; set; }
        public override object?[] GetKeys()
        {
            return [NodeId, ApplicationId];
        }
    }
}
