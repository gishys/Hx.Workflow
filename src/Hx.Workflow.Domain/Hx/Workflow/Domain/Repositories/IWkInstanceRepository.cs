using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkInstanceRepository : IBasicRepository<WkInstance, Guid>
    {
        Task<List<WkInstance>> GetInstancesAsync(string difinitionId, int version);
        Task<List<Guid>> GetRunnableInstancesAsync(DateTime nextExecute);
        Task<IQueryable<WkInstance>> GetDetails(bool tracking = false);
        Task<List<WkInstance>> GetDetails(List<Guid> ids);
        Task<WkExecutionPointer?> GetPointerAsync(Guid pointerId);
        Task<List<WkInstance>> GetMyInstancesAsync(
            ICollection<Guid>? creatorIds,
            ICollection<Guid>? definitionIds,
            IDictionary<string, object>? instanceData,
            ICollection<Guid> id,
            string? reference,
            MyWorkState? state,
            int skipCount,
            int maxResultCount);
            
        /// <summary>
        /// 获取我的实例（支持版本控制）
        /// </summary>
        /// <param name="creatorIds">创建者ID列表</param>
        /// <param name="definitionIds">模板ID列表</param>
        /// <param name="definitionVersions">模板版本列表（可选，null表示所有版本）</param>
        /// <param name="instanceData">实例数据</param>
        /// <param name="ids">用户ID列表</param>
        /// <param name="reference">引用</param>
        /// <param name="state">状态</param>
        /// <param name="skipCount">跳过数量</param>
        /// <param name="maxResultCount">最大结果数量</param>
        /// <returns></returns>
        Task<List<WkInstance>> GetMyInstancesWithVersionAsync(
            ICollection<Guid>? creatorIds,
            ICollection<Guid>? definitionIds,
            ICollection<int>? definitionVersions,
            IDictionary<string, object>? instanceData,
            ICollection<Guid> ids,
            string? reference,
            MyWorkState? state,
            int skipCount,
            int maxResultCount);
            
        /// <summary>
        /// 获取我的实例数量（支持版本控制）
        /// </summary>
        /// <param name="creatorIds">创建者ID列表</param>
        /// <param name="definitionIds">模板ID列表</param>
        /// <param name="definitionVersions">模板版本列表（可选，null表示所有版本）</param>
        /// <param name="instanceData">实例数据</param>
        /// <param name="ids">用户ID列表</param>
        /// <param name="reference">引用</param>
        /// <param name="state">状态</param>
        /// <returns></returns>
        Task<int> GetMyInstancesCountWithVersionAsync(
            ICollection<Guid>? creatorIds,
            ICollection<Guid>? definitionIds,
            ICollection<int>? definitionVersions,
            IDictionary<string, object>? instanceData,
            ICollection<Guid> ids,
            string? reference,
            MyWorkState? state);
            
        /// <summary>
        /// 获取指定模板版本的实例
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        Task<List<WkInstance>> GetInstancesByDefinitionVersionAsync(Guid definitionId, int version);
        
        /// <summary>
        /// 获取运行中的实例（按模板版本）
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        Task<List<WkInstance>> GetRunningInstancesByVersionAsync(Guid definitionId, int version);
        
        /// <summary>
        /// 获取运行中的实例数量（按模板版本）
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns>运行中的实例数量</returns>
        Task<int> GetRunningInstancesCountByVersionAsync(Guid definitionId, int version);
        
        /// <summary>
        /// 获取指定模板版本的所有实例数量（包括已完成和运行中的）
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns>实例数量</returns>
        Task<int> GetInstancesCountByVersionAsync(Guid definitionId, int version);
        Task<int> GetMyInstancesCountAsync(
            ICollection<Guid>? creatorIds,
            ICollection<Guid>? definitionIds,
            IDictionary<string, object>? instanceData,
            ICollection<Guid> ids,
            string? reference,
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
        Task<WkInstance> RecipientExePointerAsync(Guid workflowId, ICurrentUser currentUser, bool isManager);
        Task<WkInstance> RecipientExePointerAsync(Guid workflowId, Guid currentUserId, string recepient, Guid recepientId);
        /// <summary>
        /// 流程实例添加业务数据
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task UpdateDataAsync(Guid workflowId, IDictionary<string, object> data);
        Task<WkInstance?> GetByReferenceAsync(string reference);
        Task<List<ProcessingStatusStat>> GetProcessingStatusStatListAsync(Guid transactorId);
        Task<List<ProcessTypeStat>> GetBusinessTypeListAsync();
        Task<List<ProcessTypeStat>> GetProcessTypeStatListAsync();
        Task<WkInstance?> FindNoTrackAsync(
    Guid id, bool includeDetails = true, CancellationToken cancellation = default);
    }
}