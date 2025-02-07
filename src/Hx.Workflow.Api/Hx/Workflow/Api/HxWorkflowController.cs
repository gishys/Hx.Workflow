using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Shared;
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
        public Task<WkDefinitionDto> GetDefinitionByName(string name)
        {
            return _workflowAppService.GetDefinitionByNameAsync(name);
        }
        [HttpGet]
        [Route("definition/details")]
        public Task<WkDefinitionDto> GetDefinition(Guid id, int version = 1)
        {
            return _workflowAppService.GetDefinitionAsync(id, version);
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
        public Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceAsync(
            MyWorkState? status = null,
            string reference = null,
            ICollection<Guid> userIds = null,
            int skipCount = 0,
            int maxResultCount = 20)
        {
            return _workflowAppService.GetMyWkInstanceAsync(status, reference, userIds, skipCount, maxResultCount);
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
        [HttpGet]
        [Route("workflow/workflowinstanceallnodes")]
        public Task<List<WkNodeTreeDto>> GetInstanceAllNodesAsync(Guid workflowId)
        {
            return _workflowAppService.GetInstanceAllNodesAsync(workflowId);
        }
        [HttpPut]
        [Route("instance/receive")]
        public Task ReceiveInstanceAsync(Guid workflowId)
        {
            return _workflowAppService.RecipientInstanceAsync(workflowId);
        }
        [HttpPut]
        [Route("instance/businessdata")]
        public Task UpdateInstanceBusinessDataAsync(Guid workflowId, [FromBody] IDictionary<string, object> data)
        {
            return _workflowAppService.UpdateInstanceBusinessDataAsync(workflowId, data);
        }
        [HttpPut]
        [Route("instance/follow")]
        public Task FollowAsync(Guid pointerId, bool follow)
        {
            return _workflowAppService.FollowAsync(pointerId, follow);
        }
        [HttpPut]
        [Route("instance/candidate")]
        public Task UpdateInstanceCandidatesAsync(WkInstanceUpdateDto entity)
        {
            return _workflowAppService.UpdateInstanceCandidatesAsync(entity);
        }
        /// <summary>
        /// 通过业务编号或者实例
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("workflow/mywkinstance")]
        public Task<WkCurrentInstanceDetailsDto> GetWkInstanceAsync(string reference)
        {
            return _workflowAppService.GetWkInstanceAsync(reference);
        }
        /// <summary>
        /// 标记初始化物料
        /// </summary>
        /// <param name="wkinstanceId"></param>
        /// <param name="executionPointerId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("workflow/mywkinstance/materials")]
        public Task InitMaterialsAsync(Guid executionPointerId)
        {
            return _workflowAppService.InitMaterialsAsync(executionPointerId);
        }
        /// <summary>
        /// 计算我的工作状态数量
        /// </summary>
        /// <param name="transactorId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("workflow/processingstatusstat")]
        public Task<List<ProcessingStatusStatDto>> GetProcessingStatusStatListAsync(Guid? transactorId)
        {
            return _workflowAppService.GetProcessingStatusStatListAsync(transactorId);
        }
        /// <summary>
        /// 按登记类型统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("workflow/businesstypestat")]
        public Task<List<ProcessTypeStatDto>> GetBusinessTypeListAsync()
        {
            return _workflowAppService.GetBusinessTypeListAsync();
        }
        /// <summary>
        /// 按业务类型统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("workflow/processtypestat")]
        public Task<List<ProcessTypeStatDto>> GetProcessTypeStatListAsync()
        {
            return _workflowAppService.GetProcessTypeStatListAsync();
        }
        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="executionPointerId"></param>
        /// <param name="remark"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("workflow/audit")]
        public Task UpdateAuditAsync(Guid wkInstanceId, Guid executionPointerId, string remark)
        {
            return _workflowAppService.AuditAsync(wkInstanceId, executionPointerId, remark);
        }
        [HttpGet]
        [Route("workflow/audit")]
        public Task<WkAuditorDto> GetAuditAsync(Guid executionPointerId)
        {
            return _workflowAppService.GetAuditAsync(executionPointerId);
        }
    }
}