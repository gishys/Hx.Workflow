using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkNode_ApplicationForms : Entity
    {
        public WkNode_ApplicationForms() { }
        public WkNode_ApplicationForms(Guid applicationId, int sequenceNumber, ICollection<WkParam> ps)
        {
            ApplicationId = applicationId;
            SequenceNumber = sequenceNumber;
            Params = ps;
        }
        public WkNode_ApplicationForms(Guid nodeId, Guid applicationId)
        {
            NodeId = nodeId;
            ApplicationId = applicationId;
        }
        public virtual Guid NodeId { get; protected set; }
        public virtual Guid ApplicationId { get; protected set; }
        public virtual ApplicationForm ApplicationForm { get; protected set; }
        public virtual int SequenceNumber { get; protected set; }
        public virtual ICollection<WkParam> Params { get; protected set; } = new List<WkParam>();
        public override object?[] GetKeys()
        {
            return [NodeId, ApplicationId];
        }
    }
}
