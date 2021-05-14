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
        public GeneralAuditingStepBody(
            IWkAuditorRespository wkAuditor,
            IWkInstanceRepository wkInstance)
        {
            _wkAuditor = wkAuditor;
            _wkInstance = wkInstance;
        }
        /// <summary>
        /// 审核人
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 分支判断
        /// </summary>
        public string DecideBranching { get; set; }
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Guid.TryParse(UserId, out Guid userId);
            if (!context.ExecutionPointer.EventPublished)
            {
                var instance = _wkInstance.FindAsync(new Guid(context.Workflow.Id)).Result;
                var executionPointer = instance.ExecutionPointers.FirstOrDefault(d => d.Id == new Guid(context.ExecutionPointer.Id));
                var auditorInstance =
                    new WkAuditor(
                    instance.Id,
                    executionPointer.Id,
                    userId,
                    "");
                var rAuditorEntity = _wkAuditor.InsertAsync(auditorInstance).Result;
                var effectiveData = DateTime.MinValue;
                IDictionary<string, object> subscriptionData = new Dictionary<string, object>();
                subscriptionData.Add("DecideBranching", "step.Result");
                var executionResult = ExecutionResult.WaitForActivity(
                    ActivityName,
                    null,
                    effectiveData);
                return executionResult;
            }
            var eventData = context.ExecutionPointer.EventData as ActivityResult;
            if (eventData != null)
            {
                if (eventData.Data?.ToString().Length > 0)
                    DecideBranching = eventData.Data.ToString();
            }
            return ExecutionResult.Next();
        }
    }
}