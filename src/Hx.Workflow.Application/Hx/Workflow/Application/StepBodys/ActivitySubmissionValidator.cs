using Hx.Workflow.Domain.BusinessModule;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using System;
using System.Linq;

namespace Hx.Workflow.Application.StepBodys
{
    internal static class ActivitySubmissionValidator
    {
        public static string? Validate(
            WkPointerEventData input,
            string currentStepName,
            WkDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(input.Step) &&
                !string.Equals(input.Step.Trim(), currentStepName, StringComparison.Ordinal))
            {
                return $"请求节点“{input.Step}”与当前待办节点“{currentStepName}”不一致，请刷新待办后重试。";
            }

            if (!input.ExecutionType.HasValue ||
                !Enum.IsDefined(typeof(StepExecutionType), input.ExecutionType.Value))
            {
                return "ExecutionType 不是有效的流程操作类型。";
            }

            var currentNode = definition.Nodes.FirstOrDefault(node =>
                string.Equals(node.Name, currentStepName, StringComparison.Ordinal));
            if (currentNode == null)
            {
                return $"流程定义中不存在当前节点“{currentStepName}”。";
            }

            if (input.ExecutionType == StepExecutionType.Forward &&
                currentNode.StepNodeType != StepNodeType.End &&
                !BranchDecisionValidator.CanTransition(currentNode, input.DecideBranching))
            {
                return $"分支值“{input.DecideBranching}”无法从当前节点“{currentStepName}”到达下一节点。";
            }

            return null;
        }
    }
}
