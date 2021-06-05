using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkInstanceUpdateDto
    {
        public Guid WkInstanceId { get; set; }
        public Guid WkExecutionPointerId { get; set; }
        public ICollection<WkCandidateUpdateDto> WkCandidates { get; set; }
    }
}
