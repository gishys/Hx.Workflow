using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class WkCandidate : Entity
    {
        public WkCandidate()
        { }
        public WkCandidate(Guid candidateId, string userName, string displayUserName)
        {
            CandidateId = candidateId;
            UserName = userName;
            DisplayUserName = displayUserName;
        }
        public Guid NodeId { get; protected set; }
        public Guid CandidateId { get; protected set; }
        public string UserName { get; protected set; }
        public string DisplayUserName { get; protected set; }
        public bool DefaultSelection { get; protected set; }
        public override object[] GetKeys()
        {
            return new object[] { NodeId, CandidateId };
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