using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application
{
    public class WkCandidateCreateDto
    {
        public Guid CandidateId { get; set; }
        public string UserName { get; set; }
        public string DisplayUserName {  get; set; }
        public bool DefaultSelection { get; set; }
    }
}
