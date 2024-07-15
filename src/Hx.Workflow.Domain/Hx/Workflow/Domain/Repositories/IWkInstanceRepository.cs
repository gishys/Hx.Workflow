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
            string reference,
            WorkflowStatus? status,
            int skipCount,
            int maxResultCount);
        Task<int> GetMyInstancesCountAsync(
            ICollection<Guid> ids,
            string reference,
            WorkflowStatus? status);
        Task<ICollection<ExePointerCandidate>> GetCandidatesAsync(Guid wkInstanceId);
        Task<WkInstance> UpdateCandidateAsync(
            Guid wkinstanceId, Guid executionPointerId, ICollection<ExePointerCandidate> wkCandidates);
        /// <summary>
        /// 获取当天编号最大值
        /// </summary>
        /// <returns></returns>
        Task<int> GetMaxNumberAsync();
        Task<WkInstance> RecipientExePointerAsync(Guid workflowId, Guid currentUserId);
        /// <summary>
        /// 流程实例添加业务数据
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task UpdateDataAsync(Guid workflowId, IDictionary<string, object> data);
    }
}
