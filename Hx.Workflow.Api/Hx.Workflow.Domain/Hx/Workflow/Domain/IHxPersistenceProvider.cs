using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace Hx.Workflow.Domain
{
    public interface IHxPersistenceProvider : IPersistenceProvider
    {
        Task<WkInstance> GetPersistedWorkflow(Guid id);
        Task<IEnumerable<WkInstance>> GetAllRunnablePersistedWorkflow(string definitionId, int version);
        Task<WkExecutionPointer> GetPersistedExecutionPointer(string id);
        Task<WkDefinition> GetPersistedWorkflowDefinition(string id, int version);
    }
}
