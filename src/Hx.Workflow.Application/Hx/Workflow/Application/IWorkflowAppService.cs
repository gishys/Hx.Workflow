using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.Stats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application
{
    public interface IWorkflowAppService
    {
        Task CreateAsync(WkDefinitionCreateDto input);
        Task<WkDefinitionDto> GetDefinitionAsync(Guid id, int version = 1);
        Task<WkDefinitionDto> GetDefinitionByNameAsync(string name);
        Task<string> StartAsync(StartWorkflowInput input);
        Task StartActivityAsync(string actName, string workflowId, Dictionary<string, object> data = null);
        Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceAsync(
            MyWorkState? status = null,
            string reference = null,
            ICollection<Guid> userIds = null,
            int skipCount = 0,
            int maxResultCount = 20);
        Task<bool> ResumeWorkflowAsync(string workflowId);
        Task<bool> SuspendWorkflowAsync(string workflowId);
        Task<bool> TerminateWorkflowAsync(string workflowId);
        Task<WkDefinitionDto> UpdateDefinitionAsync(WkDefinitionUpdateDto entity);
        Task<ICollection<WkCandidateDto>> GetCandidatesAsync(Guid wkInstanceId);
        Task<List<WkDefinitionDto>> GetDefinitionsAsync();
        /// <summary>
        /// 获取可创建的模板（赋予权限）
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        Task<List<WkDefinitionDto>> GetDefinitionsCanCreateAsync();
        /// <summary>
        /// 接收实例
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        Task RecipientInstanceAsync(Guid workflowId);
        Task<WkCurrentInstanceDetailsDto> GetInstanceAsync(Guid workflowId, Guid? pointerId);
        Task<List<WkNodeTreeDto>> GetInstanceNodesAsync(Guid workflowId);
        /// <summary>
        /// 流程实例添加业务数据
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task UpdateInstanceBusinessDataAsync(InstanceBusinessDataInput input);
        /// <summary>
        /// 关注实例（取消关注）
        /// </summary>
        /// <param name="pointerId"></param>
        /// <param name="follow"></param>
        /// <returns></returns>
        Task FollowAsync(Guid pointerId, bool follow);
        /// <summary>
        /// 更新实例候选人（委托、抄送、会签）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<WkInstancesDto> UpdateInstanceCandidatesAsync(WkInstanceUpdateDto entity);
        Task<List<WkNodeTreeDto>> GetInstanceAllNodesAsync(Guid workflowId);
        /// <summary>
        /// 通过业务编号或者实例
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        Task<WkCurrentInstanceDetailsDto> GetWkInstanceAsync(string reference);
        /// <summary>
        /// 标记初始化物料
        /// </summary>
        /// <param name="wkinstanceId"></param>
        /// <param name="executionPointerId"></param>
        /// <returns></returns>
        Task InitMaterialsAsync(Guid executionPointerId);
        /// <summary>
        /// 计算我的工作状态数量
        /// </summary>
        /// <param name="transactorId"></param>
        /// <returns></returns>
        Task<List<ProcessingStatusStat>> GetProcessingStatusStatListAsync(Guid? transactorId);
        Task<List<ProcessTypeStat>> GetBusinessTypeListAsync();
        Task<List<ProcessTypeStat>> GetProcessTypeStatListAsync();
        Task AuditAsync(Guid wkInstanceId, Guid executionPointerId, string remark);
        Task<WkAuditorDto> GetAuditAsync(Guid executionPointerId);
    }
}