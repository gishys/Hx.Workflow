using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class ExePointerCandidate : Candidate
    {
        public ExePointerCandidate()
        { }
        public ExePointerCandidate(Guid candidateId, string userName, string displayUserName, bool defaultSelection = false)
            : base(candidateId, userName, displayUserName, defaultSelection)
        {
        }
    }
}