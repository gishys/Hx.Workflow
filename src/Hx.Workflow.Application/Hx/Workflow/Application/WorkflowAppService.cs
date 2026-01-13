using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.BusinessModule;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.Stats;
using Hx.Workflow.Domain.StepBodys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    //[Authorize]
    public class WorkflowAppService(
        HxWorkflowManager hxWorkflowManager,
        IWkDefinitionRespository wkDefinition,
        IWkInstanceRepository wkInstanceRepository,
        IWkErrorRepository errorRepository,
        IWkExecutionPointerRepository wkExecutionPointerRepository,
        IWkAuditorRespository wkAuditor,
        IServiceProvider serviceProvider) : HxWorkflowAppServiceBase, IWorkflowAppService
    {
        private readonly HxWorkflowManager _hxWorkflowManager = hxWorkflowManager;
        private readonly IWkDefinitionRespository _wkDefinition = wkDefinition;
        private readonly IWkInstanceRepository _wkInstanceRepository = wkInstanceRepository;
        private readonly IWkErrorRepository _errorRepository = errorRepository;
        private readonly IWkExecutionPointerRepository _wkExecutionPointerRepository = wkExecutionPointerRepository;
        private readonly IWkAuditorRespository _wkAuditor = wkAuditor;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IAuthorizationService? _authorizationService = serviceProvider.GetService<IAuthorizationService>();

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
            throw new UserFriendlyException(message: "未获取到当前登录用户！");
        }
        /// <summary>
        /// 通过流程模版Id创建流程
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<string> StartAsync(StartWorkflowInput input)
        {
            try
            {
                // 验证版本号
                if (input.Version <= 0)
                {
                    throw new UserFriendlyException(message: $"流程启动失败：版本号必须大于0，当前提供的版本号为 {input.Version}。");
                }
                
                // 验证候选人参数
                var candidateKeyValue = input.Inputs.FirstOrDefault(kv => kv.Key == "Candidates");
                if (candidateKeyValue.Equals(default(KeyValuePair<string, object>)))
                {
                    throw new UserFriendlyException(message: "流程启动失败：启动参数中缺少候选人信息（Candidates）。请确保在输入参数中包含候选人ID。");
                }

                if (!Guid.TryParse(candidateKeyValue.Value?.ToString(), out Guid candidateId))
                {
                    var candidateValue = candidateKeyValue.Value?.ToString() ?? "null";
                    throw new UserFriendlyException(message: $"流程启动失败：候选人ID格式无效。提供的值：{candidateValue}，期望格式：有效的GUID（例如：xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx）。");
                }

                // 查询流程模板定义
                var entity = await _wkDefinition.GetDefinitionAsync(new Guid(input.Id), input.Version) 
                    ?? throw new UserFriendlyException(message: $"流程启动失败：未找到流程模板。模板ID：{input.Id}，版本号：{input.Version}。请确认模板ID和版本号是否正确。");
                
                // 检查版本是否已归档
                if (entity.IsArchived)
                {
                    throw new UserFriendlyException(message: $"流程启动失败：流程模板版本已归档。模板ID：{input.Id}，版本号：{input.Version}。已归档的版本仅用于服务已创建的实例，无法用于创建新实例。请使用未归档的版本。");
                }
                
                // 检查版本是否已注册到工作流引擎
                if (!_hxWorkflowManager.IsRegistered(input.Id, input.Version))
                {
                    throw new UserFriendlyException(message: $"流程启动失败：流程模板未注册到工作流引擎。模板ID：{input.Id}，版本号：{input.Version}。请确保模板已正确创建并注册到工作流引擎。");
                }
                
                // 检查用户权限
                if (!entity.WkCandidates.Any(d => d.CandidateId == candidateId))
                {
                    throw new UserFriendlyException(message: $"流程启动失败：当前用户无权限启动此流程。候选人ID：{candidateId}，模板ID：{input.Id}，版本号：{input.Version}。请在流程定义中配置该用户的权限。");
                }
                
                // 将当前用户信息添加到工作流数据中，以便在 StepBody 中获取
                if (CurrentUser.Id.HasValue)
                {
                    input.Inputs["StartUserId"] = CurrentUser.Id.Value;
                    if (!string.IsNullOrEmpty(CurrentUser.UserName))
                    {
                        input.Inputs["StartUserName"] = CurrentUser.UserName;
                    }
                    // 设置当前操作用户（启动时与启动用户相同）
                    input.Inputs["CurrentUserId"] = CurrentUser.Id.Value;
                    if (!string.IsNullOrEmpty(CurrentUser.UserName))
                    {
                        input.Inputs["CurrentUserName"] = CurrentUser.UserName;
                    }
                }
                
                return await _hxWorkflowManager.StartWorkflowAsync(input.Id, input.Version, input.Inputs);
            }
            catch (UserFriendlyException)
            {
                // 重新抛出用户友好异常，保持原始错误信息
                throw;
            }
            catch (Exception ex)
            {
                // 对于其他异常，包装为用户友好异常
                throw new UserFriendlyException(message: $"流程启动失败：{ex.Message}");
            }
        }
        /// <summary>
        /// 开始活动
        /// </summary>
        /// <param name="actName"></param>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public virtual async Task StartActivityAsync(string actName, string workflowId, Dictionary<string, object>? data = null)
        {
            var eventPointerEventData = JsonSerializer.Deserialize<WkPointerEventData>(JsonSerializer.Serialize(data));
            if (string.IsNullOrEmpty(eventPointerEventData?.DecideBranching))
                throw new UserFriendlyException(message: "提交必须携带分支节点名称！");
            
            // 将当前用户信息添加到活动数据中，以便在 StepBody 中获取
            data ??= new Dictionary<string, object>();
            if (CurrentUser.Id.HasValue)
            {
                data["CurrentUserId"] = CurrentUser.Id.Value;
                if (!string.IsNullOrEmpty(CurrentUser.UserName))
                {
                    data["CurrentUserName"] = CurrentUser.UserName;
                }
            }
            
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
        //[AllowAnonymous]
        public virtual async Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceAsync(
            ICollection<Guid>? creatorIds = null,
            ICollection<Guid>? definitionIds = null,
            IDictionary<string, object>? instanceData = null,
            MyWorkState? status = null,
            string? reference = null,
            ICollection<Guid>? userIds = null,
            string? queryType = null,
            int skipCount = 0,
            int maxResultCount = 20)
        {
            var canGetAll = _authorizationService != null ? await _authorizationService.AuthorizeAsync("Workflow.Instance.List") : null;
            if ((userIds == null || userIds.Count == 0) && CurrentUser.Id.HasValue)
            {
                userIds = [CurrentUser.Id.Value];
            }
            if (canGetAll != null && canGetAll.Succeeded && queryType == "all")
            {
                userIds = null;
            }
            Logger.LogInformation("Name:{CurrentUser.Name}",CurrentTenant.Name);
            //userIds = [new Guid("3a1a2bff-b3cd-d1f8-97e4-3ee9d66a1f59")];
            List<WkProcessInstanceDto> result = [];
            var instances = await _hxWorkflowManager.WkInstanceRepository.GetMyInstancesAsync(
                creatorIds,
                definitionIds,
                instanceData,
                userIds ?? [],
                reference,
                status,
                skipCount,
                maxResultCount);
            var count = await _wkInstanceRepository.GetMyInstancesCountAsync(
                creatorIds,
                definitionIds,
                instanceData,
                userIds ?? [],
                reference, status);
            foreach (var instance in instances)
            {
                var processInstance = instance.ToProcessInstanceDto();
                result.Add(processInstance);
            }
            return new PagedResultDto<WkProcessInstanceDto>(count, result);
        }
        /// <summary>
        /// 查询我的办理件（支持版本控制）
        /// </summary>
        /// <param name="creatorIds">创建者ID列表</param>
        /// <param name="definitionIds">模板ID列表</param>
        /// <param name="definitionVersions">模板版本列表（可选，null表示所有版本）</param>
        /// <param name="instanceData">实例数据</param>
        /// <param name="status">状态</param>
        /// <param name="reference">引用</param>
        /// <param name="userIds">用户ID列表</param>
        /// <param name="queryType">查询类型</param>
        /// <param name="skipCount">跳过数量</param>
        /// <param name="maxResultCount">最大结果数量</param>
        /// <returns></returns>
        public virtual async Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceWithVersionAsync(
            ICollection<Guid>? creatorIds = null,
            ICollection<Guid>? definitionIds = null,
            ICollection<int>? definitionVersions = null,
            IDictionary<string, object>? instanceData = null,
            MyWorkState? status = null,
            string? reference = null,
            ICollection<Guid>? userIds = null,
            string? queryType = null,
            int skipCount = 0,
            int maxResultCount = 20)
        {
            var canGetAll = _authorizationService != null ? await _authorizationService.AuthorizeAsync("Workflow.Instance.List") : null;
            if ((userIds == null || userIds.Count == 0) && CurrentUser.Id.HasValue)
            {
                userIds = [CurrentUser.Id.Value];
            }
            if (canGetAll != null && canGetAll.Succeeded && queryType == "all")
            {
                userIds = null;
            }
            Logger.LogInformation("Name:{CurrentUser.Name}",CurrentTenant.Name);
            
            List<WkProcessInstanceDto> result = [];
            var instances = await _wkInstanceRepository.GetMyInstancesWithVersionAsync(
                creatorIds,
                definitionIds,
                definitionVersions,
                instanceData,
                userIds ?? [],
                reference,
                status,
                skipCount,
                maxResultCount);
            var count = await _wkInstanceRepository.GetMyInstancesCountWithVersionAsync(
                creatorIds,
                definitionIds,
                definitionVersions,
                instanceData,
                userIds ?? [],
                reference, status);
            foreach (var instance in instances)
            {
                var processInstance = instance.ToProcessInstanceDto();
                result.Add(processInstance);
            }
            return new PagedResultDto<WkProcessInstanceDto>(count, result);
        }
        
        /// <summary>
        /// 获取指定模板版本的实例
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        public virtual async Task<List<WkProcessInstanceDto>> GetInstancesByDefinitionVersionAsync(Guid definitionId, int version)
        {
            var instances = await _wkInstanceRepository.GetInstancesByDefinitionVersionAsync(definitionId, version);
            var result = new List<WkProcessInstanceDto>();
            foreach (var instance in instances)
            {
                var processInstance = instance.ToProcessInstanceDto();
                result.Add(processInstance);
            }
            return result;
        }
        
        /// <summary>
        /// 获取运行中的实例（按模板版本）
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        public virtual async Task<List<WkProcessInstanceDto>> GetRunningInstancesByVersionAsync(Guid definitionId, int version)
        {
            var instances = await _wkInstanceRepository.GetRunningInstancesByVersionAsync(definitionId, version);
            var result = new List<WkProcessInstanceDto>();
            foreach (var instance in instances)
            {
                var processInstance = instance.ToProcessInstanceDto();
                result.Add(processInstance);
            }
            return result;
        }
        /// <summary>
        /// 通过业务编号获得实例详细信息
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public virtual async Task<WkCurrentInstanceDetailsDto> GetWkInstanceAsync(string reference)
        {
            var instance = await _wkInstanceRepository.GetByReferenceAsync(reference) ?? throw new UserFriendlyException(message: $"不存在reference为：[{reference}]流程实例！");
            var businessData = JsonSerializer.Deserialize<Dictionary<string, object>>(instance.Data);
            WkExecutionPointer pointer = instance.ExecutionPointers.First(d => d.Status != PointerStatus.Complete);
            var errors = await _errorRepository.GetListByIdAsync(instance.Id, pointer.Id);
            return instance.ToWkInstanceDetailsDto(ObjectMapper, businessData, pointer, CurrentUser.Id, errors);
        }
        /// <summary>
        /// 签收实例
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public virtual async Task RecipientInstanceAsync(Guid workflowId)
        {
            var userId = CurrentUser.Id;
            //Guid? userId = new Guid("3a140076-3f3e-0ae6-56ad-2c3f1c06508f");
            if (userId.HasValue)
            {
                await _wkInstanceRepository.RecipientExePointerAsync(workflowId, CurrentUser, true);
            }
            else
            {
                throw new UserFriendlyException(message: "未获取到当前登录用户！");
            }
        }
        /// <summary>
        /// 获得实例详细信息
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="pointerId"></param>
        /// <returns></returns>
        //[AllowAnonymous]
        public virtual async Task<WkCurrentInstanceDetailsDto> GetInstanceAsync(Guid workflowId, Guid? pointerId)
        {
            var instance = await _wkInstanceRepository.FindAsync(workflowId) ?? throw new UserFriendlyException(message: $"不存在Id为：[{workflowId}]流程实例！");
            var businessData = JsonSerializer.Deserialize<Dictionary<string, object>>(instance.Data);

            WkExecutionPointer pointer;
            var executionPointers = instance.ExecutionPointers.ToList();
            if (instance.Status == WorkflowStatus.Complete)
            {
                var lastPointer = executionPointers.Aggregate((a, b) => a.StepId > b.StepId ? a : b);
                var nodeMap = instance.WkDefinition.Nodes.ToDictionary(n => n.Name);
                var lastStep = nodeMap[lastPointer.StepName];

                if (lastStep.StepNodeType == StepNodeType.End)
                {
                    pointer = !string.IsNullOrEmpty(lastPointer.PredecessorId)
                        ? executionPointers.First(p => p.Id == new Guid(lastPointer.PredecessorId))
                        : executionPointers.First(p => p.StepId == lastPointer.StepId - 1);
                }
                else
                {
                    pointer = lastPointer;
                }
            }
            else
            {
                pointer = pointerId.HasValue
                    ? executionPointers.First(p => p.Id == pointerId.Value)
                    : executionPointers.First(p => p.Status != PointerStatus.Complete);
            }

            var errors = await _errorRepository.GetListByIdAsync(workflowId, pointer.Id);
            var result = instance.ToWkInstanceDetailsDto(ObjectMapper, businessData, pointer, CurrentUser.Id, errors);
            return result;
        }
        /// <summary>
        /// 获得实例节点（包含已完成及正在办理的节点）
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<List<WkNodeTreeDto>> GetInstanceNodesAsync(Guid workflowId)
        {
            var instance = await _wkInstanceRepository.FindAsync(workflowId) ?? throw new UserFriendlyException(message: $"不存在Id为：[{workflowId}]流程实例！");
            var result = new List<WkNodeTreeDto>();
            string? preId = null;
            while (instance.ExecutionPointers.Any(d => d.PredecessorId == preId))
            {
                var node = instance.ExecutionPointers.First(d => d.PredecessorId == preId);
                var defNode = instance.WkDefinition.Nodes.First(d => d.Name == node.StepName);
                string? name = null;
                if (node.WkCandidates.Count > 0)
                {
                    name = node.WkCandidates.FirstOrDefault(c => c.UserName == node.Recipient)?.DisplayUserName;
                }
                name ??= node.Recipient;
                if (defNode.StepNodeType == StepNodeType.Activity)
                {
                    result.Add(new WkNodeTreeDto()
                    {
                        Key = node.Id,
                        Title = defNode.DisplayName,
                        Selected = node.Status != PointerStatus.Complete,
                        Name = node.StepName,
                        Receiver = node.Recipient,
                        CommitmentDeadline = node.CommitmentDeadline,
                        Status = (int)node.Status,
                        ReceiverName = name,
                        WkCandidates = ObjectMapper.Map<ICollection<ExePointerCandidate>, ICollection<WkPointerCandidateDto>>(node.WkCandidates)
                    });
                }
                preId = node.Id.ToString();
            }
            return result;
        }
        /// <summary>
        /// 通过实例Id获取可选择的人员
        /// </summary>
        /// <param name="wkInstanceId"></param>
        /// <returns></returns>
        public virtual async Task<ICollection<WkCandidateDto>> GetCandidatesAsync(Guid wkInstanceId)
        {
            var instance = await _wkInstanceRepository.FindAsync(wkInstanceId) ?? throw new UserFriendlyException(message: $"不存在Id为：[{wkInstanceId}]流程实例！");
            var currentPointer = instance.ExecutionPointers.First(p => p.Status != PointerStatus.Complete);
            var currentNode = instance.WkDefinition.Nodes.First(n => n.Name == currentPointer.StepName);
            var nextNode = currentNode.NextNodes.First(n => n.NodeType == WkRoleNodeType.Forward);
            // 使用实例的版本号获取候选人，而不是最新版本
            var candidates = await _wkDefinition.GetCandidatesAsync(instance.WkDifinitionId, nextNode.NextNodeName, instance.Version);
            return ObjectMapper.Map<ICollection<WkNodeCandidate>, ICollection<WkCandidateDto>>(candidates);
        }
        /// <summary>
        /// 流程实例添加业务数据
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task UpdateInstanceBusinessDataAsync(Guid workflowId, IDictionary<string, object> data)
        {
            await _wkInstanceRepository.UpdateDataAsync(workflowId, data);
        }
        /// <summary>
        /// 关注实例（取消关注）
        /// </summary>
        /// <param name="pointerId"></param>
        /// <param name="follow"></param>
        /// <returns></returns>
        public virtual async Task FollowAsync(Guid pointerId, bool follow)
        {
            if (CurrentUser.Id.HasValue)
            {
                var pointer = await _wkExecutionPointerRepository.FindAsync(pointerId) ?? throw new UserFriendlyException(message: $"不存在Id为：[{pointerId}]执行节点！");
                var exeCandidate = pointer.WkCandidates.Where(d => d.CandidateId == CurrentUser.Id.Value).FirstOrDefault();
                if (exeCandidate != null)
                {
                    await exeCandidate.SetFollow(follow);
                    await _wkExecutionPointerRepository.UpdateAsync(pointer);
                }
            }
        }
        /// <summary>
        /// 标记初始化物料
        /// </summary>
        /// <param name="wkinstanceId"></param>
        /// <param name="executionPointerId"></param>
        /// <returns></returns>
        public virtual async Task InitMaterialsAsync(Guid executionPointerId)
        {
            await _wkExecutionPointerRepository.InitMaterialsAsync(executionPointerId);
        }
        /// <summary>
        /// 计算我的工作状态数量
        /// </summary>
        /// <param name="transactorId"></param>
        /// <returns></returns>
        public virtual async Task<List<ProcessingStatusStatDto>> GetProcessingStatusStatListAsync(Guid? transactorId)
        {
            if (CurrentUser.Id.HasValue)
            {
                transactorId = CurrentUser.Id.Value;
            }
            if (!transactorId.HasValue) throw new UserFriendlyException(message: "计算我的工作状态数量，办理人为空！");
            return ObjectMapper.Map<List<ProcessingStatusStat>, List<ProcessingStatusStatDto>>(
                await _wkInstanceRepository.GetProcessingStatusStatListAsync(transactorId.Value));
        }
        public virtual async Task<List<ProcessTypeStatDto>> GetBusinessTypeListAsync()
        {
            var result = await _wkInstanceRepository.GetBusinessTypeListAsync();
            return ObjectMapper.Map<List<ProcessTypeStat>, List<ProcessTypeStatDto>>(result);
        }
        public virtual async Task<List<ProcessTypeStatDto>> GetProcessTypeStatListAsync()
        {
            var entitys = await _wkInstanceRepository.GetProcessTypeStatListAsync();
            var result = ObjectMapper.Map<List<ProcessTypeStat>, List<ProcessTypeStatDto>>(entitys);
            // 创建一个包含所有月份的列表
            var allMonths = Enumerable.Range(1, 12).Select(m => m.ToString().PadLeft(2, '0')).ToList();

            // 创建一个字典来存储每个ProcessType的全年数据
            var fullYearData = new Dictionary<string, List<ProcessTypeStatDto>>();

            foreach (var item in result)
            {
                if (!fullYearData.TryGetValue(item.PClassification, out List<ProcessTypeStatDto>? value))
                {
                    value = ([]);
                    fullYearData[item.PClassification] = value;
                }

                value.Add(item);
            }

            // 确保每个ProcessType都包含全年的每个月份
            var finalResult = new List<ProcessTypeStatDto>();
            foreach (var processType in fullYearData.Keys)
            {
                foreach (var month in allMonths)
                {
                    var existingStat = fullYearData[processType].FirstOrDefault(s => s.SClassification == month);
                    if (existingStat != null)
                    {
                        finalResult.Add(existingStat);
                    }
                    else
                    {
                        // 如果数据库中没有该月份的数据，则添加计数为0的条目
                        finalResult.Add(new ProcessTypeStatDto
                        {
                            PClassification = processType,
                            SClassification = month,
                            Count = 0
                        });
                    }
                }
            }
            return finalResult;
        }
        public virtual async Task AuditAsync(Guid wkInstanceId, Guid executionPointerId, string remark)
        {
            if (CurrentUser.Id.HasValue && !string.IsNullOrEmpty(CurrentUser.UserName))
            {
                var entity = await _wkAuditor.GetAuditorAsync(executionPointerId, CurrentUser.Id.Value);
                if (entity != null)
                {
                    await entity.Audit(DateTime.Now, remark);
                    await _wkAuditor.UpdateAsync(entity);
                }
                else
                {
                    var auditorInstance = new WkAuditor(
                    wkInstanceId,
                    executionPointerId,
                    CurrentUser.UserName,
                    userId: CurrentUser.Id.Value,
                    status: EnumAuditStatus.Unapprove);
                    await auditorInstance.Audit(DateTime.Now, remark);
                    await _wkAuditor.InsertAsync(auditorInstance);
                }
            }
            else
            {
                throw new UserFriendlyException(message: "获取当前用户失败！");
            }
        }
        public virtual async Task<WkAuditorDto> GetAuditAsync(Guid executionPointerId)
        {
            if (CurrentUser.Id.HasValue)
            {
                var entity = await _wkAuditor.GetAuditorAsync(executionPointerId, CurrentUser.Id.Value) ?? throw new UserFriendlyException(message: $"Id为：[{executionPointerId}]的执行点不存在！");
                return ObjectMapper.Map<WkAuditor, WkAuditorDto>(entity);
            }
            else
            {
                throw new UserFriendlyException(message: "获取当前用户失败！");
            }
        }
        /// <summary>
        /// update execution pointer data
        /// </summary>
        /// <param name="executionPointerId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public virtual async Task SaveExecutionPointerDataAsync(Guid executionPointerId, IDictionary<string, object> data)
        {
            var dics = data.ToDictionary(x => x.Key, x => JsonSerializer.Serialize(x.Value));
            await _wkExecutionPointerRepository.UpdateDataAsync(executionPointerId, dics);
        }
        public virtual Task RetryAsync(Guid executionPointerId)
        {
            return _wkExecutionPointerRepository.RetryAsync(executionPointerId);
        }
    }
}
