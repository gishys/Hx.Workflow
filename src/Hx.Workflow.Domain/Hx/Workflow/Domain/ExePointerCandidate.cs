using Hx.Workflow.Domain.Shared;
using System;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain
{
    public class ExePointerCandidate : Candidate
    {
        public ExePointerCandidate()
        { }
        public ExePointerCandidate(
            Guid candidateId,
            string userName,
            string displayUserName,
            ExePersonnelOperateType exeOperateType,
            ExeCandidateState parentState,
            WkParticipantType executorType,
            bool defaultSelection = false)
            : base(candidateId, userName, displayUserName, executorType, defaultSelection)
        {
            ExeOperateType = exeOperateType;
            ParentState = parentState;
        }
        /// <summary>
        /// 执行操作类型
        /// </summary>
        public ExePersonnelOperateType ExeOperateType { get; protected set; }
        /// <summary>
        /// 执行者状态
        /// </summary>
        public ExeCandidateState ParentState { get; protected set; }
        /// <summary>
        /// 关注
        /// </summary>
        public bool? Follow { get; protected set; }
        public void SetCandidateType(ExePersonnelOperateType exeOperateType)
        {
            ExeOperateType = exeOperateType;
        }
        public void SetParentState(ExeCandidateState parentState)
        {
            ParentState = parentState;
        }
        public Task SetFollow(bool follow)
        {
            Follow = follow;
            return Task.CompletedTask;
        }
    }
}