using Hx.Workflow.Application;
using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.Api
{
    [ApiController]
    [Route("hxworkflow")]
    public class HxWorkflowController : AbpController
    {
        private readonly IWkStepBodyAppService _wkStepBodyAppService;
        private readonly IWorkflowAppService _workflowAppService;
        public HxWorkflowController(
            IWkStepBodyAppService wkStepBodyAppService,
            IWorkflowAppService workflowAppService)
        {
            _wkStepBodyAppService = wkStepBodyAppService;
            _workflowAppService = workflowAppService;
        }
        [HttpPost]
        [Route("stepbody")]
        public Task CreateStepBody(WkSepBodyCreateDto input)
        {
            return _wkStepBodyAppService.CreateAsync(input);
        }
        [HttpDelete]
        [Route("stepbody/{name}")]
        public Task DeleteStepBody(string name)
        {
            return _wkStepBodyAppService.DeleteAsync(name);
        }
        [HttpGet]
        [Route("stepbody/{name}")]
        public Task<WkStepBodyDto> GetStepBody(string name)
        {
            return _wkStepBodyAppService.GetStepBodyAsync(name);
        }
        [HttpPost]
        [Route("definition")]
        public Task CreateDefinition(WkDefinitionCreateDto input)
        {
            return _workflowAppService.CreateAsync(input);
        }
        [HttpPut]
        [Route("definition")]
        public Task<WkDefinitionDto> UpdateDefinition(WkDefinitionUpdateDto entity)
        {
            return _workflowAppService.UpdateDefinitionAsync(entity);
        }
        [HttpGet]
        [Route("definition/{name}")]
        public Task<WkDefinitionDto> GetDefinition(string name)
        {
            return _workflowAppService.GetDefinitionAsync(name);
        }
        [HttpGet]
        [Route("definition")]
        public Task<List<WkDefinitionDto>> GetDefinitionsAsync()
        {
            return _workflowAppService.GetDefinitionsAsync();
        }
        [HttpPost]
        [Route("workflow")]
        public Task<string> StartWorkflowAsync([FromBody] StartWorkflowInput input)
        {
            return _workflowAppService.StartAsync(input);
        }
        [HttpPost]
        [Route("workflow/activity")]
        public Task StartActivity(WkActivityInputDto input)
        {
            return _workflowAppService.StartActivityAsync(input.ActivityName, input.WorkflowId, input.Data);
        }
        [HttpPut]
        [Route("workflow/terminate")]
        public Task<bool> TerminateWorkflowAsync(string workflowId)
        {
            return _workflowAppService.TerminateWorkflowAsync(workflowId);
        }
        [HttpPut]
        [Route("workflow/suspend")]
        public Task<bool> SuspendWorkflowAsync(string workflowId)
        {
            return _workflowAppService.SuspendWorkflowAsync(workflowId);
        }
        [HttpPut]
        [Route("workflow/resume")]
        public Task<bool> ResumeWorkflowAsync(string workflowId)
        {
            return _workflowAppService.ResumeWorkflowAsync(workflowId);
        }
        [HttpGet]
        [Route("workflow/mywkinstances")]
        public Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceAsync(Guid[]? ids, string? businessNumber)
        {
            return _workflowAppService.GetMyWkInstanceAsync(userIds: ids, businessNumber: businessNumber);
        }
        [HttpGet]
        [Route("workflow/candidate/{wkInstanceId}")]
        public Task<ICollection<WkCandidateDto>> GetCandidatesAsync(Guid wkInstanceId)
        {
            return _workflowAppService.GetCandidatesAsync(wkInstanceId);
        }
        [HttpGet]
        [Route("workflow/definitionscancreate")]
        public Task<List<WkDefinitionDto>> GetDefinitionsCanCreateAsync()
        {
            return _workflowAppService.GetDefinitionsCanCreateAsync();
        }
        [HttpPut]
        [Route("workflow/recipientinstance")]
        public Task RecipientInstanceAsync(Guid workflowId)
        {
            return _workflowAppService.RecipientInstanceAsync(workflowId);
        }
        [HttpGet]
        [Route("workflow/workflowinstance")]
        public Task<WkCurrentInstanceDetailsDto> GetInstanceAsync(Guid workflowId, Guid? pointerId)
        {
            return _workflowAppService.GetInstanceAsync(workflowId, pointerId);
        }
        [HttpGet]
        [Route("workflow/workflowinstancenodes")]
        public Task<List<WkNodeTreeDto>> GetInstanceNodesAsync(Guid workflowId)
        {
            return _workflowAppService.GetInstanceNodesAsync(workflowId);
        }
    }
}