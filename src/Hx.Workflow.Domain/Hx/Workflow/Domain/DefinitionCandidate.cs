using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class DefinitionCandidate : Candidate
    {
        public DefinitionCandidate()
        { }
        public DefinitionCandidate(Guid candidateId, string userName, string displayUserName, bool defaultSelection = false)
            : base(candidateId, userName, displayUserName, defaultSelection)
        {
        }
    }
}
