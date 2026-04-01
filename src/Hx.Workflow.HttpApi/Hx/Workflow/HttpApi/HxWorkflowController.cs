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
        public Task StartActivity([FromBody] WkActivityInputDto input)
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
            int maxResultCount = 20,
            string? keyword = null)
        {
            return _workflowAppService.GetMyWkInstanceAsync(
                input?.CreatorIds,
                input?.DefinitionIds,
                input?.InstanceData,
                status, reference,
                input?.userIds,
                queryType,
                skipCount,
                maxResultCount,
                keyword);
        }
        
        /// <summary>
        /// 查询我的办理件（支持版本控制）
        /// </summary>
        /// <param name="input">查询输入</param>
        /// <param name="status">状态</param>
        /// <param name="reference">引用</param>
        /// <param name="queryType">查询类型</param>
        /// <param name="skipCount">跳过数量</param>
        /// <param name="maxResultCount">最大结果数量</param>
        /// <returns></returns>
        [HttpGet]
        [Route("workflow/mywkinstances/version")]
        public Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceWithVersionAsync(
            WkGetMyInstancesInput? input = null,
            MyWorkState? status = null,
            string? reference = null,
            string? queryType = null,
            int skipCount = 0,
            int maxResultCount = 20,
            string? keyword = null)
        {
            return _workflowAppService.GetMyWkInstanceWithVersionAsync(
                input?.CreatorIds,
                input?.DefinitionIds,
                input?.DefinitionVersions,
                input?.InstanceData,
                status, reference,
                input?.userIds,
                queryType,
                skipCount,
                maxResultCount,
                keyword);
        }
        
        /// <summary>
        /// 获取指定模板版本的实例
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        [HttpGet]
        [Route("workflow/instances/by-version")]
        public Task<List<WkProcessInstanceDto>> GetInstancesByDefinitionVersionAsync(Guid definitionId, int version)
        {
            return _workflowAppService.GetInstancesByDefinitionVersionAsync(definitionId, version);
        }
        
        /// <summary>
        /// 获取运行中的实例（按模板版本）
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        [HttpGet]
        [Route("workflow/instances/running-by-version")]
        public Task<List<WkProcessInstanceDto>> GetRunningInstancesByVersionAsync(Guid definitionId, int version)
        {
            return _workflowAppService.GetRunningInstancesByVersionAsync(definitionId, version);
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
        /// 实例概览统计：总数、运行中、已完成、已终止、已挂起
        /// </summary>
        /// <param name="input">统计查询参数（时间范围等），为空时默认当年</param>
        /// <returns>概览统计结果</returns>
        [HttpGet]
        [Route("workflow/stats/overview")]
        public Task<InstanceOverviewStatDto> GetStatsOverviewAsync([FromQuery] StatsQueryInput? input = null)
            => _workflowAppService.GetInstanceOverviewStatAsync(input);

        /// <summary>
        /// 流程耗时统计：各模板/业务类型的平均和中位数完成时长
        /// </summary>
        /// <param name="input">支持按模板ID（DefinitionId）和分组方式（GroupBy：Definition/BusinessType）过滤</param>
        /// <returns>耗时统计列表</returns>
        [HttpGet]
        [Route("workflow/stats/duration")]
        public Task<List<DurationStatDto>> GetStatsDurationAsync([FromQuery] StatsQueryInput? input = null)
            => _workflowAppService.GetDurationStatListAsync(input);

        /// <summary>
        /// 超期/SLA 统计：按模板分项，返回各模板的超期数量和超期率
        /// </summary>
        /// <param name="input">支持按模板ID（DefinitionId）和时间范围过滤</param>
        /// <returns>超期统计列表</returns>
        [HttpGet]
        [Route("workflow/stats/overdue")]
        public Task<List<OverdueStatDto>> GetStatsOverdueAsync([FromQuery] StatsQueryInput? input = null)
            => _workflowAppService.GetOverdueStatListAsync(input);

        /// <summary>
        /// 按流程模板统计：各模板的实例总数、运行中、已完成、已终止数量
        /// </summary>
        /// <param name="input">支持时间范围过滤</param>
        /// <returns>按模板分组的统计列表</returns>
        [HttpGet]
        [Route("workflow/stats/by-definition")]
        public Task<List<DefinitionStatDto>> GetStatsByDefinitionAsync([FromQuery] StatsQueryInput? input = null)
            => _workflowAppService.GetDefinitionStatListAsync(input);

        /// <summary>
        /// 按发起人统计：各发起人的发起数量、完成数量和完成率
        /// </summary>
        /// <param name="input">支持按发起人ID（CreatorId）和时间范围过滤</param>
        /// <returns>按发起人分组的统计列表</returns>
        [HttpGet]
        [Route("workflow/stats/by-creator")]
        public Task<List<CreatorStatDto>> GetStatsByCreatorAsync([FromQuery] StatsQueryInput? input = null)
            => _workflowAppService.GetCreatorStatListAsync(input);

        /// <summary>
        /// 错误统计：错误总数、涉及实例数、错误率，以及按模板和步骤的分项统计
        /// </summary>
        /// <param name="input">支持按模板ID（DefinitionId）和时间范围过滤</param>
        /// <returns>错误统计汇总结果</returns>
        [HttpGet]
        [Route("workflow/stats/errors")]
        public Task<ErrorsStatResultDto> GetStatsErrorsAsync([FromQuery] StatsQueryInput? input = null)
            => _workflowAppService.GetErrorsStatAsync(input);

        /// <summary>
        /// 步骤处理时长统计：指定模板各步骤的平均处理时长和经过次数
        /// </summary>
        /// <param name="input">必须提供 DefinitionId 和 Version，支持时间范围过滤</param>
        /// <returns>按步骤分组的时长统计列表</returns>
        [HttpGet]
        [Route("workflow/stats/step-duration")]
        public Task<List<StepDurationStatDto>> GetStatsStepDurationAsync([FromQuery] StepDurationQueryInput input)
            => _workflowAppService.GetStepDurationStatListAsync(input);

        /// <summary>
        /// 趋势统计（时间序列）：按指定时间粒度统计创建数和完成数的变化趋势
        /// </summary>
        /// <param name="input">支持时间范围和粒度（Granularity：day/week/month，默认 month）过滤</param>
        /// <returns>趋势统计列表</returns>
        [HttpGet]
        [Route("workflow/stats/trend")]
        public Task<List<TrendStatDto>> GetStatsTrendAsync([FromQuery] StatsQueryInput? input = null)
            => _workflowAppService.GetTrendStatListAsync(input);

        /// <summary>
        /// 仪表盘汇总统计：概览 + 按模板 Top10 + 超期汇总 + 近5个月趋势
        /// </summary>
        /// <param name="input">支持时间范围过滤，近期趋势固定取截止当前日期的最近5个月</param>
        /// <returns>仪表盘汇总数据</returns>
        [HttpGet]
        [Route("workflow/stats/dashboard")]
        public Task<DashboardStatDto> GetStatsDashboardAsync([FromQuery] StatsQueryInput? input = null)
            => _workflowAppService.GetDashboardStatAsync(input);

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