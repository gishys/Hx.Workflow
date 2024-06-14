using Hx.Workflow.Application.BusinessModule;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.StepBodys;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
    public class GeneralAuditingStepBody : StepBody, ITransientDependency
    {
        private const string ActivityName = "GeneralAuditActivity";
        private readonly IWkAuditorRespository _wkAuditor;
        private readonly IWkInstanceRepository _wkInstance;
        private readonly IWkDefinitionRespository _wkDefinition;
        private readonly ICurrentUser _currentUser;
        public GeneralAuditingStepBody(
            IWkAuditorRespository wkAuditor,
            IWkInstanceRepository wkInstance,
            IWkDefinitionRespository wkDefinition,
            ICurrentUser currentUser)
        {
            _wkAuditor = wkAuditor;
            _wkInstance = wkInstance;
            _wkDefinition = wkDefinition;
            _currentUser = currentUser;
        }
        /// <summary>
        /// 审核人
        /// </summary>
        public string Candidates { get; set; }
        /// <summary>
        /// 分支判断
        /// </summary>
        public string DecideBranching { get; set; } = null;
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            var instance = _wkInstance.FindAsync(new Guid(context.Workflow.Id)).Result;
            if (instance.WkDefinition.LimitTime.HasValue)
            {
                var workflowData = new Dictionary<string, object>
                {
                    { "BusinessCommitmentDeadline", context.Workflow.CreateTime.AddMinutes((double)instance.WkDefinition.LimitTime) }
                };
                context.Workflow.Data = workflowData;
            }
            var executionPointer = instance.ExecutionPointers.FirstOrDefault(d => d.Id == new Guid(context.ExecutionPointer.Id));
            if (!executionPointer.EventPublished)
            {
                var auditorInstance =
                    new WkAuditor(
                    instance.Id,
                    executionPointer.Id,
                    null,
                    userId: null,
                    status: EnumAuditStatus.UnAudited);
                var rAuditorEntity = _wkAuditor.InsertAsync(auditorInstance).Result;
                var tempCandidates = Candidates.Split(',');
                if (tempCandidates?.Length > 0)
                {
                    var definition = _wkDefinition.FindAsync(instance.WkDifinitionId).Result;
                    if (definition.WkCandidates?.Count > 0)
                    {
                        var dcandidate = definition.Nodes
                            .First(d => d.Name == executionPointer.StepName)
                            .WkCandidates
                            .Where(d => tempCandidates.Any(f => new Guid(f) == d.CandidateId)).ToList();
                        if (dcandidate?.Count > 0)
                            _wkInstance.UpdateCandidateAsync(instance.Id, executionPointer.Id, dcandidate as ICollection<ExePointerCandidate>);
                    }
                }
                var effectiveData = DateTime.MinValue;
                var executionResult = ExecutionResult.WaitForActivity(
                    ActivityName,
                    null,
                    effectiveData);
                return executionResult;
            }
            var eventData = context.ExecutionPointer.EventData as ActivityResult;
            if (eventData != null)
            {
                var eventInstancePersistData = JsonSerializer.Deserialize<WkInstanceEventData>(JsonSerializer.Serialize(eventData.Data));
                var step = instance.WkDefinition.Nodes.First(d => d.Name == executionPointer.StepName);
                if (!step.NextNodes.Any(d => d.WkConNodeConditions.Any(d => d.Value == eventInstancePersistData.DecideBranching)))
                    throw new UserFriendlyException("参数DecideBranching错误！");
                var auditStatus = eventInstancePersistData.ExecutionType == StepExecutionType.Next ? EnumAuditStatus.Pass : EnumAuditStatus.Unapprove;
                Audit(eventData.Data, executionPointer.Id, auditStatus);
                var evantData = eventData.Data as Dictionary<string, object>;
                var workflowData = new Dictionary<string, object>(context.Workflow.Data as Dictionary<string, object>);
                foreach (var item in evantData)
                {
                    if (!workflowData.ContainsKey(item.Key))
                    {
                        workflowData.Add(item.Key, item.Value);
                    }
                }
                context.Workflow.Data = workflowData;
            }
            else
            {
                throw new UserFriendlyException("提交data不能为空！");
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
        private void Audit(object data, Guid executionId, EnumAuditStatus auditStatus)
        {
            string Remark = null;
            if (data != null)
                AnalysisEventData(ref Remark, data);
            var auditorQueryEntity = _wkAuditor.GetAuditorAsync(executionId).Result;
            if (auditorQueryEntity != null)
            {
                var auditTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                auditorQueryEntity.Audit(auditStatus, auditTime, Remark);
                _wkAuditor.UpdateAsync(auditorQueryEntity);
            }
        }
    }
}