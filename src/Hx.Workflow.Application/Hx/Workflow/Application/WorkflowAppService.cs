﻿using Hx.Workflow.Application.BusinessModule;
using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.BusinessModule;
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
        public virtual async Task<WkDefinitionDto> GetDefinitionByNameAsync(string name)
        {
            var entity = await _hxWorkflowManager.GetDefinitionAsync(name);
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
        }
        /// <summary>
        /// 获取流程模版
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> GetDefinitionAsync(Guid id, int version = 1)
        {
            var entity = await _wkDefinition.GetDefinitionAsync(id, version);
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
            var eventPointerEventData = JsonSerializer.Deserialize<WkPointerEventData>(JsonSerializer.Serialize(data));
            if (string.IsNullOrEmpty(eventPointerEventData.DecideBranching))
                throw new UserFriendlyException("提交必须携带分支节点名称！");
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
            string reference = null,
            ICollection<Guid> userIds = null,
            int skipCount = 0,
            int maxResultCount = 20)
        {
            if ((userIds == null || userIds.Count == 0) && CurrentUser.Id.HasValue)
            {
                userIds = [CurrentUser.Id.Value];
            }
            //userIds = [new Guid("3a13ccf2-b9db-ebbb-bca6-214b40a79473")];
            List<WkProcessInstanceDto> result = [];
            var instances = await _hxWorkflowManager.WkInstanceRepository.GetMyInstancesAsync(
                userIds,
                reference,
                status,
                skipCount,
                maxResultCount);
            var count = await _wkInstanceRepository.GetMyInstancesCountAsync(userIds, reference, status);
            foreach (var instance in instances)
            {
                var businessData = JsonSerializer.Deserialize<WkInstanceEventData>(instance.Data);
                var pointer = instance.ExecutionPointers.First(d => d.Active || d.Status == PointerStatus.WaitingForEvent);
                var step = instance.WkDefinition.Nodes.First(d => d.Name == pointer.StepName);
                var processInstance = new WkProcessInstanceDto
                {
                    Id = instance.Id,
                    EarlyWarning = GetEarlyWarning(businessData, instance),
                    Reference = instance.Reference,
                    ProcessName = businessData.ProcessName,
                    Located = businessData.Located,
                    ProcessingStepName = step.DisplayName,
                    Recipient = pointer.Recipient,
                    Submitter = pointer.Submitter,
                    ReceivingTime = instance.CreateTime,
                    State = instance.Status.ToString(),
                    BusinessType = instance.WkDefinition.BusinessType,
                    BusinessCommitmentDeadline = businessData.BusinessCommitmentDeadline,
                    ProcessType = instance.WkDefinition.ProcessType,
                    IsSign = instance.Status != WorkflowStatus.Runnable || userIds.Any(id => id == pointer.RecipientId),
                    IsProcessed = pointer.WkSubscriptions.Any(d => d.ExternalToken != null),
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
                if (businessData.BusinessCommitmentDeadline <= DateTime.Now)
                {
                    earlyWarning = "red";
                }
                else if (businessData.BusinessCommitmentDeadline.AddHours(2) <= DateTime.Now)
                {
                    earlyWarning = "yellow";
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
            currentPointerDto.Forms = ObjectMapper.Map<ICollection<ApplicationForm>, ICollection<ApplicationFormDto>>(step.ApplicationForms.OrderBy(d => d.SequenceNumber).ToList());
            currentPointerDto.StepDisplayName = step.DisplayName;
            var errors = await _errorRepository.GetListByIdAsync(workflowId, pointerId);
            currentPointerDto.Errors = ObjectMapper.Map<List<WkExecutionError>, List<WkExecutionErrorDto>>(errors);
            currentPointerDto.Params = ObjectMapper.Map<List<WkParam>, List<WkParamDto>>(step.Params.ToList());
            return new WkCurrentInstanceDetailsDto()
            {
                Id = instance.Id,
                DefinitionId = instance.WkDefinition.Id,
                Reference = instance.Reference,
                Receiver = pointer.Recipient,
                ReceiveTime = pointer.StartTime,
                RegistrationCategory = instance.WkDefinition.BusinessType,
                BusinessCommitmentDeadline = businessData.BusinessCommitmentDeadline,
                CurrentExecutionPointer = currentPointerDto,
                ProcessName = businessData.ProcessName,
                Located = businessData.Located,
                CanHandle = instance.Status == WorkflowStatus.Runnable &&
                (pointer.Active || pointer.Status == PointerStatus.WaitingForEvent)
                && pointer.WkSubscriptions.Any(d => d.ExternalToken == null)
                && CurrentUser.Id.HasValue && pointer.WkCandidates.Any(d => d.CandidateId == CurrentUser.Id),
            };
        }
        public virtual async Task<List<WkNodeTreeDto>> GetInstanceNodesAsync(Guid workflowId)
        {
            var instance = await _wkInstanceRepository.FindAsync(workflowId);
            var result = new List<WkNodeTreeDto>();
            foreach (var node in instance.ExecutionPointers.OrderBy(d => d.StepId))
            {
                result.Add(new WkNodeTreeDto()
                {
                    Key = node.Id,
                    Title = instance.WkDefinition.Nodes.First(d => d.Name == node.StepName).DisplayName,
                    Selected = node.Active || node.Status == PointerStatus.WaitingForEvent,
                    Name = node.StepName,
                    Receiver = node.Recipient
                });
            }
            return result;
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
        /// <summary>
        /// 接收流程实例
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public virtual async Task ReceiveInstanceAsync(Guid workflowId)
        {
            if (!CurrentUser.Id.HasValue)
                throw new UserFriendlyException("未获取到当前登录用户！");
            await _wkInstanceRepository.RecipientExePointerAsync(workflowId, CurrentUser.Id.Value);
        }
        /// <summary>
        /// 流程实例添加业务数据
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task UpdateInstanceBusinessDataAsync(InstanceBusinessDataInput input)
        {
            var data = new Dictionary<string, object>() {
                { "ProcessName", input.ProcessName },
                { "Located", input.Located }
                };
            await _wkInstanceRepository.UpdateDataAsync(input.WorkflowId, data);
        }
    }
}