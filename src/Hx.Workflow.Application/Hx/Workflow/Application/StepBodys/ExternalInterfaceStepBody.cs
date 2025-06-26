using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    internal class ExternalInterfaceStepBody(ILocalEventBus localEventBus, IWkInstanceRepository wkInstance) : StepBodyAsync, ITransientDependency
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        private readonly ILocalEventBus _localEventBus = localEventBus;
        private readonly IWkInstanceRepository _wkInstance = wkInstance;
        public const string Name = "ExternalInterfaceStepBody";
        public const string DisplayName = "外部系统接口";

        /// <summary>
        /// 审核人
        /// </summary>
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        public string Candidates { get; set; }
        /// <summary>
        /// 下一步接收人
        /// </summary>
        public string NextCandidates { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        /// <summary>
        /// 分支判断
        /// </summary>
        public string? DecideBranching { get; set; }
        public async override Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var instance = await _wkInstance.FindAsync(new Guid(context.Workflow.Id)) ?? throw new UserFriendlyException(message: $"Id为：{context.Workflow.Id}的实例不存在！");
            var dataDict = context.Workflow.Data as IDictionary<string, object> ?? throw new InvalidOperationException("Workflow.Data 必须为字典类型");

            var executionPointer = instance.ExecutionPointers.FirstOrDefault(d => d.Id == new Guid(context.ExecutionPointer.Id)) ?? throw new UserFriendlyException(message: "未找到");
            var step = (instance.WkDefinition.Nodes.FirstOrDefault(d => d.Name == executionPointer.StepName) ?? throw new UserFriendlyException(message: $"在流程({instance.Id})中未找到名称为({executionPointer.StepName})的节点！")) ?? throw new UserFriendlyException(message: "未找到当前节点！");
            var nextStepC = step.NextNodes.FirstOrDefault(n => n.NodeType == WkRoleNodeType.Forward) ?? throw new UserFriendlyException(message: "未找到下一节点！");
            var nextStepName = nextStepC.NextNodeName;
            var stepNext = (instance.WkDefinition.Nodes.FirstOrDefault(d => d.Name == nextStepName) ?? throw new UserFriendlyException(message: $"在流程({instance.Id})中未找到名称为({executionPointer.StepName})的节点！")) ?? throw new UserFriendlyException(message: "未找到当前节点！");
            if (stepNext.StepNodeType != StepNodeType.End)
            {
                var candidate = stepNext.WkCandidates.First();
                NextCandidates = candidate.CandidateId.ToString();
            }
            DecideBranching = nextStepName;
            if (step.StepNodeType != StepNodeType.End)
            {
                if (!step.NextNodes.Any(d => d.Rules.Any(d => d.Value == nextStepName)))
                    throw new UserFriendlyException(message: "参数DecideBranching的值不在下一步节点中！");
            }
            await _wkInstance.UpdateCandidateAsync(instance.Id, executionPointer.Id, ExeCandidateState.Completed);

            return ExecutionResult.Next();
        }
    }
}
