﻿using Hx.Workflow.Application;
using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
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
        [HttpGet]
        [Route("definition/{name}")]
        public Task<WkDefinitionDto> GetDefinition(string name)
        {
            return _workflowAppService.GetDefinitionAsync(name);
        }
        [HttpPost]
        [Route("workflow")]
        public Task StartWorkflowAsync([FromBody] StartWorkflowInput input)
        {
            return _workflowAppService.StartAsync(input);
        }
        [HttpPost]
        [Route("workflow/activity")]
        public Task StartActivity(WkActivityInputDto input)
        {
            return _workflowAppService.StartActivityAsync(input.ActivityName, input.WorkflowId, input.Data);
        }
    }
}