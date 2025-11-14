using Hx.Workflow.Domain.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    /// <summary>
    /// 候选人基类（不包含 Version 字段，用于 WkNodeCandidate）
    /// </summary>
    public class CandidateBase : Entity
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public CandidateBase()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        { }
        public CandidateBase(
            Guid candidateId,
            string userName,
            string displayUserName,
            WkParticipantType executorType,
            bool defaultSelection = false)
        {
            CandidateId = candidateId;
            UserName = userName;
            DisplayUserName = displayUserName;
            ExecutorType = executorType;
            DefaultSelection = defaultSelection;
        }
        public virtual Guid NodeId { get; protected set; }
        public virtual Guid CandidateId { get; protected set; }
        public virtual string UserName { get; protected set; }
        public virtual string DisplayUserName { get; protected set; }
        public virtual bool DefaultSelection { get; protected set; }
        /// <summary>
        /// 执行者类型
        /// </summary>
        public virtual WkParticipantType ExecutorType { get; protected set; }
        public override object[] GetKeys()
        {
            return [NodeId, CandidateId];
        }
        public Task SetNodeId(Guid nodeId)
        {
            NodeId = nodeId;
            return Task.CompletedTask;
        }
        public Task SetCandidateId(Guid candidateId)
        {
            CandidateId = candidateId;
            return Task.CompletedTask;
        }
        public Task SetUserName(string userName)
        {
            UserName = userName;
            return Task.CompletedTask;
        }
        public Task SetDisplayUserName(string displayUserName)
        {
            DisplayUserName = displayUserName;
            return Task.CompletedTask;
        }
        public Task SetSelection(bool selection)
        {
            DefaultSelection = selection;
            return Task.CompletedTask;
        }
    }
}

