using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkEventRepository : IBasicRepository<WkEvent, Guid>
    {
        /// <summary>
        /// get entity by event event name,event key,event time
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventKey"></param>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        Task<List<WkEvent>> GetEventsAsync(
            string eventName, string eventKey, DateTime eventTime);
        /// <summary>
        /// Get runnable events by event time
        /// </summary>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        Task<List<Guid>> GetRunnableEventsAsync(DateTime eventTime);
    }
}
