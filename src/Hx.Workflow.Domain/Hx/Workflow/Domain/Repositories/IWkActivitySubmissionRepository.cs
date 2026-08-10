using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkActivitySubmissionRepository : IBasicRepository<WkActivitySubmission, Guid>
    {
        Task<WkActivitySubmission?> FindByKeyAsync(
            Guid workflowId,
            string activityName,
            CancellationToken cancellationToken = default);

        Task<(WkActivitySubmission Submission, bool Created)> GetOrCreateAsync(
            WkActivitySubmission submission,
            CancellationToken cancellationToken = default);

        Task<List<WkActivitySubmission>> GetProcessableAsync(
            DateTime asOf,
            int maxCount,
            CancellationToken cancellationToken = default);

        Task<bool> TryClaimAsync(
            Guid id,
            DateTime asOf,
            DateTime lockedUntil,
            CancellationToken cancellationToken = default);

        Task MarkEventPublishedAsync(Guid id, DateTime asOf, CancellationToken cancellationToken = default);
        Task MarkSucceededAsync(Guid id, DateTime asOf, CancellationToken cancellationToken = default);
        Task MarkFailedAsync(Guid id, string error, DateTime asOf, CancellationToken cancellationToken = default);
        Task MarkRejectedAsync(Guid id, string error, DateTime asOf, CancellationToken cancellationToken = default);
        Task ReleaseForRetryAsync(Guid id, string error, DateTime asOf, CancellationToken cancellationToken = default);
    }
}
