using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkCandidate : Entity
    {
        public Guid NodeId { get; set; }
        public Guid CandidateId { get; set; }
        public override object[] GetKeys()
        {
            return new object[] { NodeId, CandidateId };
        }
    }
}
