using System;

namespace Hx.Workflow.Application.Contracts
{
    public class WkCandidateUpdateDto
    {
        public Guid CandidateId { get; set; }
        public string UserName { get; set; }
        public string DisplayUserName { get; set; }
    }
}