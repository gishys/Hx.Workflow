using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hx.Workflow.Application.StepBodys
{
    /// <summary>
    /// Defines how an activity decision affects the candidates on the current pointer.
    /// A Host candidate pool is competitive (one handler is enough), while Countersign
    /// candidates are additive and all active countersign handlers must finish.
    /// </summary>
    internal static class ActivityCandidateCompletionPolicy
    {
        internal static bool ApplyDecisionAndShouldWait(
            ICollection<ExePointerCandidate> candidates,
            Guid actingCandidateId,
            StepExecutionType executionType)
        {
            ArgumentNullException.ThrowIfNull(candidates);

            var actor = candidates.FirstOrDefault(candidate =>
                candidate.CandidateId == actingCandidateId)
                ?? throw new InvalidOperationException(
                    $"Candidate {actingCandidateId} does not belong to the current activity.");

            actor.SetParentState(executionType == StepExecutionType.Forward
                ? ExeCandidateState.Completed
                : ExeCandidateState.BeRolledBack);

            if (executionType != StepExecutionType.Forward)
            {
                // One rollback decision is decisive. Remove other unhandled decision
                // candidates so a completed pointer does not report them as participants.
                var unhandledApprovers = candidates
                    .Where(candidate =>
                        candidate.CandidateId != actingCandidateId &&
                        (candidate.ExeOperateType == ExePersonnelOperateType.Host ||
                         candidate.ExeOperateType == ExePersonnelOperateType.Countersign) &&
                        IsAwaitingDecision(candidate.ParentState))
                    .ToList();

                foreach (var unhandledApprover in unhandledApprovers)
                {
                    candidates.Remove(unhandledApprover);
                }

                return false;
            }

            // The normal UI receipt flow removes alternative Host candidates. External
            // activity submissions can bypass receipt, so apply the same one-of-many
            // semantics here and do not leave false participation records behind.
            if (actor.ExeOperateType == ExePersonnelOperateType.Host)
            {
                var alternativeHosts = candidates
                    .Where(candidate =>
                        candidate.CandidateId != actingCandidateId &&
                        candidate.ExeOperateType == ExePersonnelOperateType.Host &&
                        IsAwaitingDecision(candidate.ParentState))
                    .ToList();

                foreach (var alternativeHost in alternativeHosts)
                {
                    candidates.Remove(alternativeHost);
                }
            }

            var countersignPending = candidates.Any(candidate =>
                candidate.ExeOperateType == ExePersonnelOperateType.Countersign &&
                IsAwaitingDecision(candidate.ParentState));

            var hostCompleted = candidates.Any(candidate =>
                candidate.ExeOperateType == ExePersonnelOperateType.Host &&
                candidate.ParentState == ExeCandidateState.Completed);
            var hostPending = candidates.Any(candidate =>
                candidate.ExeOperateType == ExePersonnelOperateType.Host &&
                IsAwaitingDecision(candidate.ParentState));

            return countersignPending || (!hostCompleted && hostPending);
        }

        private static bool IsAwaitingDecision(ExeCandidateState state)
            => state == ExeCandidateState.Pending ||
               state == ExeCandidateState.Waiting ||
               state == ExeCandidateState.WaitingReceipt;
    }
}
