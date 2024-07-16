using Hx.Workflow.Domain;
using Hx.Workflow.Domain.BusinessModule;
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
        public GeneralAuditingStepBody(
            IWkAuditorRespository wkAuditor,
            IWkInstanceRepository wkInstance,
            IWkDefinitionRespository wkDefinition)
        {
            _wkAuditor = wkAuditor;
            _wkInstance = wkInstance;
            _wkDefinition = wkDefinition;
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
            var instance = await _wkInstance.FindAsync(new Guid(context.Workflow.Id));
            if (instance.WkDefinition.LimitTime.HasValue)
            {
                var workflowData = new Dictionary<string, object>
                {
                    { "BusinessCommitmentDeadline", context.Workflow.CreateTime.AddMinutes((double)instance.WkDefinition.LimitTime) }
                };
                context.Workflow.Data = (context.Workflow.Data as IDictionary<string, object>).Cancat(workflowData);
            }
            var executionPointer = instance.ExecutionPointers.FirstOrDefault(d => d.Id == new Guid(context.ExecutionPointer.Id));
            var definition = await _wkDefinition.FindAsync(instance.WkDifinitionId);
            var pointer = definition.Nodes.First(d => d.Name == executionPointer.StepName);
            if (pointer.LimitTime != null)
            {
                if (context.ExecutionPointer.ExtensionAttributes.ContainsKey("CommitmentDeadline"))
                    context.ExecutionPointer.ExtensionAttributes.Remove("CommitmentDeadline");
                context.ExecutionPointer.ExtensionAttributes.Add("CommitmentDeadline", DateTime.Now.AddMinutes((double)pointer.LimitTime));
            }
            if (pointer.StepNodeType != StepNodeType.End)
            {
                if (!executionPointer.EventPublished)
                {
                    if (definition == null)
                        throw new UserFriendlyException("获取实例流程模板失败！");
                    if (pointer == null)
                        throw new UserFriendlyException("获取流程节点失败！");
                    List<WkNodeCandidate> dcandidate;
                    if (pointer.StepNodeType == StepNodeType.Activity)
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
                    else
                    {
                        if (!Guid.TryParse(Candidates, out var candidateId)) throw new UserFriendlyException("未传入正确的接收者！");
                        var defCandidate = definition.WkCandidates.First(d => d.CandidateId == candidateId);
                        var auditorQueryEntity = await _wkAuditor.GetAuditorAsync(executionPointer.Id);
                        if (auditorQueryEntity == null)
                        {
                            var auditorInstance =
                                new WkAuditor(
                                    instance.Id,
                                    executionPointer.Id,
                                    defCandidate.UserName,
                                    userId: defCandidate.CandidateId,
                                    status: EnumAuditStatus.UnAudited);
                            await _wkAuditor.InsertAsync(auditorInstance);
                        }
                        dcandidate = [new(defCandidate.CandidateId, defCandidate.UserName, defCandidate.DisplayUserName, defCandidate.DefaultSelection)];
                    }
                    await _wkInstance.UpdateCandidateAsync(
                        instance.Id,
                        executionPointer.Id,
                        dcandidate.ToCandidates());
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
                    var step = instance.WkDefinition.Nodes.First(d => d.Name == executionPointer.StepName);
                    if (!step.NextNodes.Any(d => d.WkConNodeConditions.Any(d => d.Value == eventPointerEventData.DecideBranching)))
                        throw new UserFriendlyException("参数DecideBranching错误！");
                    var auditStatus = eventPointerEventData.ExecutionType == StepExecutionType.Next ? EnumAuditStatus.Pass : EnumAuditStatus.Unapprove;
                    await Audit(eventData.Data, executionPointer.Id, auditStatus);
                }
                else
                {
                    throw new UserFriendlyException("提交data不能为空！");
                }
            }
            return ExecutionResult.Next();
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
        private async Task Audit(object data, Guid executionId, EnumAuditStatus auditStatus)
        {
            string Remark = null;
            if (data != null)
                AnalysisEventData(ref Remark, data);
            var auditorQueryEntity = await _wkAuditor.GetAuditorAsync(executionId);
            if (auditorQueryEntity != null)
            {
                await auditorQueryEntity.Audit(auditStatus, DateTime.Now, Remark);
                await _wkAuditor.UpdateAsync(auditorQueryEntity);
            }
        }
    }
}