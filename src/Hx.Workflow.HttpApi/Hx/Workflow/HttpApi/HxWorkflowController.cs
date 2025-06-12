using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Shared;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("hxworkflow")]
    public class HxWorkflowController(
        IWorkflowAppService workflowAppService) : AbpController
    {
        private readonly IWorkflowAppService _workflowAppService = workflowAppService;

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
        [HttpGet]
        [Route("workflow/mywkinstances")]
        public Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceAsync(
            WkGetMyInstancesInput? input = null,
            MyWorkState? status = null,
            string? reference = null,
            string? queryType = null,
            int skipCount = 0,
            int maxResultCount = 20)
        {
            return _workflowAppService.GetMyWkInstanceAsync(
                input?.CreatorIds,
                input?.DefinitionIds,
                input?.InstanceData,
                status, reference,
                input?.userIds,
                queryType,
                skipCount,
                maxResultCount);
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
        public Task UpdateAuditAsync(AuditUpdateDto input)
        {
            return _workflowAppService.AuditAsync(input.WkInstanceId, input.ExecutionPointerId, input.Remark);
        }
        [HttpGet]
        [Route("workflow/audit")]
        public Task<WkAuditorDto> GetAuditAsync(Guid executionPointerId)
        {
            return _workflowAppService.GetAuditAsync(executionPointerId);
        }
        [HttpPut]
        [Route("workflow/data")]
        public Task UpdateAsync(Guid executionPointerId, IDictionary<string, object> data)
        {
            return _workflowAppService.SaveExecutionPointerDataAsync(executionPointerId, data);
        }
        [HttpPut]
        [Route("workflow/retry")]
        public Task RetryAsync(Guid executionPointerId)
        {
            return _workflowAppService.RetryAsync(executionPointerId);
        }
    }
}