using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow
{
    public class Candidate : Entity
    {
        public Candidate()
        { }
        public Candidate(
            Guid candidateId, 
            string userName, 
            string displayUserName, 
            WkParticipantType executorType, 
            bool defaultSelection = false)
        {
            CandidateId = candidateId;
            UserName = userName;
            DisplayUserName = displayUserName;
            ExecutorType= executorType;
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
        public Task SetSelection(bool selection)
        {
            DefaultSelection = selection;
            return Task.CompletedTask;
        }
    }
}
