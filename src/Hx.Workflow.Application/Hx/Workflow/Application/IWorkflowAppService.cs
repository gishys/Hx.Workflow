using Hx.Workflow.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using WorkflowCore.Models;

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
            WorkflowStatus? status = WorkflowStatus.Runnable,
            string businessNumber = null,
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
    }
}