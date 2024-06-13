using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkInstanceRepository : IBasicRepository<WkInstance, Guid>
    {
        Task<List<WkInstance>> GetInstancesAsync(string difinitionId, int version);
        Task<List<Guid>> GetRunnableInstancesAsync(DateTime nextExecute);
        Task<IQueryable<WkInstance>> GetDetails(bool tracking = false);
        Task<List<WkInstance>> GetDetails(List<Guid> ids);
        Task<WkExecutionPointer> GetPointerAsync(Guid pointerId);
        Task<List<WkInstance>> GetMyInstancesAsync(
            ICollection<Guid> id,
            WorkflowStatus? status,
            int skipCount,
            int maxResultCount);
        Task<int> GetMyInstancesCountAsync(
            ICollection<Guid> ids,
            WorkflowStatus? status);
        Task<ICollection<WkCandidate>> GetCandidatesAsync(Guid wkInstanceId);
        Task<WkInstance> UpdateCandidateAsync(
            Guid wkinstanceId, Guid executionPointerId, ICollection<WkCandidate> wkCandidates);
        /// <summary>
        /// 获取当天编号最大值
        /// </summary>
        /// <returns></returns>
        Task<int> GetMaxNumberAsync();
        Task<WkInstance> RecipientExePointerAsync(Guid workflowId, Guid currentUserId);
    }
}
