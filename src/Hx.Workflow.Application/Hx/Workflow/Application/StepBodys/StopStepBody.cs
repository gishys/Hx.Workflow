using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.EventBus.Local;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
    internal class StopStepBody : StepBodyAsync
    {
        private readonly ILocalEventBus _localEventBus;
        private readonly IWkInstanceRepository _wkInstance;
        public StopStepBody(ILocalEventBus localEventBus, IWkInstanceRepository wkInstance)
        {
            _localEventBus = localEventBus;
            _wkInstance = wkInstance;
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
            try
            {
                await _localEventBus.PublishAsync(new StopStepBodyChangeEvent(
                    new Guid(context.Workflow.Id),
                    instance.Reference,
                    context.Workflow.Data as IDictionary<string, object>));
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"StopStepBodyChangeEvent 改变事件异常：{ex.Message}");
            }
            return ExecutionResult.Next();
        }
    }
}
