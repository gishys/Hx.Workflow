using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkNode_ApplicationFormsDto
    {
        public Guid NodeId { get; set; }
        public Guid ApplicationId { get; set; }
        public required ApplicationFormDto ApplicationForm { get; set; }
        public int SequenceNumber { get; set; }
        public required ICollection<WkParamDto> Params { get; set; }
    }
}
