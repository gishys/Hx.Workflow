using Hx.Workflow.Domain.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    /// <summary>
    /// 流程定义候选人（包含 Version 字段作为主键的一部分）
    /// </summary>
    public class DefinitionCandidate : Entity
    {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        public DefinitionCandidate()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        { }
        public DefinitionCandidate(
            Guid candidateId,
            string userName, 
            string displayUserName,
            WkParticipantType executorType,
            bool defaultSelection = false,
            int version = 1)
        {
            CandidateId = candidateId;
            UserName = userName;
            DisplayUserName = displayUserName;
            ExecutorType = executorType;
            DefaultSelection = defaultSelection;
            Version = version;
        }
        /// <summary>
        /// WkDefinition 的 ID（NodeId 实际上是 WkDefinition 的 Id）
        /// </summary>
        public virtual Guid NodeId { get; protected set; }
        public virtual Guid CandidateId { get; protected set; }
        public virtual string UserName { get; protected set; }
        public virtual string DisplayUserName { get; protected set; }
        public virtual bool DefaultSelection { get; protected set; }
        /// <summary>
        /// 执行者类型
        /// </summary>
        public virtual WkParticipantType ExecutorType { get; protected set; }
        /// <summary>
        /// WkDefinition 的版本号（作为主键的一部分）
        /// </summary>
        public virtual int Version { get; protected set; }
        public override object[] GetKeys()
        {
            return [NodeId, CandidateId, Version];
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
        public Task SetVersion(int version)
        {
            Version = version;
            return Task.CompletedTask;
        }
    }
}
