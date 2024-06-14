using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkNodeCandidate : Candidate
    {
        public WkNodeCandidate()
        { }
        public WkNodeCandidate(Guid candidateId, string userName, string displayUserName, bool defaultSelection = false)
            : base(candidateId, userName, displayUserName, defaultSelection)
        {
        }
    }
}