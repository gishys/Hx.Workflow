﻿using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWorkflowAppService
    {
        Task<string> StartAsync(StartWorkflowInput input);
        Task StartActivityAsync(string actName, string workflowId, Dictionary<string, object>? data = null);
        Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceAsync(
            ICollection<Guid>? creatorIds = null,
            ICollection<Guid>? definitionIds = null,
            IDictionary<string, object>? instanceData = null,
            MyWorkState? status = null,
            string? reference = null,
            ICollection<Guid>? userIds = null,
            string? queryType = null,
            int skipCount = 0,
            int maxResultCount = 20);
        Task<ICollection<WkCandidateDto>> GetCandidatesAsync(Guid wkInstanceId);
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
        Task UpdateInstanceBusinessDataAsync(Guid workflowId, IDictionary<string, object> data);
        /// <summary>
        /// 关注实例（取消关注）
        /// </summary>
        /// <param name="pointerId"></param>
        /// <param name="follow"></param>
        /// <returns></returns>
        Task FollowAsync(Guid pointerId, bool follow);
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
        Task<List<ProcessingStatusStatDto>> GetProcessingStatusStatListAsync(Guid? transactorId);
        Task<List<ProcessTypeStatDto>> GetBusinessTypeListAsync();
        Task<List<ProcessTypeStatDto>> GetProcessTypeStatListAsync();
        Task AuditAsync(Guid wkInstanceId, Guid executionPointerId, string remark);
        Task<WkAuditorDto> GetAuditAsync(Guid executionPointerId);
        /// <summary>
        /// update execution pointer data
        /// </summary>
        /// <param name="executionPointerId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        Task SaveExecutionPointerDataAsync(Guid executionPointerId, IDictionary<string, object> data);
        Task RetryAsync(Guid executionPointerId);
    }
}