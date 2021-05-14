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
        public virtual async Task<List<WkSubscription>> GetSubcriptionAsync(
            string eventName, string eventKey, DateTime eventTime)
        {
            return await (await GetDbSetAsync()).Where(
                d => d.EventName == eventName &&
                d.EventKey == eventKey &&
                d.SubscribeAsOf <= eventTime && d.ExternalToken == null)
                .ToListAsync();
        }
    }
}
