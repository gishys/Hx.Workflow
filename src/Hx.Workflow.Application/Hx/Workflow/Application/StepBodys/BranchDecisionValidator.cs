using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hx.Workflow.Application.StepBodys
{
    internal static class BranchDecisionValidator
    {
        public static bool CanTransition(WkNode step, string? decision)
        {
            if (string.IsNullOrWhiteSpace(decision))
            {
                return false;
            }

            return GetAllowedForwardDecisions(step)
                .Any(value => string.Equals(value, decision, StringComparison.Ordinal));
        }

        public static IReadOnlyList<string> GetAllowedForwardDecisions(WkNode step)
        {
            return step.NextNodes
                .Where(relation => relation.NodeType == WkRoleNodeType.Forward)
                .SelectMany(relation => relation.Rules.Count == 0
                    ? [relation.NextNodeName]
                    : relation.Rules
                    .Where(rule =>
                        string.Equals(rule.Field, "DecideBranching", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(rule.Operator, "==", StringComparison.Ordinal))
                    .Select(rule => rule.Value))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        public static string DescribeAllowedForwardDecisions(WkNode step)
        {
            var allowed = GetAllowedForwardDecisions(step);
            return allowed.Count == 0
                ? "当前节点未配置可用的向前分支"
                : $"允许的向前分支：{string.Join("、", allowed)}";
        }
    }
}
