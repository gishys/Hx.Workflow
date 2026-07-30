using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.BusinessModule;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.StepBodys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
    public class GeneralAuditingStepBody(
        IWkAuditorRespository wkAuditor,
        IWkInstanceRepository wkInstance,
        IWkDefinitionRespository wkDefinition,
        ILimitTimeManager limitTimeManager,
        ILocalEventBus localEventBus,
        WorkflowUserContext workflowUserContext) : StepBodyAsync, ITransientDependency
    {
        private readonly IWkAuditorRespository _wkAuditor = wkAuditor;
        private readonly IWkInstanceRepository _wkInstance = wkInstance;
        private readonly IWkDefinitionRespository _wkDefinition = wkDefinition;
        private readonly ILimitTimeManager _limitTimeManager = limitTimeManager;
        private readonly ILocalEventBus _localEventBus = localEventBus;
        private readonly WorkflowUserContext _workflowUserContext = workflowUserContext;
        public const string Name = "GeneralAuditingStepBody";
        public const string DisplayName = "指定用户审核";

        /// <summary>
        /// 审核人
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        public string Candidates { get; set; }
        /// <summary>
        /// 分支判断
        /// </summary>
        public string DecideBranching { get; set; }
        /// <summary>
        /// 下一步接收人
        /// </summary>
        public string NextCandidates {  get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        public async override Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            try
            {
                var dataDict = context.Workflow.Data as IDictionary<string, object> ?? throw new InvalidOperationException("Workflow.Data 必须为字典类型");
                
                // 尝试从工作流数据中获取当前用户信息（用于日志记录和审计）
                var currentUserId = await _workflowUserContext.GetCurrentUserIdAsync(context);
                var currentUserName = WorkflowUserContext.GetCurrentUserName(context);
                
                if (string.IsNullOrWhiteSpace(Candidates))
                {
                    const string key = "Candidates";

                    if (dataDict.TryGetValue(key, out object? candidatesValue))
                    {
                        string? candidatesStr = candidatesValue?.ToString();
                        if (!string.IsNullOrWhiteSpace(candidatesStr))
                        {
                            Candidates = candidatesStr;
                        }
                    }
                }
                var instance = await _wkInstance.FindAsync(new Guid(context.Workflow.Id)) ?? throw new UserFriendlyException(message: $"Id为：{context.Workflow.Id}的实例不存在！");
                try
                {
                    await _localEventBus.PublishAsync(new WkGeneralAuditStepBodyChangeEvent(
                        new Guid(context.Workflow.Id),
                        instance.Reference,
                        dataDict));
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException(message: $"WkGeneralAuditStepBodyChangeEvent 改变事件异常：{ex.Message}");
                }
                if (instance.WkDefinition.LimitTime is int limitTime)
                {
                    DateTime? deadline = await _limitTimeManager.GetAsync(context.Workflow.CreateTime, limitTime);
                    dataDict["BusinessCommitmentDeadline"] = deadline;
                }
                var executionPointer = instance.ExecutionPointers.FirstOrDefault(d => d.Id == new Guid(context.ExecutionPointer.Id)) ?? throw new UserFriendlyException(message: $"Id为：{context.ExecutionPointer.Id}的执行点不存在！");
                // 使用实例的版本号获取模板定义，而不是最新版本
                var definition = await _wkDefinition.GetDefinitionAsync(instance.WkDifinitionId, instance.Version) ?? throw new UserFriendlyException(message: $"Id为：{instance.WkDifinitionId}，版本为：{instance.Version}的流程模板不存在！");
                var pointer = definition.Nodes.FirstOrDefault(d => d.Name == executionPointer.StepName) ?? throw new UserFriendlyException(message: $"在流程({instance.Id})中未找到名称为({executionPointer.StepName})的节点！");
                if (pointer.LimitTime != null)
                {
                    context.ExecutionPointer.ExtensionAttributes ??= new Dictionary<string, object>();
                    context.ExecutionPointer.ExtensionAttributes.Remove("CommitmentDeadline");
                    context.ExecutionPointer.ExtensionAttributes.Add("CommitmentDeadline", DateTime.Now.AddMinutes((double)pointer.LimitTime));
                }
                if (!executionPointer.EventPublished)
                {
                    if (definition == null)
                        throw new UserFriendlyException(message: "获取实例流程模板失败！");
                    if (pointer == null)
                        throw new UserFriendlyException(message: "获取流程节点失败！");
                    List<WkNodeCandidate>? dcandidate = null;
                    if (pointer.StepNodeType == StepNodeType.Activity || pointer.StepNodeType == StepNodeType.End)
                    {
                        var nodeCandidates = pointer.WkCandidates ?? [];
                        if (nodeCandidates.Count == 0)
                        {
                            throw new UserFriendlyException(message: "当前流程节点未配置接收人。");
                        }

                        if (!string.IsNullOrWhiteSpace(Candidates))
                        {
                            var candidateIds = ParseCandidateIds(Candidates);
                            if (candidateIds.Count == 0)
                            {
                                throw new UserFriendlyException(message: "接收人参数格式无效。");
                            }

                            var candidateIdSet = candidateIds.ToHashSet();
                            dcandidate = [.. nodeCandidates.Where(d => candidateIdSet.Contains(d.CandidateId))];
                            if (dcandidate.Count == 0)
                            {
                                throw new UserFriendlyException(message: "传入的接收人不在当前流程节点配置的接收人范围内。");
                            }
                        }
                        else
                        {
                            dcandidate = [.. nodeCandidates];
                        }
                    }
                    else if (pointer.StepNodeType == StepNodeType.Start)
                    {
                        var candidateIds = ParseCandidateIds(Candidates);
                        if (candidateIds.Count == 0)
                        {
                            var currentCandidateId = await _workflowUserContext.GetCurrentUserIdAsync(context);
                            if (!currentCandidateId.HasValue)
                            {
                                throw new UserFriendlyException(message: "未传入正确的接收者！");
                            }

                            candidateIds = [currentCandidateId.Value];
                        }

                        var defCandidates = candidateIds
                            .Select(id => (definition.WkCandidates ?? []).FirstOrDefault(d => d.CandidateId == id))
                            .ToList();
                        if (defCandidates.Any(d => d == null))
                        {
                            var invalidIds = defCandidates.Select((d, i) => (d, i)).Where(x => x.d == null).Select(x => candidateIds[x.i]).ToList();
                            throw new UserFriendlyException(message: $"无权限，请在流程定义中配置以下候选人ID的权限：{string.Join(", ", invalidIds)}");
                        }
                        dcandidate = [.. defCandidates
                            .Where(d => d != null)
                            .Select(d => new WkNodeCandidate(d!.CandidateId, d.UserName, d.DisplayUserName, d.ExecutorType, d.DefaultSelection))];
                    }
                    if (dcandidate == null)
                        throw new UserFriendlyException(message: "未传入正确的接收者!");

                    var candidates = dcandidate.ToCandidates();
                    var (instanceId, pointerId) = (instance.Id, executionPointer.Id);

                    // 当存在前置节点时处理特殊逻辑：仅当只有一个候选人时默认指定其为接收人并设为待完成；多人时全部保持待接收
                    if (executionPointer.PredecessorId != null)
                    {
                        var preNode = instance.ExecutionPointers.FirstOrDefault(
                            x => x.Id.ToString() == executionPointer.PredecessorId);

                        var preStep = preNode != null
                            ? definition.Nodes.FirstOrDefault(x => x.Name == preNode.StepName)
                            : null;

                        if (preStep?.StepNodeType == StepNodeType.Start && candidates.Count == 1)
                        {
                            var firstCandidate = candidates.First();
                            firstCandidate.SetParentState(ExeCandidateState.Pending);
                            await _wkInstance.UpdateCandidateAsync(
                                instanceId, pointerId, candidates, ExePersonnelOperateType.Host);
                            await _wkInstance.RecipientExePointerAsync(
                                instanceId, pointerId, firstCandidate.UserName, firstCandidate.CandidateId);
                        }
                    }

                    // 通用处理逻辑
                    await _wkInstance.UpdateCandidateAsync(
                        instanceId, pointerId, candidates, ExePersonnelOperateType.Host);
                    var effectiveData = DateTime.MinValue;
                    var executionResult = ExecutionResult.WaitForActivity(
                        context.ExecutionPointer.Id,
                        null,
                        effectiveData);
                    return executionResult;
                }
                if (context.ExecutionPointer.EventData is ActivityResult eventData)
                {
                    var eventPointerEventData = JsonSerializer.Deserialize<WkPointerEventData>(JsonSerializer.Serialize(eventData.Data)) ?? throw new InvalidOperationException("事件数据缺少DecideBranching和ExecutionType！");
                    var step = instance.WkDefinition.Nodes.FirstOrDefault(d => d.Name == executionPointer.StepName) ?? throw new UserFriendlyException(message: $"在流程({instance.Id})中未找到名称为({executionPointer.StepName})的节点！");
                    if (!string.IsNullOrEmpty(eventPointerEventData.Candidates))
                        NextCandidates = eventPointerEventData.Candidates;
                    if (step.StepNodeType != StepNodeType.End)
                    {
                        if (!step.NextNodes.Any(d => d.Rules.Any(d => d.Value == eventPointerEventData.DecideBranching)))
                            throw new UserFriendlyException(message: "参数DecideBranching的值不在下一步节点中！");
                    }
                    EnumAuditStatus auditStatus = EnumAuditStatus.Unapprove;
                    if (eventPointerEventData.ExecutionType == StepExecutionType.Forward)
                    {
                        auditStatus = EnumAuditStatus.Pass;
                    }
                    else
                    {
                        //回退逻辑
                        WkExecutionPointer beRolledBackNode = instance.ExecutionPointers.FirstOrDefault(d => d.StepName == eventPointerEventData.DecideBranching)
                            ?? throw new UserFriendlyException(message: $"驳回失败：在流程实例（{instance.Id}）中未找到步骤名称为「{eventPointerEventData.DecideBranching}」的执行记录，驳回目标节点必须是该实例已执行过的节点。");
                        NextCandidates = string.Join(",", beRolledBackNode.WkCandidates.Select(d => d.CandidateId).ToList());
                    }
                    var candidateIdsForAudit = ParseCandidateIds(Candidates);
                    var candidateId = await ResolveAuditCandidateIdAsync(context, executionPointer, candidateIdsForAudit);

                    await Audit(eventData.Data, instance.Id, executionPointer, candidateId, auditStatus);
                    var candidateIdSet = candidateIdsForAudit.Count > 0
                        ? candidateIdsForAudit.ToHashSet()
                        : new HashSet<Guid> { candidateId };
                    foreach (var item in executionPointer.WkCandidates.Where(d => candidateIdSet.Contains(d.CandidateId)))
                    {
                        if (eventPointerEventData.ExecutionType == StepExecutionType.Forward)
                        {
                            item.SetParentState(ExeCandidateState.Completed);
                        }
                        else
                        {
                            item.SetParentState(ExeCandidateState.BeRolledBack);
                        }
                    }
                    await _wkInstance.UpdateAsync(instance);
                    if (executionPointer.WkCandidates.Any(d =>
                    (d.ExeOperateType == ExePersonnelOperateType.Countersign ||
                    d.ExeOperateType == ExePersonnelOperateType.Host) &&
                    (d.ParentState == ExeCandidateState.Pending ||
                    d.ParentState == ExeCandidateState.Waiting ||
                    d.ParentState == ExeCandidateState.WaitingReceipt)))
                    {
                        var effectiveData = DateTime.MinValue;
                        var executionResult = ExecutionResult.WaitForActivity(
                            context.ExecutionPointer.Id,
                            null,
                            effectiveData);
                        return executionResult;
                    }
                }
                else
                {
                    throw new UserFriendlyException(message: "提交data不能为空！");
                }
                return ExecutionResult.Next();
            }
            catch (Exception ex)
            {

                throw new UserFriendlyException(message: $"{ex.Message}");
            }
        }
        private void AnalysisEventData(ref string? Remark, object eventData)
        {
            if (eventData is IDictionary<string, object>)
            {
                if (eventData is not IDictionary<string, object> dataDic) return;
                foreach (var kv in dataDic)
                {
                    switch (kv.Key)
                    {
                        case "DecideBranching":
                            var value = kv.Value.ToString();
                            if (value != null)
                                DecideBranching = value;
                            break;
                        case "Remark":
                            Remark = kv.Value.ToString();
                            break;
                    }
                }
            }
        }
        private async Task Audit(object data, Guid instanceId, WkExecutionPointer execution, Guid candicateId, EnumAuditStatus auditStatus)
        {
            string? Remark = null;
            if (data != null)
                AnalysisEventData(ref Remark, data);
            var user = execution.WkCandidates.FirstOrDefault(d => d.CandidateId == candicateId);
            if ((user == null))
            {
                throw new UserFriendlyException(message: "无权限，请在流程定义中配置此人权限！");
            }
            // 审核记录中的用户就是本次节点的实际办理人。未签收直接办理时补齐接收人，
            // 并统一记录提交人，避免流程轨迹退回展示全部候选人。
            if (!execution.RecipientId.HasValue)
            {
                await execution.SetRecipientInfo(user.UserName, user.CandidateId);
            }
            await execution.SetSubmitterInfo(user.UserName, user.CandidateId);
            var entity = await _wkAuditor.GetAuditorAsync(execution.Id, user.CandidateId);
            if (entity == null)
            {
                var auditorInstance = new WkAuditor(
                    instanceId,
                    execution.Id,
                    user.UserName,
                    userId: user.CandidateId,
                    status: auditStatus);
                if (!string.IsNullOrEmpty(Remark))
                    await auditorInstance.Audit(DateTime.Now, remark: Remark);
                await _wkAuditor.InsertAsync(auditorInstance);
            }
            else
            {
                await entity.Audit(auditStatus);
                await _wkAuditor.UpdateAsync(entity);
            }
        }

        private async Task<Guid> ResolveAuditCandidateIdAsync(
            IStepExecutionContext context,
            WkExecutionPointer executionPointer,
            IReadOnlyCollection<Guid> candidateIdsForAudit)
        {
            var currentUserId = await _workflowUserContext.GetCurrentUserIdAsync(context);
            if (currentUserId.HasValue && executionPointer.WkCandidates.Any(d => d.CandidateId == currentUserId.Value))
            {
                return currentUserId.Value;
            }

            if (candidateIdsForAudit.Count > 0)
            {
                var candidateId = candidateIdsForAudit.FirstOrDefault(id =>
                    executionPointer.WkCandidates.Any(d => d.CandidateId == id));
                if (candidateId != Guid.Empty)
                {
                    return candidateId;
                }

                throw new UserFriendlyException(message: "未传入正确的接收者！");
            }

            var pendingCandidates = executionPointer.WkCandidates
                .Where(d => d.ParentState == ExeCandidateState.Pending)
                .ToList();
            if (pendingCandidates.Count == 1)
            {
                return pendingCandidates[0].CandidateId;
            }

            if (executionPointer.WkCandidates.Count == 1)
            {
                return executionPointer.WkCandidates.First().CandidateId;
            }

            throw new UserFriendlyException(message: "未传入正确的接收者！");
        }

        /// <summary>
        /// 解析候选人参数字符串，支持英文逗号分隔的多个 GUID。
        /// </summary>
        private static List<Guid> ParseCandidateIds(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return [];
            var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var list = new List<Guid>(parts.Length);
            foreach (var part in parts)
            {
                if (Guid.TryParse(part, out var id))
                    list.Add(id);
            }
            return list;
        }
    }
}
