using Hx.Workflow.Domain.Shared;
using System;

namespace Hx.Workflow.Application.Contracts
{
    public class WkCandidateDto
    {
        public Guid CandidateId { get; set; }
        public required string UserName { get; set; }
        public required string DisplayUserName { get; set; }
        public bool DefaultSelection { get; set; }
        /// <summary>
        /// 执行者类型
        /// </summary>
        public WkParticipantType ExecutorType { get; set; }
    }
}
