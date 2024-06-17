using Hx.Workflow.Application.BusinessModule;
using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using WorkflowCore.Models;

namespace Hx.Workflow.Application
{
    public class WorkflowAppService : HxWorkflowAppServiceBase, IWorkflowAppService
    {
        private readonly IWkStepBodyRespository _wkStepBody;
        private readonly HxWorkflowManager _hxWorkflowManager;
        private readonly IWkAuditorRespository _wkAuditor;
        private readonly IWkDefinitionRespository _wkDefinition;
        private readonly IWkInstanceRepository _wkInstanceRepository;
        private readonly IWkErrorRepository _errorRepository;
        public WorkflowAppService(
            IWkStepBodyRespository wkStepBody,
            HxWorkflowManager hxWorkflowManager,
            IWkAuditorRespository wkAuditor,
            IWkDefinitionRespository wkDefinition,
            IWkInstanceRepository wkInstanceRepository,
            IWkErrorRepository errorRepository)
        {
            _wkStepBody = wkStepBody;
            _hxWorkflowManager = hxWorkflowManager;
            _wkAuditor = wkAuditor;
            _wkDefinition = wkDefinition;
            _wkInstanceRepository = wkInstanceRepository;
            _errorRepository = errorRepository;
        }
        /// <summary>
        /// 创建流程模版
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public virtual async Task CreateAsync(WkDefinitionCreateDto input)
        {
            var sortNumber = await _wkDefinition.GetMaxSortNumberAsync();
            var entity = new WkDefinition(
                    input.Title,
                    input.Icon,
                    input.Color,
                    sortNumber,
                    input.Discription,
                    input.BusinessType,
                    input.ProcessType,
                    limitTime: input.LimitTime,
                    version: input.Version <= 0 ? 1 : input.Version);
            foreach (var candidate in input.WkCandidates)
            {
                entity.WkCandidates.Add(new DefinitionCandidate(
                    candidate.CandidateId,
                    candidate.UserName,
                    candidate.DisplayUserName,
                    candidate.DefaultSelection));
            }
            var nodeEntitys = input.Nodes.ToWkNodes();
            foreach (var node in nodeEntitys)
            {
                var wkStepBodyId = input.Nodes.FirstOrDefault(d => d.Name == node.Name).WkStepBodyId;
                if (wkStepBodyId?.Length > 0)
                {
                    Guid.TryParse(wkStepBodyId, out Guid guidStepBodyId);
                    var stepBodyEntity = await _wkStepBody.FindAsync(guidStepBodyId);
                    if (stepBodyEntity == null)
                        throw new BusinessException(message: "StepBody没有查询到");
                    await node.SetWkStepBody(stepBodyEntity);
                }
                await entity.AddWkNode(node);
            }
            await _hxWorkflowManager.CreateAsync(entity);
        }
        /// <summary>
        /// 获取流程模版
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> GetDefinitionAsync(string name)
        {
            var entity = await _hxWorkflowManager.GetDefinitionAsync(name);
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
        }
        /// <summary>
        /// 获取全部流程模版
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<WkDefinitionDto>> GetDefinitionsAsync()
        {
            var entitys = await _wkDefinition.GetListAsync(true);
            return ObjectMapper.Map<List<WkDefinition>, List<WkDefinitionDto>>(entitys);
        }
        /// <summary>
        /// 获取可创建的模板（赋予权限）
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public virtual async Task<List<WkDefinitionDto>> GetDefinitionsCanCreateAsync()
        {
            if (CurrentUser.Id.HasValue)
            {
                var entitys = await _wkDefinition.GetListHasPermissionAsync(CurrentUser.Id.Value);
                return ObjectMapper.Map<List<WkDefinition>, List<WkDefinitionDto>>(entitys);
            }
            throw new UserFriendlyException("未获取到当前登录用户！");
        }
        /// <summary>
        /// 通过流程模版Id创建流程
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<string> StartAsync(StartWorkflowInput input)
        {
            return await _hxWorkflowManager.StartWorlflowAsync(input.Id, input.Version, input.Inputs);
        }
        /// <summary>
        /// 开始活动
        /// </summary>
        /// <param name="actName"></param>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task StartActivityAsync(string actName, string workflowId, Dictionary<string, object> data = null)
        {
            await _hxWorkflowManager.StartActivityAsync(actName, workflowId, data);
        }
        /// <summary>
        /// 查询我的办理件（在办、废弃、已完成、挂起），如果为空则查询所有
        /// </summary>
        /// <param name="status"></param>
        /// <param name="userIds"></param>
        /// <param name="skipCount"></param>
        /// <param name="maxResultCount"></param>
        /// <returns></returns>
        public virtual async Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceAsync(
            WorkflowStatus? status = WorkflowStatus.Runnable,
            ICollection<Guid> userIds = null,
            int skipCount = 0,
            int maxResultCount = 20)
        {
            if ((userIds == null || userIds.Count == 0) && CurrentUser.Id.HasValue)
            {
                userIds = [CurrentUser.Id.Value];
            }
            List<WkProcessInstanceDto> result = [];
            var instances = await _hxWorkflowManager.WkInstanceRepository.GetMyInstancesAsync(
                userIds,
                status,
                skipCount,
                maxResultCount);
            var count = await _wkInstanceRepository.GetMyInstancesCountAsync(userIds, status);
            foreach (var instance in instances)
            {
                var businessData = JsonSerializer.Deserialize<WkInstanceEventData>(instance.Data);
                var pointer = instance.ExecutionPointers.First(d => d.Active || d.Status == PointerStatus.WaitingForEvent);
                var step = instance.WkDefinition.Nodes.First(d => d.Name == pointer.StepName);
                var processInstance = new WkProcessInstanceDto
                {
                    EarlyWarning = GetEarlyWarning(businessData, instance),
                    BusinessNumber = instance.BusinessNumber,
                    ProcessName = businessData.ProcessName,
                    Located = businessData.Located,
                    ProcessingStepName = step.DisplayName,
                    Recipient = pointer.Recipient,
                    Submitter = pointer.Submitter,
                    ReceivingTime = instance.CreateTime.ToString("D"),
                    State = instance.Status.ToString(),
                    BusinessType = instance.WkDefinition.BusinessType,
                    BusinessCommitmentDeadline = businessData.BusinessCommitmentDeadline.ToString("D"),
                    ProcessType = instance.WkDefinition.ProcessType,
                };
                result.Add(processInstance);
            }
            return new PagedResultDto<WkProcessInstanceDto>(count, result);
        }
        private string GetEarlyWarning(WkInstanceEventData businessData, WkInstance instance)
        {
            var earlyWarning = "green";
            if (instance.Status == WorkflowStatus.Runnable)
            {
                if (businessData.BusinessCommitmentDeadline.AddHours(2) >= DateTime.Now)
                {
                    earlyWarning = "yellow";
                }
                if (businessData.BusinessCommitmentDeadline >= DateTime.Now)
                {
                    earlyWarning = "red";
                }
            }
            return earlyWarning;
        }
        public virtual async Task RecipientInstanceAsync(Guid workflowId)
        {
            if (CurrentUser.Id.HasValue)
            {
                await _wkInstanceRepository.RecipientExePointerAsync(workflowId, CurrentUser.Id.Value);
            }
            else
            {
                throw new UserFriendlyException("未获取到当前登录用户！");
            }
        }
        public virtual async Task<WkCurrentInstanceDetailsDto> GetInstanceAsync(Guid workflowId, Guid? pointerId)
        {
            var instance = await _wkInstanceRepository.FindAsync(workflowId);
            var businessData = JsonSerializer.Deserialize<WkInstanceEventData>(instance.Data);
            WkExecutionPointer pointer;
            if (pointerId.HasValue)
            {
                pointer = instance.ExecutionPointers.First(d => d.Id == pointerId.Value);
            }
            else
            {
                pointer = instance.ExecutionPointers.First(d => d.Active || d.Status == PointerStatus.WaitingForEvent);
            }
            var step = instance.WkDefinition.Nodes.First(d => d.Name == pointer.StepName);
            var currentPointerDto = ObjectMapper.Map<WkExecutionPointer, WkExecutionPointerDto>(pointer);
            currentPointerDto.Forms = ObjectMapper.Map<ICollection<ApplicationForm>, ICollection<ApplicationFormDto>>(step.ApplicationForms);
            currentPointerDto.StepDisplayName = step.DisplayName;
            var errors = await _errorRepository.GetListByIdAsync(workflowId, pointerId);
            currentPointerDto.Errors = ObjectMapper.Map<List<WkExecutionError>, List<WkExecutionErrorDto>>(errors);
            return new WkCurrentInstanceDetailsDto()
            {
                Id = instance.Id,
                BusinessNumber = instance.BusinessNumber,
                Receiver = pointer.Recipient,
                ReceiveTime = pointer.StartTime?.ToString("D"),
                RegistrationCategory = instance.WkDefinition.BusinessType,
                BusinessCommitmentDeadline = businessData.BusinessCommitmentDeadline.ToString("D"),
                CurrentExecutionPointer = currentPointerDto,
            };
        }
        public virtual async Task<List<WkNodeTreeDto>> GetInstanceNodesAsync(Guid workflowId)
        {
            if (CurrentUser.Id.HasValue)
            {
                var instance = await _wkInstanceRepository.FindAsync(workflowId);
                var pointer = instance.ExecutionPointers.OrderBy(d => d.StepId).First(d => d.Active || d.Status == PointerStatus.WaitingForEvent);
                return instance.ExecutionPointers.Select(
                    d => new WkNodeTreeDto()
                    {
                        Key = d.Id,
                        Title = d.StepName,
                        Selected = d.Active
                    })
                    .ToList();
            }
            else
            {
                throw new UserFriendlyException("未获取到当前登录用户！");
            }
        }
        /// <summary>
        /// 终止工作流
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> TerminateWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.TerminateWorkflowAsync(workflowId);
        }
        /// <summary>
        /// 挂起工作流
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> SuspendWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.SuspendWorkflowAsync(workflowId);
        }
        /// <summary>
        /// 恢复工作流
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> ResumeWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.ResumeWorkflowAsync(workflowId);
        }
        /// <summary>
        /// 更新模板定义候选人
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> UpdateDefinitionAsync(WkDefinitionUpdateDto entity)
        {
            var wkCandidates = entity.WkCandidates.ToWkCandidate();
            var resultEntity = await _wkDefinition.UpdateCandidatesAsync(entity.Id, entity.UserId, wkCandidates as ICollection<DefinitionCandidate>);
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(resultEntity);
        }
        /// <summary>
        /// 更新实例候选人
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<WkInstancesDto> UpdateInstanceAsync(WkInstanceUpdateDto entity)
        {
            var wkCandidates = entity.WkCandidates.ToWkCandidate();
            var resultEntity = await _wkInstanceRepository.UpdateCandidateAsync(
                entity.WkInstanceId,
                entity.WkExecutionPointerId,
                wkCandidates as ICollection<ExePointerCandidate>);
            return ObjectMapper.Map<WkInstance, WkInstancesDto>(resultEntity);
        }
        /// <summary>
        /// 通过实例Id获取可选择的人员
        /// </summary>
        /// <param name="wkInstanceId"></param>
        /// <returns></returns>
        public virtual async Task<ICollection<WkCandidateDto>> GetCandidatesAsync(Guid wkInstanceId)
        {
            var entitys = await _wkInstanceRepository.GetCandidatesAsync(wkInstanceId);
            return ObjectMapper.Map<ICollection<ExePointerCandidate>, ICollection<WkCandidateDto>>(entitys);
        }
    }
}