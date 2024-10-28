using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

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
            MyWorkState? state,
            int skipCount,
            int maxResultCount);
        Task<int> GetMyInstancesCountAsync(
            ICollection<Guid> ids,
            string reference,
            MyWorkState? state);
        Task<ICollection<ExePointerCandidate>> GetCandidatesAsync(Guid wkInstanceId);
        Task<WkInstance> UpdateCandidateAsync(
            Guid wkinstanceId, Guid executionPointerId, ICollection<ExePointerCandidate> wkCandidates, ExePersonnelOperateType type);
        /// <summary>
        /// 修改候选人办理状态
        /// </summary>
        /// <param name="wkinstanceId"></param>
        /// <param name="executionPointerId"></param>
        /// <param name="parentState"></param>
        /// <returns></returns>
        Task UpdateCandidateAsync(Guid wkinstanceId, Guid executionPointerId, ExeCandidateState parentState);
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
        Task<WkInstance> GetByReferenceAsync(string reference);
    }
}
