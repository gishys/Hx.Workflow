﻿using Hx.Workflow.Application.BusinessModule;
using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using SharpYaml;
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
        public WorkflowAppService(
            IWkStepBodyRespository wkStepBody,
            HxWorkflowManager hxWorkflowManager,
            IWkAuditorRespository wkAuditor,
            IWkDefinitionRespository wkDefinition,
            IWkInstanceRepository wkInstanceRepository)
        {
            _wkStepBody = wkStepBody;
            _hxWorkflowManager = hxWorkflowManager;
            _wkAuditor = wkAuditor;
            _wkDefinition = wkDefinition;
            _wkInstanceRepository = wkInstanceRepository;
        }
        /// <summary>
        /// 创建流程模版
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public virtual async Task CreateAsync(WkDefinitionCreateDto input)
        {
            var entity = new WkDefinition(
                    input.Title,
                    input.Icon,
                    input.Color,
                    input.SortNumber,
                    input.Discription,
                    input.BusinessType,
                    input.ProcessType,
                    version: input.Version <= 0 ? 1 : input.Version);
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
            if (userIds?.Count <= 0 && CurrentUser?.Id != null)
            {
                userIds = new List<Guid>
                {
                    (Guid)CurrentUser.Id
                };
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
                var businessData = JsonSerializer.Deserialize<WkInstancePersistData>(instance.Data);
                var pointer = instance.ExecutionPointers.First(d => d.Active);
                var processInstance = new WkProcessInstanceDto
                {
                    EarlyWarning = GetEarlyWarning(businessData, instance),
                    BusinessNumber = instance.BusinessNumber,
                    ProcessName = businessData.ProcessName,
                    Located = businessData.Located,
                    ProcessingStepName = pointer.StepName,
                    ReceivingTime = instance.CreateTime.ToString("t"),
                    State = instance.Status.ToString(),
                    BusinessType = instance.WkDefinition.BusinessType,
                    BusinessCommitmentDeadline = businessData.BusinessCommitmentDeadline.ToString("t"),
                    ProcessType = instance.WkDefinition.ProcessType,
                };
                result.Add(processInstance);
            }
            return new PagedResultDto<WkProcessInstanceDto>(count, result);
        }
        private string GetEarlyWarning(WkInstancePersistData businessData, WkInstance instance)
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
            var resultEntity = await _wkDefinition.UpdateCandidatesAsync(entity.Id, entity.UserId, wkCandidates);
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
                wkCandidates);
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
            return ObjectMapper.Map<ICollection<WkCandidate>, ICollection<WkCandidateDto>>(entitys);
        }
    }
}