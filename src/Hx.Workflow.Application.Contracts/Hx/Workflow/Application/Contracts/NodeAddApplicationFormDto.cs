using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class NodeAddApplicationFormDto
    {
        public Guid ApplicationFormId { get; set; }
        public int SequenceNumber {  get; set; }
        public ICollection<WkParamCreateDto> Params { get; set; }
    }
}
