using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    internal class StopStepBody(ILocalEventBus localEventBus, IWkInstanceRepository wkInstance) : StepBodyAsync, ITransientDependency
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        private readonly ILocalEventBus _localEventBus = localEventBus;
        private readonly IWkInstanceRepository _wkInstance = wkInstance;
        public const string Name = "StopStepBody";
        public const string DisplayName = "结束";

        /// <summary>
        /// 审核人
        /// </summary>
        public string Candidates { get; set; }
        /// <summary>
        /// 分支判断
        /// </summary>
        public string? DecideBranching { get; set; }
        public async override Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var instance = await _wkInstance.FindAsync(new Guid(context.Workflow.Id)) ?? throw new UserFriendlyException(message: $"Id为：{context.Workflow.Id}的实例不存在！");
            var dataDict = context.Workflow.Data as IDictionary<string, object> ?? throw new InvalidOperationException("Workflow.Data 必须为字典类型");
            try
            {
                await _localEventBus.PublishAsync(new StopStepBodyChangeEvent(
                    new Guid(context.Workflow.Id),
                    instance.Reference,
                    dataDict));
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(message: $"StopStepBodyChangeEvent 改变事件异常：{ex.Message}");
            }
            return ExecutionResult.Next();
        }
    }
}
