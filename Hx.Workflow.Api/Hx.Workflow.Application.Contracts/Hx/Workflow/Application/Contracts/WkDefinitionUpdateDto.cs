using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkDefinitionUpdateDto
    {
        public Guid Id { get; set; }
        public ICollection<WkCandidateUpdateDto> WkCandidates { get; set; }
        public Guid UserId { get; set; }
    }
}
