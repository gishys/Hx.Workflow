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
        ILocalEventBus localEventBus) : StepBodyAsync, ITransientDependency
    {
        private readonly IWkAuditorRespository _wkAuditor = wkAuditor;
        private readonly IWkInstanceRepository _wkInstance = wkInstance;
        private readonly IWkDefinitionRespository _wkDefinition = wkDefinition;
        private readonly ILimitTimeManager _limitTimeManager = limitTimeManager;
        private readonly ILocalEventBus _localEventBus = localEventBus;
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
                if (string.IsNullOrEmpty(Candidates))
                {
                    const string key = "Candidates";

                    if (!dataDict.TryGetValue(key, out object? candidatesValue))
                    {
                        throw new UserFriendlyException(message: "必须提供接收者参数 [Candidates]");
                    }
                    string? candidatesStr = candidatesValue?.ToString();
                    if (string.IsNullOrEmpty(candidatesStr))
                    {
                        throw new UserFriendlyException(message: $"接收者参数格式无效，应为非空字符串 [实际值类型: {candidatesValue?.GetType().Name}]");
                    }

                    Candidates = candidatesStr;
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
                var definition = await _wkDefinition.FindAsync(instance.WkDifinitionId) ?? throw new UserFriendlyException(message: $"Id为：{instance.WkDifinitionId}的流程模板不存在！");
                var pointer = definition.Nodes.FirstOrDefault(d => d.Name == executionPointer.StepName) ?? throw new UserFriendlyException(message: $"在流程({instance.Id})中未找到名称为({executionPointer.StepName})的节点！");
                if (pointer.LimitTime != null)
                {
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
                        if (!string.IsNullOrEmpty(Candidates))
                        {
                            var tempCandidates = Candidates.Split(',');
                            if (tempCandidates?.Length > 0)
                            {
                                dcandidate = [.. pointer.WkCandidates.Where(d => tempCandidates.Any(f => new Guid(f) == d.CandidateId))];
                            }
                            else
                            {
                                throw new UserFriendlyException(message: "未传入正确的接收者！");
                            }
                        }
                        else
                        {
                            throw new UserFriendlyException(message: "未传入正确的接收者！");
                        }
                    }
                    else if (pointer.StepNodeType == StepNodeType.Start)
                    {
                        if (!Guid.TryParse(Candidates, out var candidateId)) throw new UserFriendlyException(message: "未传入正确的接收者！");
                        var defCandidate = definition.WkCandidates.FirstOrDefault(d => d.CandidateId == candidateId);
                        if ((defCandidate == null))
                        {
                            throw new UserFriendlyException(message: $"无权限，请在流程定义中配置Id为（{candidateId}）的权限！");
                        }
                        dcandidate = [new(
                        defCandidate.CandidateId,
                        defCandidate.UserName,
                        defCandidate.DisplayUserName,
                        defCandidate.ExecutorType,
                        defCandidate.DefaultSelection)];
                    }
                    if (dcandidate == null)
                        throw new UserFriendlyException(message: "未传入正确的接收者!");

                    var candidates = dcandidate.ToCandidates();
                    var (instanceId, pointerId) = (instance.Id, executionPointer.Id);

                    // 当存在前置节点时处理特殊逻辑
                    if (executionPointer.PredecessorId != null)
                    {
                        var preNode = instance.ExecutionPointers.FirstOrDefault(
                            x => x.Id.ToString() == executionPointer.PredecessorId);

                        var preStep = preNode != null
                            ? definition.Nodes.FirstOrDefault(x => x.Name == preNode.StepName)
                            : null;

                        if (preStep?.StepNodeType == StepNodeType.Start)
                        {
                            var firstCandidate = candidates.FirstOrDefault() ?? throw new UserFriendlyException(message: "候选人列表不能为空");
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
                        await _wkInstance.UpdateCandidateAsync(instance.Id, executionPointer.Id, ExeCandidateState.Completed);
                    }
                    else
                    {
                        //回退逻辑
                        WkExecutionPointer beRolledBackNode = instance.ExecutionPointers.FirstOrDefault(d => d.StepName == eventPointerEventData.DecideBranching) ?? throw new UserFriendlyException(message: $"在流程({instance.Id})中未找到Id为({executionPointer.PredecessorId})的节点！");
                        NextCandidates = string.Join(",", beRolledBackNode.WkCandidates.Select(d => d.CandidateId).ToList());
                        await _wkInstance.UpdateCandidateAsync(instance.Id, executionPointer.Id, ExeCandidateState.BeRolledBack);
                    }
                    if (!Guid.TryParse(Candidates, out var candidateId)) throw new UserFriendlyException(message: "未传入正确的接收者！");
                    await Audit(eventData.Data, instance.Id, executionPointer, candidateId, auditStatus);
                    foreach (var item in executionPointer.WkCandidates.Where(d => d.CandidateId == new Guid(Candidates)))
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
    }
}
