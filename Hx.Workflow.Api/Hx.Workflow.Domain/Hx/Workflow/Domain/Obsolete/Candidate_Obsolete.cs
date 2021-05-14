using System;

namespace Hx.Workflow.Domain
{
    /// <summary>
    /// 候选者关联表
    /// </summary>
    public class Candidate_Obsolete
    {
        /// <summary>
        /// 候选者Id
        /// </summary>
        public Guid CandidateId { get; protected set; }
        /// <summary>
        /// 依附者Id
        /// </summary>
        public Guid DependId { get; protected set; }
    }
}
