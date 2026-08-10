using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Shared;
using System;
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

            return step.NextNodes
                .Where(relation => relation.NodeType == WkRoleNodeType.Forward)
                .Any(relation =>
                    (relation.Rules.Count == 0 &&
                     string.Equals(relation.NextNodeName, decision, StringComparison.Ordinal)) ||
                    relation.Rules.Any(rule =>
                        string.Equals(rule.Field, "DecideBranching", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(rule.Operator, "==", StringComparison.Ordinal) &&
                        string.Equals(rule.Value, decision, StringComparison.Ordinal)));
        }
    }
}
