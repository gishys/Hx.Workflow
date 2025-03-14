using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkNode_ApplicationFormsDto
    {
        public Guid NodeId { get; set; }
        public Guid ApplicationId { get; set; }
        public ApplicationFormDto ApplicationForm { get; set; }
        public int SequenceNumber { get; set; }
        public ICollection<WkParamDto> Params { get; set; }
    }
}
