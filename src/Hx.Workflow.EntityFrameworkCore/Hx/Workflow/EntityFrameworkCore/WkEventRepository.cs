using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkEventRepository
        : EfCoreRepository<WkDbContext, WkEvent, Guid>,
        IWkEventRepository
    {
        public WkEventRepository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        /// <summary>
        /// get entity by event event name,event key,event time
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventKey"></param>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        public virtual async Task<List<WkEvent>> GetEventsAsync(
            string eventName, string eventKey, DateTime eventTime)
        {
            return await (await GetDbSetAsync()).Where(
                d => d.Name == eventName &&
                d.Key == eventKey &&
                d.Time >= eventTime).ToListAsync();
        }
        /// <summary>
        /// Get runnable events by event time
        /// </summary>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        public virtual async Task<List<Guid>> GetRunnableEventsAsync(DateTime eventTime)
        {
            return await (from x in await GetDbSetAsync()
                          where !x.IsProcessed && x.Time <= eventTime
                          select x.Id).ToListAsync();
        }
    }
}
