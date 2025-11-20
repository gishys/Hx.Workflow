using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

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
                    var newCandidate = new DefinitionCandidate(
                        candidate.CandidateId,
                        candidate.UserName,
                        candidate.DisplayUserName,
                        candidate.ExecutorType,
                        candidate.DefaultSelection,
                        entity.Version);
                    // 设置 NodeId（实际上是 WkDefinition 的 Id）
                    await newCandidate.SetNodeId(entity.Id);
                    entity.WkCandidates.Add(newCandidate);
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
            // 获取最新未归档版本的实体
            var entity = await _definitionRespository.FindLatestVersionAsync(input.Id) ?? throw new UserFriendlyException(message: $"Id为：{input.Id}模板不存在！");
            
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
                    var newCandidate = new DefinitionCandidate(
                        candidate.CandidateId,
                        candidate.UserName,
                        candidate.DisplayUserName,
                        candidate.ExecutorType,
                        candidate.DefaultSelection,
                        entity.Version);
                    // 设置 NodeId（实际上是 WkDefinition 的 Id）
                    await newCandidate.SetNodeId(entity.Id);
                    entity.WkCandidates.Add(newCandidate);
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
                var newNode = await CloneNodeAsync(node);
                await newEntity.AddWkNode(newNode);
            }
            
            // 更新候选人（需要设置 NodeId 和 Version 以避免主键冲突）
            newEntity.WkCandidates.Clear();
            if (input.WkCandidates?.Count > 0)
            {
                foreach (var candidate in input.WkCandidates)
                {
                    var newCandidate = new DefinitionCandidate(
                        candidate.CandidateId,
                        candidate.UserName,
                        candidate.DisplayUserName,
                        candidate.ExecutorType,
                        candidate.DefaultSelection,
                        newVersion);
                    // 设置 NodeId（实际上是 WkDefinition 的 Id）和 Version
                    await newCandidate.SetNodeId(newEntity.Id);
                    newEntity.WkCandidates.Add(newCandidate);
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
        private async Task<WkNode> CloneNodeAsync(WkNode originalNode)
        {
            // 创建新节点，使用新的ID
            var clonedNode = new WkNode(
                originalNode.Name,
                originalNode.DisplayName,
                originalNode.StepNodeType,
                originalNode.SortNumber,
                originalNode.LimitTime,
                GuidGenerator.Create()); // 生成新的ID

            // 注意：WkDefinitionId 和 WkDefinitionVersion 会在 AddWkNode 时自动设置
            // 这里不需要手动设置，因为节点必须通过 AddWkNode 添加到定义中

            // 复制步骤体（必需）
            if (originalNode.StepBody == null)
            {
                throw new InvalidOperationException($"节点 [{originalNode.Name}] 的 StepBody 不能为 null，节点必须有一个执行体。");
            }
            await clonedNode.SetWkStepBody(originalNode.StepBody);
            
            // 复制候选人
            foreach (var candidate in originalNode.WkCandidates)
            {
                clonedNode.WkCandidates.Add(new WkNodeCandidate(
                    candidate.CandidateId,
                    candidate.UserName,
                    candidate.DisplayUserName,
                    candidate.ExecutorType,
                    candidate.DefaultSelection));
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
                    []);
                
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
        /// 注意：如果当前版本没有节点（刚创建的模板定义），则直接更新当前版本，不创建新版本
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<List<WkNodeDto>> UpdateAsync(DefinitionNodeUpdateDto input)
        {
            var entity = await _definitionRespository.FindLatestVersionAsync(input.Id) ?? throw new UserFriendlyException(message: $"Id为：{input.Id}模板不存在！");
            
            // 验证并过滤节点：确保只有一个开始节点，并且所有节点都在关系链中
            var validatedNodes = ValidateAndFilterNodes(input.Nodes);
            
            // 如果验证后没有节点，抛出异常
            if (validatedNodes.Count == 0)
            {
                throw new UserFriendlyException(message: "节点验证失败：没有有效的节点可以保存。请确保至少有一个开始节点，并且所有节点都能通过开始节点的关系链关联起来。");
            }
            
            // 创建新的输入对象，使用验证后的节点
            var validatedInput = new DefinitionNodeUpdateDto
            {
                Id = input.Id,
                ExtraProperties = input.ExtraProperties,
                Nodes = validatedNodes
            };
            
            // 如果当前版本没有节点（刚创建的模板定义），直接更新当前版本，不创建新版本
            // 这是因为创建模板定义和更新节点是独立的方法，首次添加节点不应该触发版本变更
            if (entity.Nodes.Count == 0)
            {
                // 首次添加节点，直接更新当前版本
                await UpdateNodesInCurrentVersionAsync(entity, validatedInput);
                
                // 更新模板定义的扩展属性
                if (validatedInput.ExtraProperties != null)
                {
                    entity.ExtraProperties.Clear();
                    validatedInput.ExtraProperties.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
                }
                
                await _definitionRespository.UpdateAsync(entity);
                await _hxWorkflowManager.UpdateAsync(entity);
                
                return ObjectMapper.Map<List<WkNode>, List<WkNodeDto>>([.. entity.Nodes]);
            }
            
            // 检查节点是否有增减（使用 ID 进行严格比较）
            var existingNodeIds = entity.Nodes.Select(n => n.Id).ToHashSet();
            var hasNewNodes = validatedNodes.Any(n => !n.Id.HasValue);
            var newNodeIds = validatedNodes
                .Where(n => n.Id.HasValue)
                .Select(n => n.Id!.Value)
                .ToHashSet();
            
            var hasNodeChanges = hasNewNodes ||
                                 existingNodeIds.Count != newNodeIds.Count ||
                                 !existingNodeIds.SetEquals(newNodeIds);
            
            if (hasNodeChanges)
            {
                // 节点有增减，需要创建新版本
                var newVersion = await GetNextVersionAsync(entity.Id, entity.Version);
                var newEntity = await CreateNewVersionForNodeUpdateAsync(entity, validatedInput, newVersion);
                
                // 保存新版本到数据库
                await _definitionRespository.InsertAsync(newEntity);
                
                // 注册新版本到工作流引擎
                await _hxWorkflowManager.UpdateAsync(newEntity);
                
                return ObjectMapper.Map<List<WkNode>, List<WkNodeDto>>([.. newEntity.Nodes]);
            }
            else
            {
                // 节点没有增减，直接更新当前版本的节点属性
                await UpdateNodesInCurrentVersionAsync(entity, validatedInput);
                
                // 更新模板定义的扩展属性
                if (validatedInput.ExtraProperties != null)
                {
                    entity.ExtraProperties.Clear();
                    validatedInput.ExtraProperties.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
                }
                
                await _definitionRespository.UpdateAsync(entity);
                await _hxWorkflowManager.UpdateAsync(entity);
                
                return ObjectMapper.Map<List<WkNode>, List<WkNodeDto>>([.. entity.Nodes]);
            }
        }
        
        /// <summary>
        /// 验证并过滤节点
        /// 1. 检查节点名称不能重复
        /// 2. 确保只有一个开始节点
        /// 3. 从开始节点遍历所有可达节点（通过 NextNodes，NodeType == Forward）
        /// 4. 所有节点必须在关系链中，否则抛出异常
        /// </summary>
        /// <param name="nodes">输入的节点列表</param>
        /// <returns>验证并过滤后的节点列表</returns>
        private static List<WkNodeCreateDto> ValidateAndFilterNodes(ICollection<WkNodeCreateDto> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return [];
            }
            
            // 1. 检查节点名称不能重复
            var nodeNameGroups = nodes.GroupBy(n => n.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();
            
            if (nodeNameGroups.Count > 0)
            {
                var duplicateNames = nodeNameGroups.Select(g => $"'{g.Key}'（出现 {g.Count()} 次）").ToList();
                var duplicateNamesStr = string.Join("、", duplicateNames);
                throw new UserFriendlyException(message: $"节点验证失败：节点名称不能重复。发现重复的节点名称：{duplicateNamesStr}。");
            }
            
            // 2. 检查开始节点数量（只能有一个）
            var startNodes = nodes.Where(n => n.StepNodeType == StepNodeType.Start).ToList();
            if (startNodes.Count == 0)
            {
                throw new UserFriendlyException(message: "节点验证失败：模板定义必须包含一个开始节点（StepNodeType = Start）。");
            }
            
            if (startNodes.Count > 1)
            {
                var startNodeNames = string.Join("、", startNodes.Select(n => $"'{n.Name}'"));
                throw new UserFriendlyException(message: $"节点验证失败：模板定义只能有一个开始节点，但找到了 {startNodes.Count} 个：{startNodeNames}。");
            }
            
            var startNode = startNodes.First();
            
            // 3. 创建节点名称到节点的映射，用于快速查找
            var nodeDict = nodes.ToDictionary(n => n.Name, StringComparer.OrdinalIgnoreCase);
            
            // 4. 从开始节点开始，通过 NextNodes（NodeType == Forward）遍历所有可达节点
            var reachableNodeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visitedNodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 使用深度优先搜索遍历所有可达节点
            TraverseNodesFromStart(startNode.Name, nodeDict, reachableNodeNames, visitedNodes);
            
            // 5. 检查所有节点是否都在关系链中
            var isolatedNodeNames = nodes
                .Where(n => !reachableNodeNames.Contains(n.Name))
                .Select(n => $"'{n.Name}'")
                .ToList();
            
            if (isolatedNodeNames.Count > 0)
            {
                var isolatedNames = string.Join("、", isolatedNodeNames);
                throw new UserFriendlyException(message: $"节点验证失败：以下节点不在关系链中（无法从开始节点通过 NextNodes 关系链到达）：{isolatedNames}。请确保所有节点都能通过开始节点的关系链关联起来。");
            }
            
            // 6. 返回所有节点（因为都已经验证通过）
            return [.. nodes];
        }
        
        /// <summary>
        /// 从指定节点开始，递归遍历所有可达节点（通过 NextNodes，NodeType == Forward）
        /// </summary>
        /// <param name="nodeName">当前节点名称</param>
        /// <param name="nodeDict">节点名称到节点的映射</param>
        /// <param name="reachableNodeNames">可达节点名称集合（输出）</param>
        /// <param name="visitedNodes">已访问节点集合（用于防止循环）</param>
        private static void TraverseNodesFromStart(
            string nodeName,
            Dictionary<string, WkNodeCreateDto> nodeDict,
            HashSet<string> reachableNodeNames,
            HashSet<string> visitedNodes)
        {
            // 防止循环引用
            if (visitedNodes.Contains(nodeName))
            {
                return;
            }
            
            // 如果节点不存在，抛出异常
            if (!nodeDict.TryGetValue(nodeName, out var currentNode))
            {
                throw new UserFriendlyException(message: $"节点验证失败：未找到节点 '{nodeName}'。可能引用了不存在的节点，请检查 NextNodes 中的节点名称是否正确。");
            }
            
            // 标记为已访问并添加到可达节点集合
            visitedNodes.Add(nodeName);
            reachableNodeNames.Add(nodeName);
            
            // 遍历当前节点的所有 NextNodes（只处理 NodeType == Forward 的节点）
            if (currentNode.NextNodes != null && currentNode.NextNodes.Count > 0)
            {
                foreach (var nextNode in currentNode.NextNodes)
                {
                    // 只处理类型为 Forward（向后）的节点关系
                    if (nextNode.NodeType == WkRoleNodeType.Forward)
                    {
                        // 递归遍历下一个节点
                        TraverseNodesFromStart(nextNode.NextNodeName, nodeDict, reachableNodeNames, visitedNodes);
                    }
                }
            }
        }
        
        /// <summary>
        /// 在当前版本中直接更新节点属性（不创建新版本）
        /// 注意：直接更新当前版本可能会影响正在运行的流程实例的执行逻辑
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
                            // 更新基本属性
                            
                            if (!string.Equals(existingNode.DisplayName, inputNode.DisplayName, StringComparison.OrdinalIgnoreCase))
                            {
                                await existingNode.SetDisplayName(inputNode.DisplayName);
                            }
                            
                            if (existingNode.StepNodeType != inputNode.StepNodeType)
                            {
                                await existingNode.SetStepNodeType(inputNode.StepNodeType);
                            }
                            
                            if (existingNode.LimitTime != inputNode.LimitTime)
                            {
                                await existingNode.SetLimitTime(inputNode.LimitTime);
                            }
                            
                            // 更新 StepBody
                            await existingNode.SetWkStepBody(await GetStepBodyByIdAsync(inputNode.WkStepBodyId, newNode.StepNodeType));
                            
                            // 更新 ExtraProperties
                            if (existingNode.ExtraProperties != null && inputNode.ExtraProperties != null)
                            {
                                existingNode.ExtraProperties.Clear();
                                inputNode.ExtraProperties.ForEach(item => existingNode.ExtraProperties.TryAdd(item.Key, item.Value));
                            }
                            
                            // 更新 NextNodes（节点的上下手关系）
                            // 如果提供了 NextNodes（即使为空），则更新；如果为 null，则保持不变
                            if (newNode.NextNodes != null)
                            {
                                existingNode.UpdateNextNodes(newNode.NextNodes);
                            }
                            
                            // 更新 OutcomeSteps（分支节点参数）
                            if (inputNode.OutcomeSteps != null)
                            {
                                existingNode.OutcomeSteps.Clear();
                                foreach (var outcomeStepDto in inputNode.OutcomeSteps)
                                {
                                    await existingNode.AddOutcomeSteps(new WkNodePara(outcomeStepDto.Key, outcomeStepDto.Value));
                                }
                            }
                            
                            // 更新 WkCandidates（避免主键冲突）
                            if (inputNode.WkCandidates != null)
                            {
                                // 清除现有的候选人
                                existingNode.WkCandidates.Clear();
                                
                                // 添加新的候选人（确保 NodeId 和 Version 正确设置）
                                foreach (var candidateDto in inputNode.WkCandidates)
                                {
                                    var candidate = new WkNodeCandidate(
                                        candidateDto.CandidateId,
                                        candidateDto.UserName,
                                        candidateDto.DisplayUserName,
                                        candidateDto.ExecutorType,
                                        candidateDto.DefaultSelection);
                                    // 设置 NodeId（使用现有节点的 ID）
                                    await candidate.SetNodeId(existingNode.Id);
                                    existingNode.WkCandidates.Add(candidate);
                                }
                            }
                            
                            // 更新 ApplicationForms（表单集合）
                            if (inputNode.ApplicationForms != null)
                            {
                                existingNode.ApplicationForms.Clear();
                                foreach (var formDto in inputNode.ApplicationForms)
                                {
                                    var ps = new List<WkParam>();
                                    if (formDto.Params != null && formDto.Params.Count > 0)
                                    {
                                        foreach (var paramDto in formDto.Params)
                                        {
                                            ps.Add(new WkParam(paramDto.WkParamKey, paramDto.Name, paramDto.DisplayName, paramDto.Value));
                                        }
                                    }
                                    await existingNode.AddApplicationForms(formDto.ApplicationFormId, formDto.SequenceNumber, ps);
                                }
                            }
                            
                            // 更新 Params（流程参数）
                            if (inputNode.Params != null)
                            {
                                existingNode.Params.Clear();
                                foreach (var paramDto in inputNode.Params)
                                {
                                    await existingNode.AddParam(new WkParam(paramDto.WkParamKey, paramDto.Name, paramDto.DisplayName, paramDto.Value));
                                }
                            }
                            
                            // 更新 Materials（材料集合）
                            if (inputNode.Materials != null)
                            {
                                existingNode.Materials.Clear();
                                foreach (var materialDto in inputNode.Materials)
                                {
                                    var material = new WkNodeMaterials(
                                        materialDto.AttachReceiveType,
                                        materialDto.ReferenceType,
                                        materialDto.CatalogueName,
                                        materialDto.SequenceNumber,
                                        materialDto.IsRequired,
                                        materialDto.IsStatic,
                                        materialDto.IsVerification,
                                        materialDto.VerificationPassed);
                                    
                                    // 处理子材料（递归）
                                    if (materialDto.Children != null && materialDto.Children.Count > 0)
                                    {
                                        foreach (var childDto in materialDto.Children)
                                        {
                                            material.AddChild(new WkNodeMaterials(
                                                childDto.AttachReceiveType,
                                                childDto.ReferenceType,
                                                childDto.CatalogueName,
                                                childDto.SequenceNumber,
                                                childDto.IsRequired,
                                                childDto.IsStatic,
                                                childDto.IsVerification,
                                                childDto.VerificationPassed));
                                        }
                                    }
                                    
                                    await existingNode.AddMaterails(material);
                                }
                            }
                        }
                    }
                }
            }
            
            // 验证节点限制时间
            if (entity.LimitTime < entity.Nodes.Sum(d => d.LimitTime ?? 0))
            {
                throw new UserFriendlyException(message: "节点限制时间合计值不能大于流程限制时间！");
            }
        }
        
        /// <summary>
        /// 为节点更新创建新版本
        /// 主要目的：克隆出一个新的模板定义，并把最新的节点等信息维护到模板定义上
        /// 注意：创建新版本时，所有节点都是全新的，ID会重新生成，与旧版本节点没有关联
        /// </summary>
        /// <param name="originalEntity">原实体</param>
        /// <param name="input">更新输入</param>
        /// <param name="newVersion">新版本号</param>
        /// <returns></returns>
        private async Task<WkDefinition> CreateNewVersionForNodeUpdateAsync(WkDefinition originalEntity, DefinitionNodeUpdateDto input, int newVersion)
        {
            // 1. 创建新的模板定义实体（新版本）
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
            
            // 2. 复制候选人
            if (originalEntity.WkCandidates != null && originalEntity.WkCandidates.Count > 0)
            {
                foreach (var candidate in originalEntity.WkCandidates.ToList())
                {
                    var newCandidate = new DefinitionCandidate(
                        candidate.CandidateId,
                        candidate.UserName,
                        candidate.DisplayUserName,
                        candidate.ExecutorType,
                        candidate.DefaultSelection,
                        newVersion);
                    
                    await newCandidate.SetNodeId(newEntity.Id);
                    await newCandidate.SetVersion(newVersion);
                    newEntity.WkCandidates.Add(newCandidate);
                }
            }
            
            // 3. 根据输入的节点创建全新的节点（所有节点都是新的，ID重新生成）
            if (input.Nodes != null && input.Nodes.Count > 0)
            {
                // 第一步：创建所有节点实例（使用新ID，避免与旧版本节点关联）
                var newNodeDict = new Dictionary<string, WkNode>(); // 使用节点名称作为键，因为ID都是新的
                int sortNumber = 0;
                
                foreach (var inputNode in input.Nodes)
                {
                    // 创建新节点，始终生成新ID（忽略inputNode.Id，因为这是新版本）
                    var newNode = new WkNode(
                        inputNode.Name,
                        inputNode.DisplayName,
                        inputNode.StepNodeType,
                        sortNumber++,
                        inputNode.LimitTime,
                        GuidGenerator.Create()); // 始终生成新ID
                    
                    // 设置 StepBody（必需）
                    // 注意：开始和结束节点如果 WkStepBodyId 为空，会自动使用预定义的 StartStepBody 或 StopStepBody
                    // Activity 类型的节点必须提供有效的 WkStepBodyId
                    if (inputNode.StepNodeType == StepNodeType.Activity && string.IsNullOrEmpty(inputNode.WkStepBodyId))
                    {
                        throw new UserFriendlyException(message: $"节点 [{inputNode.Name}] 是 Activity 类型，必须提供有效的 WkStepBodyId。");
                    }
                    await newNode.SetWkStepBody(await GetStepBodyByIdAsync(inputNode.WkStepBodyId, inputNode.StepNodeType));
                    
                    // 设置 ExtraProperties
                    if (inputNode.ExtraProperties != null && inputNode.ExtraProperties.Count > 0)
                    {
                        inputNode.ExtraProperties.ForEach(item => newNode.ExtraProperties.TryAdd(item.Key, item.Value));
                    }
                    
                    // 设置 OutcomeSteps
                    if (inputNode.OutcomeSteps != null && inputNode.OutcomeSteps.Count > 0)
                    {
                        foreach (var outcomeStepDto in inputNode.OutcomeSteps)
                        {
                            await newNode.AddOutcomeSteps(new WkNodePara(outcomeStepDto.Key, outcomeStepDto.Value));
                        }
                    }
                    
                    // 设置 WkCandidates
                    if (inputNode.WkCandidates != null && inputNode.WkCandidates.Count > 0)
                    {
                        foreach (var candidateDto in inputNode.WkCandidates)
                        {
                            var candidate = new WkNodeCandidate(
                                candidateDto.CandidateId,
                                candidateDto.UserName,
                                candidateDto.DisplayUserName,
                                candidateDto.ExecutorType,
                                candidateDto.DefaultSelection);
                            await candidate.SetNodeId(newNode.Id);
                            newNode.WkCandidates.Add(candidate);
                        }
                    }
                    
                    // 设置 ApplicationForms
                    if (inputNode.ApplicationForms != null && inputNode.ApplicationForms.Count > 0)
                    {
                        foreach (var formDto in inputNode.ApplicationForms)
                        {
                            var ps = new List<WkParam>();
                            if (formDto.Params != null && formDto.Params.Count > 0)
                            {
                                foreach (var paramDto in formDto.Params)
                                {
                                    ps.Add(new WkParam(paramDto.WkParamKey, paramDto.Name, paramDto.DisplayName, paramDto.Value));
                                }
                            }
                            await newNode.AddApplicationForms(formDto.ApplicationFormId, formDto.SequenceNumber, ps);
                        }
                    }
                    
                    // 设置 Params
                    if (inputNode.Params != null && inputNode.Params.Count > 0)
                    {
                        foreach (var paramDto in inputNode.Params)
                        {
                            await newNode.AddParam(new WkParam(paramDto.WkParamKey, paramDto.Name, paramDto.DisplayName, paramDto.Value));
                        }
                    }
                    
                    // 设置 Materials
                    if (inputNode.Materials != null && inputNode.Materials.Count > 0)
                    {
                        foreach (var materialDto in inputNode.Materials)
                        {
                            var material = new WkNodeMaterials(
                                materialDto.AttachReceiveType,
                                materialDto.ReferenceType,
                                materialDto.CatalogueName,
                                materialDto.SequenceNumber,
                                materialDto.IsRequired,
                                materialDto.IsStatic,
                                materialDto.IsVerification,
                                materialDto.VerificationPassed);
                            
                            if (materialDto.Children != null && materialDto.Children.Count > 0)
                            {
                                foreach (var childDto in materialDto.Children)
                                {
                                    material.AddChild(new WkNodeMaterials(
                                        childDto.AttachReceiveType,
                                        childDto.ReferenceType,
                                        childDto.CatalogueName,
                                        childDto.SequenceNumber,
                                        childDto.IsRequired,
                                        childDto.IsStatic,
                                        childDto.IsVerification,
                                        childDto.VerificationPassed));
                                }
                            }
                            
                            await newNode.AddMaterails(material);
                        }
                    }
                    
                    // 保存到字典中（使用节点名称作为键，用于建立关系）
                    newNodeDict[newNode.Name] = newNode;
                }
                
                // 第二步：建立节点关系（NextNodes）
                // 根据节点名称查找目标节点，因为 NextNodes 中使用的是节点名称
                foreach (var inputNode in input.Nodes)
                {
                    var newNode = newNodeDict[inputNode.Name];
                    
                    if (inputNode.NextNodes != null && inputNode.NextNodes.Count > 0)
                    {
                        foreach (var nextNodeRelation in inputNode.NextNodes)
                        {
                            // 查找目标节点（通过节点名称）
                            if (newNodeDict.TryGetValue(nextNodeRelation.NextNodeName, out var targetNode))
                            {
                                // 创建新的关系对象
                                // 注意：WkConditionNodeCreateDto 没有 Label 属性，使用空字符串
                                var newRelation = new WkNodeRelation(
                                    nextNodeRelation.NextNodeName,
                                    nextNodeRelation.NodeType,
                                    "");
                                
                                // 复制规则
                                if (nextNodeRelation.Rules != null && nextNodeRelation.Rules.Count > 0)
                                {
                                    foreach (var rule in nextNodeRelation.Rules)
                                    {
                                        await newRelation.AddConNodeCondition(new WkNodeRelationRule(
                                            rule.Field,
                                            rule.Operator,
                                            rule.Value));
                                    }
                                }
                                
                                await newNode.AddNextNode(newRelation);
                            }
                        }
                    }
                }
                
                // 第三步：将所有节点添加到新实体（按输入顺序添加）
                foreach (var inputNode in input.Nodes)
                {
                    var newNode = newNodeDict[inputNode.Name];
                    await newEntity.AddWkNode(newNode);
                }
            }
            else
            {
                // 如果没有输入节点，复制原节点的所有节点（这种情况不应该发生，因为验证已经确保有节点）
                foreach (var node in originalEntity.Nodes)
                {
                    var clonedNode = await CloneNodeAsync(node);
                    await newEntity.AddWkNode(clonedNode);
                }
            }
            
            // 4. 验证节点限制时间
            if (newEntity.LimitTime < newEntity.Nodes.Sum(d => d.LimitTime ?? 0))
            {
                throw new UserFriendlyException(message: "节点限制时间合计值不能大于流程限制时间！");
            }
            
            // 5. 验证节点名称唯一性
            var duplicateNames = newEntity.Nodes.GroupBy(p => p.Name).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicateNames.Any())
            {
                throw new UserFriendlyException(message: $"节点名称不能重复：{string.Join(", ", duplicateNames)}");
            }
            
            // 6. 复制扩展属性
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
            var entity = await _definitionRespository.FindLatestVersionAsync(id);
            return ObjectMapper.Map<WkDefinition?, WkDefinitionDto?>(entity);
        }
        /// <summary>
        /// 删除模板定义
        /// 删除逻辑：
        /// 1. 删除所有没有实例关联的版本
        /// 2. 对于有实例关联的版本，标记为已归档（IsArchived = true），并禁用（IsEnabled = false）
        /// 3. 已归档的版本不再用于模板管理，仅用于服务已创建的实例
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(Guid id)
        {
            // 获取所有版本（包括已归档的）
            var allVersions = await _definitionRespository.GetAllVersionsAsync(id);
            if (allVersions.Count == 0)
            {
                throw new UserFriendlyException(message: $"模板 {id} 不存在！");
            }

            var versionsToDelete = new List<WkDefinition>();
            var versionsToArchive = new List<WkDefinition>();

            // 检查每个版本是否有实例关联
            foreach (var version in allVersions)
            {
                var instancesCount = await _wkInstanceRepository.GetInstancesCountByVersionAsync(id, version.Version);
                
                if (instancesCount == 0)
                {
                    // 没有实例关联，可以删除
                    versionsToDelete.Add(version);
                }
                else
                {
                    // 有实例关联，需要归档
                    versionsToArchive.Add(version);
                }
            }

            // 物理删除没有实例关联的版本
            foreach (var version in versionsToDelete)
            {
                await _definitionRespository.HardDeleteAsync(version, true);
            }

            // 归档有实例关联的版本
            foreach (var version in versionsToArchive)
            {
                await version.SetArchived(true);
                await version.SetEnabled(false);
                await _definitionRespository.UpdateAsync(version);
            }

            // 从工作流引擎中注销已归档的版本
            // 注意：已归档的版本仍然需要保留在工作流引擎中，以便已创建的实例可以继续执行
            // 因此这里不需要注销，只需要标记为已归档即可
        }
        /// <summary>
        /// 获取流程模版
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> GetDefinitionAsync(Guid id, int version)
        {
            // 验证版本号
            if (version <= 0)
            {
                throw new UserFriendlyException(message: $"版本号必须大于0，当前值：{version}");
            }
            
            var entity = await _definitionRespository.GetDefinitionAsync(id, version) ?? throw new UserFriendlyException(message: $"Id为：{id}，版本为：{version}的模板不存在！");
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
