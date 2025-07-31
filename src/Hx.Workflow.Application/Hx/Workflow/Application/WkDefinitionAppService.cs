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
    //[Authorize]
    public class WkDefinitionAppService(
        IWkDefinitionRespository definitionRespository,
        IWkStepBodyRespository wkStepBody,
        HxWorkflowManager hxWorkflowManager,
        IWkDefinitionGroupRepository groupRepository,
        IWkInstanceRepository wkInstanceRepository) : WorkflowAppServiceBase, IWkDefinitionAppService
    {
        private readonly IWkDefinitionRespository _definitionRespository = definitionRespository;
        private readonly IWkStepBodyRespository _wkStepBody = wkStepBody;
        private readonly HxWorkflowManager _hxWorkflowManager = hxWorkflowManager;
        private IWkDefinitionGroupRepository GroupRepository { get; } = groupRepository;
        private readonly IWkInstanceRepository _wkInstanceRepository = wkInstanceRepository;

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
                var group = await GroupRepository.FindAsync(input.GroupId.Value) ?? throw new UserFriendlyException(message: $"Id为：{input.GroupId}模板组不存在！");
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
            var nodeEntitys = input.Nodes?.ToWkNodes(GuidGenerator);
            if (nodeEntitys != null)
            {
                foreach (var node in nodeEntitys)
                {
                    var wkStepBodyId = input.Nodes?.FirstOrDefault(d => d.Name == node.Name)?.WkStepBodyId ?? throw new UserFriendlyException(message: $"节点：{node.Name}的WkStepBodyId不存在！");
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
            // 获取最新版本的实体
            var entity = await _definitionRespository.FindAsync(input.Id) ?? throw new UserFriendlyException(message: $"Id为：{input.Id}模板不存在！");
            
            // 版本控制最佳实践
            var newVersion = await GetNextVersionAsync(entity.Id, input.Version);
            
            // 检查是否有正在运行的实例使用当前版本
            var runningInstances = await CheckRunningInstancesAsync(entity.Id, entity.Version);
            if (runningInstances.Count != 0)
            {
                throw new UserFriendlyException(message: $"当前版本有正在运行的实例，无法直接更新。请等待实例完成或创建新版本。");
            }
            
            // 创建新版本而不是修改原版本
            var newEntity = await CreateNewVersionAsync(entity, input, newVersion);
            
            // 保存新版本到数据库
            await _definitionRespository.InsertAsync(newEntity);
            
            // 注册新版本到工作流引擎
            await _hxWorkflowManager.UpdateAsync(newEntity);
            
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(newEntity);
        }
        
        /// <summary>
        /// 获取下一个版本号
        /// </summary>
        private async Task<int> GetNextVersionAsync(Guid definitionId, int requestedVersion)
        {
            if (requestedVersion <= 0)
            {
                // 自动递增版本号
                var maxVersion = await _definitionRespository.GetMaxVersionAsync(definitionId);
                return maxVersion + 1;
            }
            
            // 检查请求的版本号是否已存在
            var exists = await _definitionRespository.ExistsAsync(definitionId, requestedVersion);
            if (exists)
            {
                throw new UserFriendlyException(message: $"版本 {requestedVersion} 已存在，请使用其他版本号。");
            }
            
            return requestedVersion;
        }
        
        /// <summary>
        /// 检查是否有正在运行的实例
        /// </summary>
        private async Task<List<WkInstance>> CheckRunningInstancesAsync(Guid definitionId, int version)
        {
            // 使用注入的IWkInstanceRepository来检查运行中的实例
            return await _wkInstanceRepository.GetRunningInstancesByVersionAsync(definitionId, version);
        }
        
        /// <summary>
        /// 创建新版本
        /// </summary>
        private async Task<WkDefinition> CreateNewVersionAsync(WkDefinition originalEntity, WkDefinitionUpdateDto input, int newVersion)
        {
            var newEntity = new WkDefinition(
                originalEntity.Id, // 使用相同的DefinitionId
                input.Title,
                originalEntity.SortNumber,
                input.Description,
                input.BusinessType,
                input.ProcessType,
                limitTime: input.LimitTime,
                groupId: originalEntity.GroupId,
                version: newVersion);
            
            // 复制节点
            foreach (var node in originalEntity.Nodes)
            {
                var newNode = await CloneNodeAsync(node, newEntity.Id);
                await newEntity.AddWkNode(newNode);
            }
            
            // 更新候选人
            newEntity.WkCandidates.Clear();
            if (input.WkCandidates?.Count > 0)
            {
                foreach (var candidate in input.WkCandidates)
                {
                    newEntity.WkCandidates.Add(new DefinitionCandidate(
                        candidate.CandidateId,
                        candidate.UserName,
                        candidate.DisplayUserName,
                        candidate.ExecutorType,
                        candidate.DefaultSelection,
                        newVersion));
                }
            }
            
            return newEntity;
        }
        
        /// <summary>
        /// 克隆节点
        /// </summary>
        /// <param name="originalNode">原节点</param>
        /// <param name="newDefinitionId">新的定义ID</param>
        /// <returns></returns>
        private async Task<WkNode> CloneNodeAsync(WkNode originalNode, Guid newDefinitionId)
        {
            // 创建新节点，使用新的ID
            var clonedNode = new WkNode(
                originalNode.Name,
                originalNode.DisplayName,
                originalNode.StepNodeType,
                originalNode.Version,
                originalNode.SortNumber,
                originalNode.LimitTime,
                GuidGenerator.Create()); // 生成新的ID

            // 设置新的定义ID
            await clonedNode.SetDefinitionId(newDefinitionId);

            // 复制步骤体
            if (originalNode.StepBody != null)
            {
                await clonedNode.SetWkStepBody(originalNode.StepBody);
            }
            
            // 复制候选人
            foreach (var candidate in originalNode.WkCandidates)
            {
                clonedNode.WkCandidates.Add(new WkNodeCandidate(
                    candidate.CandidateId,
                    candidate.UserName,
                    candidate.DisplayUserName,
                    candidate.ExecutorType,
                    candidate.DefaultSelection,
                    candidate.Version));
            }
            
            // 复制参数
            foreach (var param in originalNode.Params)
            {
                clonedNode.Params.Add(new WkParam(
                    param.WkParamKey,
                    param.Name,
                    param.DisplayName,
                    param.Value));
            }
            
            // 复制材料
            foreach (var material in originalNode.Materials)
            {
                var clonedMaterial = new WkNodeMaterials(
                    material.AttachReceiveType,
                    material.ReferenceType,
                    material.CatalogueName,
                    material.SequenceNumber,
                    material.IsRequired,
                    material.IsStatic,
                    material.IsVerification,
                    material.VerificationPassed);
                
                // 复制子材料
                foreach (var child in material.Children)
                {
                    clonedMaterial.AddChild(new WkNodeMaterials(
                        child.AttachReceiveType,
                        child.ReferenceType,
                        child.CatalogueName,
                        child.SequenceNumber,
                        child.IsRequired,
                        child.IsStatic,
                        child.IsVerification,
                        child.VerificationPassed));
                }
                
                clonedNode.Materials.Add(clonedMaterial);
            }
            
            // 复制下一个节点关系
            foreach (var nextNode in originalNode.NextNodes)
            {
                var clonedNextNode = new WkNodeRelation(
                    nextNode.NextNodeName,
                    nextNode.NodeType,
                    nextNode.Label ?? "");
                
                // 复制规则
                foreach (var rule in nextNode.Rules)
                {
                    clonedNextNode.Rules.Add(new WkNodeRelationRule(
                        rule.Field,
                        rule.Operator,
                        rule.Value));
                }
                
                await clonedNode.AddNextNode(clonedNextNode);
            }
            
            // 复制输出步骤
            foreach (var outcomeStep in originalNode.OutcomeSteps)
            {
                clonedNode.OutcomeSteps.Add(new WkNodePara(
                    outcomeStep.Key,
                    outcomeStep.Value));
            }
            
            // 复制应用表单
            foreach (var appForm in originalNode.ApplicationForms)
            {
                var clonedAppForm = new WkNode_ApplicationForms(
                    appForm.ApplicationId,
                    appForm.SequenceNumber,
                    [],
                    appForm.Version);
                
                // 复制表单参数
                foreach (var param in appForm.Params)
                {
                    clonedAppForm.Params.Add(new WkParam(
                        param.WkParamKey,
                        param.Name,
                        param.DisplayName,
                        param.Value));
                }
                
                clonedNode.ApplicationForms.Add(clonedAppForm);
            }
            
            // 复制扩展属性
            foreach (var extraProperty in originalNode.ExtraProperties)
            {
                clonedNode.ExtraProperties[extraProperty.Key] = extraProperty.Value;
            }
            
            return clonedNode;
        }
        
        /// <summary>
        /// 获取模板的所有版本
        /// </summary>
        public virtual async Task<List<WkDefinitionDto>> GetVersionsAsync(Guid id)
        {
            var versions = await _definitionRespository.GetAllVersionsAsync(id);
            return ObjectMapper.Map<List<WkDefinition>, List<WkDefinitionDto>>(versions);
        }
        
        /// <summary>
        /// 回滚到指定版本
        /// </summary>
        public virtual async Task<WkDefinitionDto> RollbackToVersionAsync(Guid id, int targetVersion)
        {
            var targetDefinition = await _definitionRespository.GetDefinitionAsync(id, targetVersion) ?? throw new UserFriendlyException(message: $"版本 {targetVersion} 不存在！");

            // 检查目标版本是否有正在运行的实例
            var runningInstances = await CheckRunningInstancesAsync(id, targetVersion);
            if (runningInstances.Count != 0)
            {
                throw new UserFriendlyException(message: $"目标版本有正在运行的实例，无法回滚。");
            }
            
            // 获取最新版本号
            var maxVersion = await _definitionRespository.GetMaxVersionAsync(id);
            var newVersion = maxVersion + 1;
            
            // 基于目标版本创建新版本
            var newEntity = await CreateNewVersionAsync(targetDefinition, new WkDefinitionUpdateDto
            {
                Id = id,
                Version = newVersion,
                Title = targetDefinition.Title,
                Description = targetDefinition.Description,
                BusinessType = targetDefinition.BusinessType,
                ProcessType = targetDefinition.ProcessType,
                LimitTime = targetDefinition.LimitTime,
                IsEnabled = targetDefinition.IsEnabled
            }, newVersion);
            
            // 保存新版本到数据库
            await _definitionRespository.InsertAsync(newEntity);
            
            // 注册新版本到工作流引擎
            await _hxWorkflowManager.UpdateAsync(newEntity);
            
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(newEntity);
        }
        
        /// <summary>
        /// 比较两个版本的差异
        /// </summary>
        public virtual async Task<WkDefinitionDiffDto> CompareVersionsAsync(Guid id, int version1, int version2)
        {
            var def1 = await _definitionRespository.GetDefinitionAsync(id, version1);
            var def2 = await _definitionRespository.GetDefinitionAsync(id, version2);
            
            if (def1 == null || def2 == null)
            {
                throw new UserFriendlyException(message: $"指定的版本不存在！");
            }
            
            var diff = new WkDefinitionDiffDto
            {
                Id = id,
                Version1 = version1,
                Version2 = version2
            };
            
            // 比较基本属性
            CompareProperty(diff, "Title", "标题", def1.Title, def2.Title);
            CompareProperty(diff, "Description", "描述", def1.Description, def2.Description);
            CompareProperty(diff, "BusinessType", "业务类型", def1.BusinessType, def2.BusinessType);
            CompareProperty(diff, "ProcessType", "流程类型", def1.ProcessType, def2.ProcessType);
            CompareProperty(diff, "LimitTime", "限制时间", def1.LimitTime?.ToString(), def2.LimitTime?.ToString());
            CompareProperty(diff, "IsEnabled", "是否启用", def1.IsEnabled.ToString(), def2.IsEnabled.ToString());
            
            // 比较节点
            await CompareNodesAsync(diff, def1.Nodes, def2.Nodes);
            
            // 比较候选人
            await CompareCandidatesAsync(diff, def1.WkCandidates, def2.WkCandidates);
            
            return diff;
        }
        
        /// <summary>
        /// 比较属性
        /// </summary>
        private static void CompareProperty(WkDefinitionDiffDto diff, string fieldName, string displayName, string? value1, string? value2)
        {
            if (value1 != value2)
            {
                diff.DiffItems.Add(new WkDefinitionDiffItemDto
                {
                    FieldName = fieldName,
                    FieldDisplayName = displayName,
                    Value1 = value1,
                    Value2 = value2,
                    ChangeType = DiffChangeType.Modified
                });
            }
        }

        /// <summary>
        /// 比较节点
        /// </summary>
#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        private static async Task CompareNodesAsync(WkDefinitionDiffDto diff, ICollection<WkNode> nodes1, ICollection<WkNode> nodes2)
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        {
            // 实现节点比较逻辑
            // 这里需要根据具体的节点结构来实现
        }

        /// <summary>
        /// 比较候选人
        /// </summary>
#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        private static async Task CompareCandidatesAsync(WkDefinitionDiffDto diff, ICollection<DefinitionCandidate> candidates1, ICollection<DefinitionCandidate> candidates2)
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        {
            // 实现候选人比较逻辑
            // 这里需要根据具体的候选人结构来实现
        }
        
        /// <summary>
        /// 获取版本历史
        /// </summary>
        public virtual async Task<List<WkDefinitionVersionHistoryDto>> GetVersionHistoryAsync(Guid id)
        {
            var versions = await _definitionRespository.GetAllVersionsAsync(id);
            var history = new List<WkDefinitionVersionHistoryDto>();
            
            foreach (var version in versions.OrderByDescending(v => v.Version))
            {
                var runningInstances = await CheckRunningInstancesAsync(id, version.Version);
                
                history.Add(new WkDefinitionVersionHistoryDto
                {
                    Version = version.Version,
                    Title = version.Title,
                    CreationTime = version.CreationTime,
                    //CreatorName = version.Creator?.Name,
                    IsEnabled = version.IsEnabled,
                    HasRunningInstances = runningInstances.Count != 0,
                    InstanceCount = runningInstances.Count,
                    // 这里可以添加更多信息，如变更摘要等
                });
            }
            
            return history;
        }
        public async Task<WkStepBody> GetStepBodyByIdAsync(string? id, StepNodeType type)
        {
            if (type == StepNodeType.Start || type == StepNodeType.End)
            {
                WkStepBody? stepBody;
                if (string.IsNullOrEmpty(id))
                {
                    string stepName = type == StepNodeType.Start
                    ? "StartStepBody"
                    : "StopStepBody";
                    stepBody = await _wkStepBody.GetStepBodyAsync(stepName);
                    return stepBody ?? throw new UserFriendlyException(message: $"未找到预定义的 {stepName} 实体。");
                }
                if (!Guid.TryParse(id, out Guid stepBodyId))
                {
                    throw new ArgumentException($"参数 {nameof(id)} 不是有效的 Guid 格式。");
                }
                stepBody = await _wkStepBody.FindAsync(stepBodyId);
                return stepBody ?? throw new UserFriendlyException(message: $"未找到 ID 为 {id} 的 stepbody 实体。");
            }
            if (type == StepNodeType.Activity)
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new UserFriendlyException(message: "Activity 类型必须提供有效的 WkStepBodyId。");
                }
                if (!Guid.TryParse(id, out Guid stepBodyId))
                {
                    throw new ArgumentException($"参数 {nameof(id)} 不是有效的 Guid 格式。");
                }
                WkStepBody? stepBody = await _wkStepBody.FindAsync(stepBodyId);
                return stepBody ?? throw new UserFriendlyException(message: $"未找到 ID 为 {id} 的 stepbody 实体。");
            }
            throw new ArgumentException($"不支持的节点类型：{type}。");
        }
        /// <summary>
        /// 更新模板节点
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<List<WkNodeDto>> UpdateAsync(DefinitionNodeUpdateDto input)
        {
            // 获取最新版本的实体
            var entity = await _definitionRespository.FindAsync(input.Id) ?? throw new UserFriendlyException(message: $"Id为：{input.Id}模板不存在！");
            
            // 版本控制最佳实践
            var newVersion = await GetNextVersionAsync(entity.Id, entity.Version);
            
            // 检查是否有正在运行的实例使用当前版本
            var runningInstances = await CheckRunningInstancesAsync(entity.Id, entity.Version);
            if (runningInstances.Count != 0)
            {
                throw new UserFriendlyException(message: $"当前版本有正在运行的实例，无法直接更新。请等待实例完成或创建新版本。");
            }
            
            // 创建新版本而不是修改原版本
            var newEntity = await CreateNewVersionForNodeUpdateAsync(entity, input, newVersion);
            
            // 保存新版本到数据库
            await _definitionRespository.InsertAsync(newEntity);
            
            // 注册新版本到工作流引擎
            await _hxWorkflowManager.UpdateAsync(newEntity);
            
            return ObjectMapper.Map<List<WkNode>, List<WkNodeDto>>([.. newEntity.Nodes]);
        }
        
        /// <summary>
        /// 为节点更新创建新版本
        /// </summary>
        /// <param name="originalEntity">原实体</param>
        /// <param name="input">更新输入</param>
        /// <param name="newVersion">新版本号</param>
        /// <returns></returns>
        private async Task<WkDefinition> CreateNewVersionForNodeUpdateAsync(WkDefinition originalEntity, DefinitionNodeUpdateDto input, int newVersion)
        {
            var newEntity = new WkDefinition(
                originalEntity.Id, // 使用相同的DefinitionId
                originalEntity.Title,
                originalEntity.SortNumber,
                originalEntity.Description,
                originalEntity.BusinessType,
                originalEntity.ProcessType,
                limitTime: originalEntity.LimitTime,
                groupId: originalEntity.GroupId,
                version: newVersion);
            
            // 复制候选人
            foreach (var candidate in originalEntity.WkCandidates)
            {
                newEntity.WkCandidates.Add(new DefinitionCandidate(
                    candidate.CandidateId,
                    candidate.UserName,
                    candidate.DisplayUserName,
                    candidate.ExecutorType,
                    candidate.DefaultSelection,
                    newVersion));
            }
            
            // 处理节点更新
            var nodeEntitys = input.Nodes.ToWkNodes(GuidGenerator);
            if (nodeEntitys != null && nodeEntitys.Count > 0)
            {
                foreach (var node in nodeEntitys)
                {
                    var inputNode = input.Nodes.FirstOrDefault(d => d.Name == node.Name) ?? throw new UserFriendlyException(message: $"节点：{node.Name}不存在！");
                    await node.SetWkStepBody(await GetStepBodyByIdAsync(inputNode.WkStepBodyId, node.StepNodeType));
                    if (node.ExtraProperties != null)
                    {
                        node.ExtraProperties.Clear();
                        inputNode.ExtraProperties.ForEach(item => node.ExtraProperties.TryAdd(item.Key, item.Value));
                    }
                }
            }
            
            if (nodeEntitys != null && nodeEntitys.Count > 0)
            {
                if (input.RecreateNodes)
                {
                    // 重新创建所有节点
                    foreach (var node in nodeEntitys)
                    {
                        await newEntity.AddWkNode(node);
                    }
                }
                else
                {
                    // 复制原节点并更新
                    var existingNodes = originalEntity.Nodes.ToList();
                    var nodeComparer = EqualityComparer<Guid>.Default;
                    var existingDict = existingNodes.ToDictionary(n => n.Name);
                    
                    // 删除不存在的节点
                    var nodesToRemove = existingNodes.Where(existing => !nodeEntitys.Any(newNode => nodeComparer.Equals(newNode.Id, existing.Id))).ToList();
                    foreach (var node in nodesToRemove)
                    {
                        existingNodes.Remove(node);
                    }
                    
                    // 更新或添加节点
                    foreach (var newNode in nodeEntitys)
                    {
                        if (existingDict.TryGetValue(newNode.Name, out var existingNode))
                        {
                            // 更新现有节点
                            await existingNode.UpdateFrom(newNode);
                            await newEntity.AddWkNode(existingNode);
                        }
                        else
                        {
                            // 添加新节点
                            await newEntity.AddWkNode(newNode);
                        }
                    }
                }
            }
            else
            {
                // 如果没有新节点，复制原节点
                foreach (var node in originalEntity.Nodes)
                {
                    var clonedNode = await CloneNodeAsync(node, newEntity.Id);
                    await newEntity.AddWkNode(clonedNode);
                }
            }
            
            // 验证节点限制时间
            if (newEntity.LimitTime < newEntity.Nodes.Sum(d => d.LimitTime))
            {
                throw new UserFriendlyException(message: "节点限制时间合计值不能大于流程限制时间！");
            }
            
            // 验证节点名称唯一性
            var count = newEntity.Nodes.GroupBy(p => p.Name).Where(g => g.Count() > 1).Select(g => g.Key);
            if (count.Any())
            {
                throw new UserFriendlyException(message: "节点名称{Name}不能重复！");
            }
            
            // 复制扩展属性
            newEntity.ExtraProperties.Clear();
            input.ExtraProperties.ForEach(item => newEntity.ExtraProperties.TryAdd(item.Key, item.Value));
            
            return newEntity;
        }
        /// <summary>
        /// 通过Id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto?> GetAsync(Guid id)
        {
            var entity = await _definitionRespository.FindAsync(id);
            return ObjectMapper.Map<WkDefinition?, WkDefinitionDto?>(entity);
        }
        /// <summary>
        /// 删除实体（删除最新版本）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(Guid id)
        {
            // 获取最新版本
            var latestVersion = await _definitionRespository.FindAsync(id) ?? throw new UserFriendlyException(message: $"模板 {id} 不存在！");

            // 检查是否有正在运行的实例
            var runningInstances = await CheckRunningInstancesAsync(id, latestVersion.Version);
            if (runningInstances.Count != 0)
            {
                throw new UserFriendlyException(message: $"该版本有正在运行的实例，无法删除。请等待实例完成后再删除。");
            }
            
            // 删除最新版本
            await _definitionRespository.DeleteAsync(latestVersion);
        }
        
        /// <summary>
        /// 删除指定版本
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        public virtual async Task DeleteVersionAsync(Guid id, int version)
        {
            // 检查版本是否存在
            var entity = await _definitionRespository.GetDefinitionAsync(id, version) ?? throw new UserFriendlyException(message: $"版本 {version} 不存在！");

            // 检查是否有正在运行的实例使用该版本
            var runningInstances = await CheckRunningInstancesAsync(id, version);
            if (runningInstances.Count != 0)
            {
                throw new UserFriendlyException(message: $"该版本有正在运行的实例，无法删除。请等待实例完成后再删除。");
            }
            
            // 删除指定版本
            await _definitionRespository.DeleteAsync(entity);
        }
        
        /// <summary>
        /// 删除所有版本
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns></returns>
        public virtual async Task DeleteAllVersionsAsync(Guid id)
        {
            // 获取所有版本
            var allVersions = await _definitionRespository.GetAllVersionsAsync(id);
            if (allVersions.Count == 0)
            {
                throw new UserFriendlyException(message: $"模板 {id} 不存在！");
            }
            
            // 检查是否有正在运行的实例
            foreach (var version in allVersions)
            {
                var runningInstances = await CheckRunningInstancesAsync(id, version.Version);
                if (runningInstances.Count != 0)
                {
                    throw new UserFriendlyException(message: $"版本 {version.Version} 有正在运行的实例，无法删除所有版本。请等待实例完成后再删除。");
                }
            }
            
            // 删除所有版本
            foreach (var version in allVersions)
            {
                await _definitionRespository.DeleteAsync(version);
            }
        }
        
        /// <summary>
        /// 删除旧版本（保留指定数量的最新版本）
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="keepCount">保留的最新版本数量</param>
        /// <returns></returns>
        public virtual async Task DeleteOldVersionsAsync(Guid id, int keepCount = 5)
        {
            if (keepCount <= 0)
            {
                throw new UserFriendlyException(message: "保留版本数量必须大于0！");
            }
            
            // 获取所有版本，按版本号降序排列
            var allVersions = await _definitionRespository.GetAllVersionsAsync(id);
            if (allVersions.Count == 0)
            {
                throw new UserFriendlyException(message: $"模板 {id} 不存在！");
            }
            
            // 如果版本数量不超过保留数量，则不需要删除
            if (allVersions.Count <= keepCount)
            {
                return;
            }
            
            // 获取需要删除的旧版本
            var versionsToDelete = allVersions.Skip(keepCount).ToList();
            
            // 检查要删除的版本是否有正在运行的实例
            foreach (var version in versionsToDelete)
            {
                var runningInstances = await CheckRunningInstancesAsync(id, version.Version);
                if (runningInstances.Count != 0)
                {
                    throw new UserFriendlyException(message: $"版本 {version.Version} 有正在运行的实例，无法删除。请等待实例完成后再删除旧版本。");
                }
            }
            
            // 删除旧版本
            foreach (var version in versionsToDelete)
            {
                await _definitionRespository.DeleteAsync(version);
            }
        }
        /// <summary>
        /// 获取流程模版
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> GetDefinitionAsync(Guid id, int version = 1)
        {
            var entity = await _definitionRespository.GetDefinitionAsync(id, version) ?? throw new UserFriendlyException(message: $"Id为：{id}模板不存在！");
            var result = ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
            result.Nodes = [];
            WkNode? node = entity.Nodes.FirstOrDefault(d => d.StepNodeType == StepNodeType.Start);
            while (node != null)
            {
                result.Nodes.Add(ObjectMapper.Map<WkNode, WkNodeDto>(node));
                var condi = node.NextNodes.FirstOrDefault(n => n.NodeType == WkRoleNodeType.Forward);
                if (condi != null)
                    node = entity.Nodes.FirstOrDefault(d => d.Name == condi.NextNodeName);
                else
                    break;
            }
            return result;
        }
    }
}
