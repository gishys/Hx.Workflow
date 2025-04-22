using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace Hx.Workflow.Application
{
    [Authorize]
    public class WkDefinitionAppService : WorkflowAppServiceBase, IWkDefinitionAppService
    {
        private readonly IWkDefinitionRespository _definitionRespository;
        private readonly IWkStepBodyRespository _wkStepBody;
        private readonly HxWorkflowManager _hxWorkflowManager;
        private IWkDefinitionGroupRepository GroupRepository { get; }
        public WkDefinitionAppService(
            IWkDefinitionRespository definitionRespository,
            IWkStepBodyRespository wkStepBody,
            HxWorkflowManager hxWorkflowManager,
            IWkDefinitionGroupRepository groupRepository)
        {
            _definitionRespository = definitionRespository;
            _wkStepBody = wkStepBody;
            _hxWorkflowManager = hxWorkflowManager;
            GroupRepository = groupRepository;
        }
        /// <summary>
        /// 创建模板
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public virtual async Task CreateAsync(WkDefinitionCreateDto input)
        {
            if (input.GroupId.HasValue)
            {
                var group = await GroupRepository.FindAsync(input.GroupId.Value);
                if (group == null)
                {
                    throw new UserFriendlyException("模板组不存在！");
                }
            }
            var sortNumber = await _definitionRespository.GetMaxSortNumberAsync();
            var entity = new WkDefinition(
                GuidGenerator.Create(),
                    input.Title,
                    sortNumber,
                    input.Description,
                    input.BusinessType,
                    input.ProcessType,
                    limitTime: input.LimitTime,
                    groupId: input.GroupId,
                    version: input.Version <= 0 ? 1 : input.Version);
            if (input.WkCandidates != null)
            {
                foreach (var candidate in input.WkCandidates)
                {
                    entity.WkCandidates.Add(new DefinitionCandidate(
                        candidate.CandidateId,
                        candidate.UserName,
                        candidate.DisplayUserName,
                        candidate.ExecutorType,
                        candidate.DefaultSelection));
                }
            }
            var nodeEntitys = input.Nodes?.ToWkNodes();
            if (nodeEntitys != null)
            {
                foreach (var node in nodeEntitys)
                {
                    var wkStepBodyId = input.Nodes.FirstOrDefault(d => d.Name == node.Name)?.WkStepBodyId;
                    await node.SetWkStepBody(await GetStepBodyByIdAsync(wkStepBodyId, node.StepNodeType));
                    await entity.AddWkNode(node);
                }
            }
            entity.ExtraProperties.Clear();
            input.ExtraProperties.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
            await _hxWorkflowManager.CreateAsync(entity);
        }
        /// <summary>
        /// 更新模板
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> UpdateAsync(WkDefinitionUpdateDto input)
        {
            var entity = await _definitionRespository.FindAsync(input.Id);
            await entity.SetVersion(input.Version);
            await entity.SetTitle(input.Title);
            await entity.SetLimitTime(input.LimitTime);
            await entity.SetDescription(input.Description);
            await entity.SetBusinessType(input.BusinessType);
            await entity.SetProcessType(input.ProcessType);
            await entity.SetEnabled(input.IsEnabled);
            entity.WkCandidates.Clear();
            if (input.WkCandidates?.Count > 0)
            {
                foreach (var candidate in input.WkCandidates)
                {
                    entity.WkCandidates.Add(new DefinitionCandidate(
                        candidate.CandidateId,
                        candidate.UserName,
                        candidate.DisplayUserName,
                        candidate.ExecutorType,
                        candidate.DefaultSelection));
                }
            }
            await _hxWorkflowManager.UpdateAsync(entity);
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
        }
        public async Task<WkStepBody> GetStepBodyByIdAsync(string id, StepNodeType type)
        {
            if (type == StepNodeType.Start || type == StepNodeType.End)
            {
                WkStepBody stepBody;
                if (string.IsNullOrEmpty(id))
                {
                    string stepName = type == StepNodeType.Start
                    ? "StartStepBody"
                    : "StopStepBody";
                    stepBody = await _wkStepBody.GetStepBodyAsync(stepName);
                    return stepBody ?? throw new UserFriendlyException($"未找到预定义的 {stepName} 实体。");
                }
                if (!Guid.TryParse(id, out Guid stepBodyId))
                {
                    throw new ArgumentException($"参数 {nameof(id)} 不是有效的 Guid 格式。");
                }
                stepBody = await _wkStepBody.FindAsync(stepBodyId);
                return stepBody ?? throw new UserFriendlyException($"未找到 ID 为 {id} 的 stepbody 实体。");
            }
            if (type == StepNodeType.Activity)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new UserFriendlyException("Activity 类型必须提供有效的 WkStepBodyId。");
                }
                if (!Guid.TryParse(id, out Guid stepBodyId))
                {
                    throw new ArgumentException($"参数 {nameof(id)} 不是有效的 Guid 格式。");
                }
                WkStepBody stepBody = await _wkStepBody.FindAsync(stepBodyId);
                return stepBody ?? throw new UserFriendlyException($"未找到 ID 为 {id} 的 stepbody 实体。");
            }
            throw new ArgumentException($"不支持的节点类型：{type}。");
        }
        /// <summary>
        /// 更新模板
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<List<WkNodeDto>> UpdateAsync(DefinitionNodeUpdateDto input)
        {
            var entity = await _definitionRespository.FindAsync(input.Id);
            var nodeEntitys = input.Nodes.ToWkNodes();
            if (nodeEntitys != null && nodeEntitys.Count > 0)
            {
                foreach (var node in nodeEntitys)
                {
                    var inputNode = input.Nodes.FirstOrDefault(d => d.Name == node.Name);
                    await node.SetWkStepBody(await GetStepBodyByIdAsync(inputNode.WkStepBodyId, node.StepNodeType));
                    if (node.ExtraProperties != null)
                    {
                        node.ExtraProperties.Clear();
                        inputNode.ExtraProperties.ForEach(item => node.ExtraProperties.TryAdd(item.Key, item.Value));
                    }
                }
            }
            if (nodeEntitys != null && nodeEntitys.Count > 0)
                await entity.UpdateNodes(nodeEntitys);
            if (entity.LimitTime < entity.Nodes.Sum(d => d.LimitTime))
            {
                throw new UserFriendlyException("节点限制时间合计值不能大于流程限制时间！");
            }
            var count = entity.Nodes.GroupBy(p => p.Name).Where(g => g.Count() > 1).Select(g => g.Key);
            if (count.Count() > 0)
            {
                throw new UserFriendlyException("节点名称{Name}不能重复！");
            }
            entity.ExtraProperties.Clear();
            input.ExtraProperties.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
            await _hxWorkflowManager.UpdateAsync(entity);
            return ObjectMapper.Map<List<WkNode>, List<WkNodeDto>>(entity.Nodes.ToList());
        }
        /// <summary>
        /// 通过Id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> GetAsync(Guid id)
        {
            var entity = await _definitionRespository.GetAsync(id);
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
        }
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(Guid id)
        {
            await _definitionRespository.DeleteAsync(id);
        }
        /// <summary>
        /// 获取流程模版
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> GetDefinitionAsync(Guid id, int version = 1)
        {
            var entity = await _definitionRespository.GetDefinitionAsync(id, version);
            var result = ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
            result.Nodes = [];
            WkNode node = entity.Nodes.FirstOrDefault(d => d.StepNodeType == StepNodeType.Start);
            while (node != null)
            {
                result.Nodes.Add(ObjectMapper.Map<WkNode, WkNodeDto>(node));
                if (node.NextNodes.Any(n => n.NodeType == WkRoleNodeType.Forward))
                    node = entity.Nodes.FirstOrDefault(d => d.Name == node.NextNodes.First().NextNodeName);
                else
                    break;
            }
            return result;
        }
    }
}