using Hx.Workflow.Domain.Shared;
using System;

namespace Hx.Workflow.Domain
{
    /// <summary>
    /// 节点候选人（不包含 Version 字段）
    /// </summary>
    public class WkNodeCandidate : CandidateBase
    {
        public WkNodeCandidate()
        { }
        public WkNodeCandidate(
            Guid candidateId,
            string userName,
            string displayUserName,
            WkParticipantType executorType,
            bool defaultSelection = false)
            : base(candidateId, userName, displayUserName, executorType, defaultSelection)
        {
        }
    }
}