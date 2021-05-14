using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Uow;
using WorkflowCore.Models;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkInstanceRepository
        : EfCoreRepository<WkDbContext, WkInstance, Guid>,
        IWkInstanceRepository
    {
        public WkInstanceRepository(
            IDbContextProvider<WkDbContext> options)
            : base(options)
        {
        }
        public virtual async Task<List<WkInstance>> GetInstancesAsync(string difinitionId, int version)
        {
            return await (await GetDbSetAsync())
                .Where(d =>
                d.WkDifinitionId == new Guid(difinitionId)
                && d.Version == version).ToListAsync();
        }
        public virtual async Task<List<Guid>> GetRunnableInstancesAsync(DateTime nextExecute)
        {
            return await (from p in await GetDbSetAsync()
                          where p.NextExecution.HasValue
                          && p.NextExecution <= nextExecute.Ticks
                          && p.Status == WorkflowStatus.Runnable
                          select p.Id).ToListAsync();
        }
        public override async Task<WkInstance> FindAsync(
            Guid id, bool includeDetails = true, CancellationToken cancellation = default)
        {
            return (await GetDbSetAsync())
                    .IncludeDetials(includeDetails)
                    .FirstOrDefault(d => d.Id == id);
        }
        public virtual async Task<WkExecutionPointer> GetPointerAsync(Guid pointerId)
        {
            var entitys = (await GetDbSetAsync()).IncludeDetials(true);
            var pointers = await (from p in entitys
                                  where p.ExecutionPointers.Any(d => d.Id == pointerId)
                                  select p.ExecutionPointers).FirstOrDefaultAsync();
            return pointers?.FirstOrDefault(d => d.Id == pointerId);
        }
        public virtual async Task<IQueryable<WkInstance>> GetDetails(bool tracking = false)
        {
            return (await GetDbSetAsync())
                .IncludeDetials(true)
                .AsTracking(tracking ?
                QueryTrackingBehavior.TrackAll :
                QueryTrackingBehavior.NoTracking);
        }
        public virtual async Task<List<WkInstance>> GetDetails(List<Guid> ids)
        {
            return await (from p in await GetDbSetAsync()
                          where ids.Contains(p.Id)
                          select p).Include(x => x.ExecutionPointers)
                   .ThenInclude(x => x.ExtensionAttributes)
                   .ToListAsync();
        }
        public virtual async Task<List<WkInstance>> GetMyInstancesAsync(
            ICollection<Guid> ids,
            WorkflowStatus? status,
            int skipCount,
            int maxResultCount)
        {
            var queryable = (await GetDbSetAsync()).AsQueryable();
            if (ids?.Count > 0)
                queryable.Where(d => ids.Any(p => p == d.Id));
            if (status != null)
                queryable = queryable.Where(d => d.Status == status);
            return await queryable.Skip(skipCount).Take(maxResultCount).ToListAsync();
        }
    }
}