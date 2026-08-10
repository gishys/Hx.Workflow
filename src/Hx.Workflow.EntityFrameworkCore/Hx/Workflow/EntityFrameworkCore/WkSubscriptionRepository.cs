using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkSubscriptionRepository
        : EfCoreRepository<WkDbContext, WkSubscription, Guid>,
        IWkSubscriptionRepository
    {
        public WkSubscriptionRepository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        /// <summary>
        /// Get entity by event name,event key,event time
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventKey"></param>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        public virtual async Task<List<WkSubscription>> GetSubscriptionAsync(
            string eventName, string eventKey, DateTime eventTime)
        {
            return await (await GetDbSetAsync())
                .Where(WkSubscriptionQueries.OpenForEvent(eventName, eventKey, eventTime))
                .OrderBy(d => d.SubscribeAsOf)
                .ToListAsync();
        }
        public virtual async Task<bool> TrySetTokenAsync(
            Guid id,
            string token,
            string workerId,
            DateTime expiry,
            DateTime asOf,
            CancellationToken cancellationToken = default)
        {
            var affectedRows = await (await GetDbSetAsync())
                .Where(d => d.Id == id &&
                    (d.ExternalToken == null ||
                     (d.ExternalTokenExpiry.HasValue && d.ExternalTokenExpiry <= asOf)))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(d => d.ExternalToken, token)
                    .SetProperty(d => d.ExternalWorkerId, workerId)
                    .SetProperty(d => d.ExternalTokenExpiry, expiry), cancellationToken);

            return affectedRows == 1;
        }
        public virtual async Task<bool> AnyAsync(Guid id)
        {
            return await (await GetDbSetAsync()).AnyAsync(d => d.WorkflowId == id);
        }
        public virtual async Task<List<WkSubscription>> GetSubscriptionsByExecutionPointerAsync(Guid exeId)
        {
            return await (await GetDbSetAsync()).Where(d => d.ExecutionPointerId == exeId).ToListAsync();
        }
    }
}
