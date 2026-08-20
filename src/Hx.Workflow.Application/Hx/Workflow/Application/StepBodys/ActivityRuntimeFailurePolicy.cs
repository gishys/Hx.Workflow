using Hx.Workflow.Domain;
using Hx.Workflow.Domain.BusinessModule;
using Hx.Workflow.Domain.Shared;
using System;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
    /// <summary>
    /// Handles deterministic validation failures discovered while replaying a persisted activity event.
    /// Such failures cannot be fixed by retrying the same event data, so the workflow is suspended for
    /// explicit operator recovery instead of remaining in WorkflowCore's unbounded retry loop.
    /// </summary>
    internal static class ActivityRuntimeFailurePolicy
    {
        public static string? ValidateAndSuspendIfInvalid(
            WorkflowInstance workflow,
            WkPointerEventData eventData,
            WkNode step)
        {
            ArgumentNullException.ThrowIfNull(workflow);
            ArgumentNullException.ThrowIfNull(eventData);
            ArgumentNullException.ThrowIfNull(step);

            var error = GetDeterministicValidationError(eventData, step);
            if (error != null)
            {
                workflow.Status = WorkflowStatus.Suspended;
            }

            return error;
        }

        private static string? GetDeterministicValidationError(
            WkPointerEventData eventData,
            WkNode step)
        {
            if (!eventData.ExecutionType.HasValue ||
                !Enum.IsDefined(typeof(StepExecutionType), eventData.ExecutionType.Value))
            {
                return "事件数据中的ExecutionType无效！";
            }

            if (eventData.ExecutionType == StepExecutionType.Forward &&
                step.StepNodeType != StepNodeType.End &&
                !BranchDecisionValidator.CanTransition(step, eventData.DecideBranching))
            {
                return $"分支值“{eventData.DecideBranching}”无法从节点“{step.Name}”到达下一节点。" +
                    $"{BranchDecisionValidator.DescribeAllowedForwardDecisions(step)}。";
            }

            return null;
        }
    }
}
