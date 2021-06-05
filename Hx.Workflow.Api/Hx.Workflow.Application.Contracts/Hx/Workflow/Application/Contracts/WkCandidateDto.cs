using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkCandidateDto
    {
        public Guid CandidateId { get; protected set; }
        public string UserName { get; protected set; }
        public string DisplayUserName { get; protected set; }
    }
}
