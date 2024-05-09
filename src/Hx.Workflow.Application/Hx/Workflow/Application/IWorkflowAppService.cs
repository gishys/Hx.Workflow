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
        Task<WkDefinitionDto> GetDefinitionAsync(string name);
        Task<string> StartAsync(StartWorkflowInput input);
        Task StartActivityAsync(string actName, string workflowId, Dictionary<string, object> data = null);
        Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceAsync(
            WorkflowStatus? status = WorkflowStatus.Runnable,
            ICollection<Guid> userIds = null,
            int skipCount = 0,
            int maxResultCount = 20);
        Task<bool> ResumeWorkflowAsync(string workflowId);
        Task<bool> SuspendWorkflowAsync(string workflowId);
        Task<bool> TerminateWorkflowAsync(string workflowId);
        Task<WkDefinitionDto> UpdateDefinitionAsync(WkDefinitionUpdateDto entity);
        Task<ICollection<WkCandidateDto>> GetCandidatesAsync(Guid wkInstanceId);
        Task<List<WkDefinitionDto>> GetDefinitionsAsync();
    }
}