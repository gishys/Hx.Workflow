using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.StepBodys;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp.DependencyInjection;
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
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            var instance = _wkInstance.FindAsync(new Guid(context.Workflow.Id)).Result;
            var executionPointer = instance.ExecutionPointers.FirstOrDefault(d => d.Id == new Guid(context.ExecutionPointer.Id));
            if (!context.ExecutionPointer.EventPublished)
            {
                var auditorInstance =
                    new WkAuditor(
                    instance.Id,
                    executionPointer.Id,
                    null,
                    userId: null,
                    status: Domain.Shared.EnumAuditStatus.UnAudited);
                var rAuditorEntity = _wkAuditor.InsertAsync(auditorInstance).Result;
                var tempCandidates = Candidates.Split(',');
                if (tempCandidates?.Length > 0)
                {
                    var definition = _wkDefinition.FindAsync(instance.WkDifinitionId).Result;
                    if (definition.WkCandidates?.Count > 0)
                    {
                        var dcandidate = definition.WkCandidates.Where(d => tempCandidates.Any(f => new Guid(f) == d.CandidateId)).ToList();
                        if (dcandidate?.Count > 0)
                            _wkInstance.UpdateCandidateAsync(instance.Id, executionPointer.Id, dcandidate);
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
                Audit(eventData.Data, executionPointer.Id);
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
        private void Audit(object data,Guid executionId)
        {
            string Remark = null;
            if (data != null)
                AnalysisEventData(ref Remark, data);
            var auditorQueryEntity = _wkAuditor.GetAuditorAsync(executionId).Result;
            if (auditorQueryEntity != null)
            {
                if (DecideBranching == "BackOff")
                    auditorQueryEntity.Audit(
                        Domain.Shared.EnumAuditStatus.Unapprove,
                        DateTime.Now, Remark);
                else
                    auditorQueryEntity.Audit(
                        Domain.Shared.EnumAuditStatus.Pass,
                        DateTime.Now, Remark);
                _wkAuditor.UpdateAsync(auditorQueryEntity);
            }
        }
    }
}