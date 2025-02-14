using System;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkNode_ApplicationForms : Entity
    {
        public WkNode_ApplicationForms() { }
        public WkNode_ApplicationForms(Guid applicationId)
        {
            ApplicationId = applicationId;
        }
        public WkNode_ApplicationForms(Guid nodeId, Guid applicationId)
        {
            NodeId = nodeId;
            ApplicationId = applicationId;
        }
        public Guid NodeId { get; set; }
        public Guid ApplicationId { get; set; }
        public ApplicationForm ApplicationForm { get; set; }
        public override object?[] GetKeys()
        {
            return [NodeId, ApplicationId];
        }
    }
}
