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
        /// 注意：此方法只更新基本信息（Title, Description等），不涉及节点变化，因此直接更新当前版本，不创建新版本
        /// 只有当节点发生变化时（通过 UpdateAsync(DefinitionNodeUpdateDto)），才会创建新版本
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> UpdateAsync(WkDefinitionUpdateDto input)
        {
            // 获取最新版本的实体
            var entity = await _definitionRespository.FindAsync(input.Id) ?? throw new UserFriendlyException(message: $"Id为：{input.Id}模板不存在！");
            
            // 更新基本信息（不涉及节点变化，直接更新当前版本）
            if (!string.Equals(entity.Title, input.Title, StringComparison.OrdinalIgnoreCase))
            {
                await entity.SetTitle(input.Title);
            }
            
            if (!string.Equals(entity.Description, input.Description, StringComparison.OrdinalIgnoreCase))
            {
                await entity.SetDescription(input.Description);
            }
            
            if (!string.Equals(entity.BusinessType, input.BusinessType, StringComparison.OrdinalIgnoreCase))
            {
                await entity.SetBusinessType(input.BusinessType);
            }
            
            if (!string.Equals(entity.ProcessType, input.ProcessType, StringComparison.OrdinalIgnoreCase))
            {
                await entity.SetProcessType(input.ProcessType);
            }
            
            if (entity.LimitTime != input.LimitTime)
            {
                await entity.SetLimitTime(input.LimitTime);
            }
            
            if (entity.IsEnabled != input.IsEnabled)
            {
                await entity.SetEnabled(input.IsEnabled);
            }
            
            // 更新候选人
            if (input.WkCandidates != null)
            {
                entity.WkCandidates.Clear();
                foreach (var candidate in input.WkCandidates)
                {
                    entity.WkCandidates.Add(new DefinitionCandidate(
                        candidate.CandidateId,
                        candidate.UserName,
                        candidate.DisplayUserName,
                        candidate.ExecutorType,
                        candidate.DefaultSelection,
                        entity.Version));
                }
            }
            
            // 保存更新
            await _definitionRespository.UpdateAsync(entity);
            
            // 重新注册到工作流引擎（因为基本信息可能影响显示）
            await _hxWorkflowManager.UpdateAsync(entity);
            
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
        }
        
        /// <summary>
        /// 获取下一个版本号
        /// </summary>
        private async Task<int> GetNextVersionAsync(Guid definitionId, int requestedVersion)
        {
            // 获取当前最大版本号
            var maxVersion = await _definitionRespository.GetMaxVersionAsync(definitionId);
            
            if (requestedVersion <= 0)
            {
                // 自动递增版本号
                return maxVersion + 1;
            }
            
            // 如果请求的版本号小于等于当前最大版本号，自动使用最大版本号+1
            // 如果请求的版本号大于当前最大版本号，使用请求的版本号
            if (requestedVersion <= maxVersion)
            {
                return maxVersion + 1;
            }
            
            return requestedVersion;
        }
        
        /// <summary>
        /// 检查指定版本是否有正在运行的实例
        /// 注意：仅在删除版本时需要检查，创建新版本时不需要检查（因为新版本不会影响正在运行的旧版本实例）
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns>正在运行的实例数量</returns>
        private async Task<int> CheckRunningInstancesAsync(Guid definitionId, int version)
        {
            return await _wkInstanceRepository.GetRunningInstancesCountByVersionAsync(definitionId, version);
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
        /// 注意：回滚实际上是基于目标版本创建新版本，不会影响正在运行的旧版本实例，因此不需要检查运行中的实例
        /// </summary>
        public virtual async Task<WkDefinitionDto> RollbackToVersionAsync(Guid id, int targetVersion)
        {
            var targetDefinition = await _definitionRespository.GetDefinitionAsync(id, targetVersion) ?? throw new UserFriendlyException(message: $"版本 {targetVersion} 不存在！");
            
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
        private static Task CompareNodesAsync(WkDefinitionDiffDto diff, ICollection<WkNode> nodes1, ICollection<WkNode> nodes2)
        {
            var nodes1Dict = nodes1?.ToDictionary(n => n.Id) ?? [];
            var nodes2Dict = nodes2?.ToDictionary(n => n.Id) ?? [];
            
            // 检查新增的节点（在 nodes2 中但不在 nodes1 中）
            foreach (var node2 in nodes2Dict.Values)
            {
                if (!nodes1Dict.ContainsKey(node2.Id))
                {
                    diff.DiffItems.Add(new WkDefinitionDiffItemDto
                    {
                        FieldName = $"Node_{node2.Id}",
                        FieldDisplayName = $"节点[{node2.Name}]",
                        Value1 = null,
                        Value2 = $"{node2.DisplayName} ({node2.StepNodeType})",
                        ChangeType = DiffChangeType.Added
                    });
                }
            }
            
            // 检查删除的节点（在 nodes1 中但不在 nodes2 中）
            foreach (var node1 in nodes1Dict.Values)
            {
                if (!nodes2Dict.ContainsKey(node1.Id))
                {
                    diff.DiffItems.Add(new WkDefinitionDiffItemDto
                    {
                        FieldName = $"Node_{node1.Id}",
                        FieldDisplayName = $"节点[{node1.Name}]",
                        Value1 = $"{node1.DisplayName} ({node1.StepNodeType})",
                        Value2 = null,
                        ChangeType = DiffChangeType.Removed
                    });
                }
            }
            
            // 检查修改的节点（在两个集合中都存在但属性不同）
            foreach (var node1 in nodes1Dict.Values)
            {
                if (nodes2Dict.TryGetValue(node1.Id, out var node2))
                {
                    // 比较节点名称
                    if (node1.Name != node2.Name)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Node_{node1.Id}_Name",
                            FieldDisplayName = $"节点[{node1.Name}]名称",
                            Value1 = node1.Name,
                            Value2 = node2.Name,
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                    
                    // 比较显示名称
                    if (node1.DisplayName != node2.DisplayName)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Node_{node1.Id}_DisplayName",
                            FieldDisplayName = $"节点[{node1.Name}]显示名称",
                            Value1 = node1.DisplayName,
                            Value2 = node2.DisplayName,
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                    
                    // 比较节点类型
                    if (node1.StepNodeType != node2.StepNodeType)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Node_{node1.Id}_StepNodeType",
                            FieldDisplayName = $"节点[{node1.Name}]节点类型",
                            Value1 = node1.StepNodeType.ToString(),
                            Value2 = node2.StepNodeType.ToString(),
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                    
                    // 比较限制时间
                    if (node1.LimitTime != node2.LimitTime)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Node_{node1.Id}_LimitTime",
                            FieldDisplayName = $"节点[{node1.Name}]限制时间",
                            Value1 = node1.LimitTime?.ToString() ?? "无",
                            Value2 = node2.LimitTime?.ToString() ?? "无",
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                    
                    // 比较 StepBody ID
                    if (node1.WkStepBodyId != node2.WkStepBodyId)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Node_{node1.Id}_WkStepBodyId",
                            FieldDisplayName = $"节点[{node1.Name}]步骤体ID",
                            Value1 = node1.WkStepBodyId.ToString(),
                            Value2 = node2.WkStepBodyId.ToString(),
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                    
                    // 比较排序号
                    if (node1.SortNumber != node2.SortNumber)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Node_{node1.Id}_SortNumber",
                            FieldDisplayName = $"节点[{node1.Name}]排序号",
                            Value1 = node1.SortNumber.ToString(),
                            Value2 = node2.SortNumber.ToString(),
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                }
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// 比较候选人
        /// </summary>
        private static Task CompareCandidatesAsync(WkDefinitionDiffDto diff, ICollection<DefinitionCandidate> candidates1, ICollection<DefinitionCandidate> candidates2)
        {
            var candidates1Dict = candidates1?.ToDictionary(c => c.CandidateId) ?? [];
            var candidates2Dict = candidates2?.ToDictionary(c => c.CandidateId) ?? [];
            
            // 检查新增的候选人（在 candidates2 中但不在 candidates1 中）
            foreach (var candidate2 in candidates2Dict.Values)
            {
                if (!candidates1Dict.ContainsKey(candidate2.CandidateId))
                {
                    diff.DiffItems.Add(new WkDefinitionDiffItemDto
                    {
                        FieldName = $"Candidate_{candidate2.CandidateId}",
                        FieldDisplayName = $"候选人[{candidate2.DisplayUserName}]",
                        Value1 = null,
                        Value2 = $"{candidate2.DisplayUserName} ({candidate2.UserName}) - {candidate2.ExecutorType}",
                        ChangeType = DiffChangeType.Added
                    });
                }
            }
            
            // 检查删除的候选人（在 candidates1 中但不在 candidates2 中）
            foreach (var candidate1 in candidates1Dict.Values)
            {
                if (!candidates2Dict.ContainsKey(candidate1.CandidateId))
                {
                    diff.DiffItems.Add(new WkDefinitionDiffItemDto
                    {
                        FieldName = $"Candidate_{candidate1.CandidateId}",
                        FieldDisplayName = $"候选人[{candidate1.DisplayUserName}]",
                        Value1 = $"{candidate1.DisplayUserName} ({candidate1.UserName}) - {candidate1.ExecutorType}",
                        Value2 = null,
                        ChangeType = DiffChangeType.Removed
                    });
                }
            }
            
            // 检查修改的候选人（在两个集合中都存在但属性不同）
            foreach (var candidate1 in candidates1Dict.Values)
            {
                if (candidates2Dict.TryGetValue(candidate1.CandidateId, out var candidate2))
                {
                    // 比较用户名
                    if (candidate1.UserName != candidate2.UserName)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Candidate_{candidate1.CandidateId}_UserName",
                            FieldDisplayName = $"候选人[{candidate1.DisplayUserName}]用户名",
                            Value1 = candidate1.UserName,
                            Value2 = candidate2.UserName,
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                    
                    // 比较显示用户名
                    if (candidate1.DisplayUserName != candidate2.DisplayUserName)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Candidate_{candidate1.CandidateId}_DisplayUserName",
                            FieldDisplayName = $"候选人[{candidate1.DisplayUserName}]显示名称",
                            Value1 = candidate1.DisplayUserName,
                            Value2 = candidate2.DisplayUserName,
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                    
                    // 比较执行者类型
                    if (candidate1.ExecutorType != candidate2.ExecutorType)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Candidate_{candidate1.CandidateId}_ExecutorType",
                            FieldDisplayName = $"候选人[{candidate1.DisplayUserName}]执行者类型",
                            Value1 = candidate1.ExecutorType.ToString(),
                            Value2 = candidate2.ExecutorType.ToString(),
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                    
                    // 比较默认选择
                    if (candidate1.DefaultSelection != candidate2.DefaultSelection)
                    {
                        diff.DiffItems.Add(new WkDefinitionDiffItemDto
                        {
                            FieldName = $"Candidate_{candidate1.CandidateId}_DefaultSelection",
                            FieldDisplayName = $"候选人[{candidate1.DisplayUserName}]默认选择",
                            Value1 = candidate1.DefaultSelection.ToString(),
                            Value2 = candidate2.DefaultSelection.ToString(),
                            ChangeType = DiffChangeType.Modified
                        });
                    }
                }
            }
            
            return Task.CompletedTask;
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
                var runningInstancesCount = await CheckRunningInstancesAsync(id, version.Version);
                
                history.Add(new WkDefinitionVersionHistoryDto
                {
                    Version = version.Version,
                    Title = version.Title,
                    CreationTime = version.CreationTime,
                    //CreatorName = version.Creator?.Name,
                    IsEnabled = version.IsEnabled,
                    HasRunningInstances = runningInstancesCount > 0,
                    InstanceCount = runningInstancesCount,
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
        /// 只有在节点有增减的情况下才创建新版本，如果只是修改节点属性，则直接更新当前版本
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<List<WkNodeDto>> UpdateAsync(DefinitionNodeUpdateDto input)
        {
            // 获取最新版本的实体
            var entity = await _definitionRespository.FindAsync(input.Id) ?? throw new UserFriendlyException(message: $"Id为：{input.Id}模板不存在！");
            
            // 检查节点是否有增减（使用 ID 进行严格比较）
            var existingNodeIds = entity.Nodes.Select(n => n.Id).ToHashSet();
            
            // 如果新节点中有任何节点的 ID 为 null，说明有新增节点
            var hasNewNodes = input.Nodes.Any(n => !n.Id.HasValue);
            
            // 获取新节点中所有有效的 ID（过滤掉 null）
            var newNodeIds = input.Nodes
                .Where(n => n.Id.HasValue)
                .Select(n => n.Id!.Value)
                .ToHashSet();
            
            // 检查是否有节点增减：
            // 1. 如果有新增节点（ID 为 null），则肯定有变化
            // 2. 如果 ID 集合数量不一致，则肯定有变化
            // 3. 如果 ID 集合不完全一致，则肯定有变化
            var hasNodeChanges = hasNewNodes ||
                                 existingNodeIds.Count != newNodeIds.Count ||
                                 !existingNodeIds.SetEquals(newNodeIds);
            
            if (hasNodeChanges)
            {
                // 节点有增减，需要创建新版本
                // 注意：创建新版本不会影响正在运行的旧版本实例，因此不需要检查运行中的实例
                // WorkflowCore 支持多版本共存，每个实例绑定到特定版本的定义
                
                // 获取新版本号
                var newVersion = await GetNextVersionAsync(entity.Id, entity.Version);
                
                // 创建新版本
                var newEntity = await CreateNewVersionForNodeUpdateAsync(entity, input, newVersion);
                
                // 保存新版本到数据库
                await _definitionRespository.InsertAsync(newEntity);
                
                // 注册新版本到工作流引擎
                await _hxWorkflowManager.UpdateAsync(newEntity);
                
                return ObjectMapper.Map<List<WkNode>, List<WkNodeDto>>([.. newEntity.Nodes]);
            }
            else
            {
                // 节点没有增减，直接更新当前版本的节点属性
                // 注意：直接修改当前版本可能会影响正在运行的实例，但 WorkflowCore 会在实例启动时加载定义快照
                // 因此修改当前版本不会影响已经运行的实例，只会影响新启动的实例
                // 如果需要更严格的控制，可以在这里添加检查，但根据最佳实践，通常不需要
                await UpdateNodesInCurrentVersionAsync(entity, input);
                
                // 保存更新
                await _definitionRespository.UpdateAsync(entity);
                
                // 重新注册到工作流引擎
                await _hxWorkflowManager.UpdateAsync(entity);
                
                return ObjectMapper.Map<List<WkNode>, List<WkNodeDto>>([.. entity.Nodes]);
            }
        }
        
        /// <summary>
        /// 在当前版本中直接更新节点属性（不创建新版本）
        /// </summary>
        private async Task UpdateNodesInCurrentVersionAsync(WkDefinition entity, DefinitionNodeUpdateDto input)
        {
            // 处理节点更新
            var nodeEntitys = input.Nodes.ToWkNodes(GuidGenerator);
            if (nodeEntitys != null && nodeEntitys.Count > 0)
            {
                // 使用 ID 来匹配节点（更严格）
                var existingDict = entity.Nodes.ToDictionary(n => n.Id);
                
                foreach (var newNode in nodeEntitys)
                {
                    // 通过 ID 查找现有节点
                    if (existingDict.TryGetValue(newNode.Id, out var existingNode))
                    {
                        // 更新现有节点的属性
                        var inputNode = input.Nodes.FirstOrDefault(d => d.Id == newNode.Id);
                        if (inputNode != null)
                        {
                            // 更新 StepBody
                            await existingNode.SetWkStepBody(await GetStepBodyByIdAsync(inputNode.WkStepBodyId, newNode.StepNodeType));
                            
                            // 更新 ExtraProperties
                            if (existingNode.ExtraProperties != null && inputNode.ExtraProperties != null)
                            {
                                existingNode.ExtraProperties.Clear();
                                inputNode.ExtraProperties.ForEach(item => existingNode.ExtraProperties.TryAdd(item.Key, item.Value));
                            }
                        }
                    }
                }
            }
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
            var runningInstancesCount = await CheckRunningInstancesAsync(id, latestVersion.Version);
            if (runningInstancesCount > 0)
            {
                throw new UserFriendlyException(message: $"该版本有 {runningInstancesCount} 个正在运行的实例，无法删除。请等待实例完成后再删除。");
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
            var runningInstancesCount = await CheckRunningInstancesAsync(id, version);
            if (runningInstancesCount > 0)
            {
                throw new UserFriendlyException(message: $"该版本有 {runningInstancesCount} 个正在运行的实例，无法删除。请等待实例完成后再删除。");
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
                var runningInstancesCount = await CheckRunningInstancesAsync(id, version.Version);
                if (runningInstancesCount > 0)
                {
                    throw new UserFriendlyException(message: $"版本 {version.Version} 有 {runningInstancesCount} 个正在运行的实例，无法删除所有版本。请等待实例完成后再删除。");
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
                var runningInstancesCount = await CheckRunningInstancesAsync(id, version.Version);
                if (runningInstancesCount > 0)
                {
                    throw new UserFriendlyException(message: $"版本 {version.Version} 有 {runningInstancesCount} 个正在运行的实例，无法删除。请等待实例完成后再删除旧版本。");
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
