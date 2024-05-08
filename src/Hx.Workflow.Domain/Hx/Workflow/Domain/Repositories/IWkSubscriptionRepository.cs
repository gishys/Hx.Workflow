using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkSubscriptionRepository : IBasicRepository<WkSubscription, Guid>
    {
        /// <summary>
        /// Get entity by event name,event key,event time
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventKey"></param>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        Task<List<WkSubscription>> GetSubcriptionAsync(
            string eventName, string eventKey, DateTime eventTime);
    }
}
