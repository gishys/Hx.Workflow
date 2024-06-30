using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkCandidateDto
    {
        public Guid CandidateId { get; set; }
        public string UserName { get; set; }
        public string DisplayUserName { get; set; }
        public bool DefaultSelection { get; set; }
    }
}
