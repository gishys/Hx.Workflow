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
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
    public class GeneralAuditingStepBody : StepBodyAsync, ITransientDependency
    {
        private const string ActivityName = "GeneralAuditActivity";
        private readonly IWkAuditorRespository _wkAuditor;
        private readonly IWkInstanceRepository _wkInstance;
        private readonly IWkDefinitionRespository _wkDefinition;
        private readonly ILimitTimeManager _limitTimeManager;
        public GeneralAuditingStepBody(
            IWkAuditorRespository wkAuditor,
            IWkInstanceRepository wkInstance,
            IWkDefinitionRespository wkDefinition,
            ILimitTimeManager limitTimeManager)
        {
            _wkAuditor = wkAuditor;
            _wkInstance = wkInstance;
            _wkDefinition = wkDefinition;
            _limitTimeManager = limitTimeManager;
        }
        /// <summary>
        /// 审核人
        /// </summary>
        public string Candidates { get; set; }
        /// <summary>
        /// 分支判断
        /// </summary>
        public string DecideBranching { get; set; } = null;
        public async override Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            try
            {
                var instance = await _wkInstance.FindAsync(new Guid(context.Workflow.Id));
                if (instance.WkDefinition.LimitTime.HasValue)
                {
                    var workflowData = new Dictionary<string, object>
                {
                    { "BusinessCommitmentDeadline",  await _limitTimeManager.GetAsync(context.Workflow.CreateTime,instance.WkDefinition.LimitTime)}
                };
                    context.Workflow.Data = (context.Workflow.Data as IDictionary<string, object>).Cancat(workflowData);
                }
                var executionPointer = instance.ExecutionPointers.FirstOrDefault(d => d.Id == new Guid(context.ExecutionPointer.Id));
                var definition = await _wkDefinition.FindAsync(instance.WkDifinitionId);
                var pointer = definition.Nodes.FirstOrDefault(d => d.Name == executionPointer.StepName);
                if (pointer == null)
                {
                    throw new UserFriendlyException($"在流程({instance.Id})中未找到名称为({executionPointer.StepName})的节点！");
                }
                if (pointer.LimitTime != null)
                {
                    if (context.ExecutionPointer.ExtensionAttributes.ContainsKey("CommitmentDeadline"))
                        context.ExecutionPointer.ExtensionAttributes.Remove("CommitmentDeadline");
                    context.ExecutionPointer.ExtensionAttributes.Add("CommitmentDeadline", DateTime.Now.AddMinutes((double)pointer.LimitTime));
                }
                if (!executionPointer.EventPublished)
                {
                    if (definition == null)
                        throw new UserFriendlyException("获取实例流程模板失败！");
                    if (pointer == null)
                        throw new UserFriendlyException("获取流程节点失败！");
                    List<WkNodeCandidate> dcandidate = null;
                    bool beRolledBack = false;
                    //回退逻辑
                    if (executionPointer.PredecessorId != null)
                    {
                        var preNode = instance.ExecutionPointers.FirstOrDefault(d => d.Id.ToString() == executionPointer.PredecessorId);
                        if (preNode == null)
                        {
                            throw new UserFriendlyException($"在流程({instance.Id})中未找到Id为({executionPointer.PredecessorId})的节点！");
                        }
                        if (preNode.WkCandidates.Any(d => d.ParentState == ExeCandidateState.BeRolledBack))
                        {
                            var beRolledBackNode = instance.ExecutionPointers.FirstOrDefault(d => d.Id.ToString() == preNode.PredecessorId);
                            if (beRolledBackNode == null)
                            {
                                throw new UserFriendlyException($"在流程({instance.Id})中未找到Id为({preNode.PredecessorId})的节点！");
                            }
                            dcandidate = beRolledBackNode.WkCandidates.Select(d =>
                            new WkNodeCandidate(
                                d.CandidateId,
                                d.UserName,
                                d.DisplayUserName,
                                d.ExecutorType,
                                d.DefaultSelection)).ToList();
                            beRolledBack = true;
                        }
                    }
                    if (!beRolledBack)
                    {
                        if (pointer.StepNodeType == StepNodeType.Activity || pointer.StepNodeType == StepNodeType.End)
                        {
                            if (!string.IsNullOrEmpty(Candidates))
                            {
                                var tempCandidates = Candidates.Split(',');
                                if (tempCandidates?.Length > 0)
                                {
                                    dcandidate = pointer.WkCandidates.Where(d => tempCandidates.Any(f => new Guid(f) == d.CandidateId)).ToList();
                                }
                                else
                                {
                                    throw new UserFriendlyException("未传入正确的接收者！");
                                }
                            }
                            else
                            {
                                throw new UserFriendlyException("未传入正确的接收者！");
                            }
                        }
                        else if (pointer.StepNodeType == StepNodeType.Start)
                        {
                            if (!Guid.TryParse(Candidates, out var candidateId)) throw new UserFriendlyException("未传入正确的接收者！");
                            var defCandidate = definition.WkCandidates.FirstOrDefault(d => d.CandidateId == candidateId);
                            if ((defCandidate == null))
                            {
                                throw new UserFriendlyException($"无权限，请在流程定义中配置Id为（{candidateId}）的权限！");
                            }
                            dcandidate = [new(
                        defCandidate.CandidateId,
                        defCandidate.UserName,
                        defCandidate.DisplayUserName,
                        defCandidate.ExecutorType,
                        defCandidate.DefaultSelection)];
                        }
                    }
                    if (dcandidate != null)
                    {
                        await _wkInstance.UpdateCandidateAsync(
                            instance.Id,
                            executionPointer.Id,
                            dcandidate.ToCandidates(),
                            ExePersonnelOperateType.Host);
                    }
                    else
                    {
                        throw new UserFriendlyException("未传入正确的接收者!");
                    }
                    var effectiveData = DateTime.MinValue;
                    var executionResult = ExecutionResult.WaitForActivity(
                        context.ExecutionPointer.Id,
                        null,
                        effectiveData);
                    return executionResult;
                }
                var eventData = context.ExecutionPointer.EventData as ActivityResult;
                if (eventData != null)
                {
                    var eventPointerEventData = JsonSerializer.Deserialize<WkPointerEventData>(JsonSerializer.Serialize(eventData.Data));
                    var step = instance.WkDefinition.Nodes.FirstOrDefault(d => d.Name == executionPointer.StepName);
                    if (step == null)
                    {
                        throw new UserFriendlyException($"在流程({instance.Id})中未找到名称为({executionPointer.StepName})的节点！");
                    }
                    if (step.StepNodeType != StepNodeType.End)
                    {
                        if (!step.NextNodes.Any(d => d.WkConNodeConditions.Any(d => d.Value == eventPointerEventData.DecideBranching)))
                            throw new UserFriendlyException("参数DecideBranching的值不在下一步节点中！");
                    }
                    EnumAuditStatus auditStatus = EnumAuditStatus.Unapprove;
                    if (eventPointerEventData.ExecutionType == StepExecutionType.Forward)
                    {
                        auditStatus = EnumAuditStatus.Pass;
                        await _wkInstance.UpdateCandidateAsync(instance.Id, executionPointer.Id, ExeCandidateState.Completed);
                    }
                    else
                    {
                        await _wkInstance.UpdateCandidateAsync(instance.Id, executionPointer.Id, ExeCandidateState.BeRolledBack);
                    }
                    await Audit(eventData.Data, instance.Id, executionPointer, new Guid(Candidates), auditStatus);
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
                    throw new UserFriendlyException("提交data不能为空！");
                }
                return ExecutionResult.Next();
            }
            catch (Exception ex)
            {

                throw new UserFriendlyException($"{ex.Message}");
            }
        }
        private void AnalysisEventData(ref string Remark, object eventData)
        {
            if (eventData is IDictionary<string, object>)
            {
                var dataDic = eventData as IDictionary<string, object>;
                foreach (var kv in dataDic)
                {
                    switch (kv.Key)
                    {
                        case "DecideBranching":
                            DecideBranching = kv.Value.ToString();
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
            string Remark = null;
            if (data != null)
                AnalysisEventData(ref Remark, data);
            var user = execution.WkCandidates.FirstOrDefault(d => d.CandidateId == candicateId);
            if ((user == null))
            {
                throw new UserFriendlyException("无权限，请在流程定义中配置此人权限！");
            }
            var auditorInstance =
            new WkAuditor(
                instanceId,
                execution.Id,
                user.UserName,
                userId: user.CandidateId,
                status: auditStatus);
            await _wkAuditor.InsertAsync(auditorInstance);
        }
    }
}