using Hx.Workflow.Domain;
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
    public class WkStepBodyRespository
        : EfCoreRepository<WkDbContext, WkStepBody, Guid>,
        IWkStepBodyRespository
    {
        public WkStepBodyRespository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        public async virtual Task<WkStepBody?> GetStepBodyAsync(string name)
        {
            return await (await GetDbSetAsync())
                .Include(d => d.Inputs)
                .FirstOrDefaultAsync(d => d.Name == name);
        }
        public override async Task<WkStepBody?> FindAsync(
            Guid id,
            bool includeDetails = true,
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .Include(d => d.Inputs)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        public virtual async Task<List<WkStepBody>> GetPagedAsync(string filter, int skipCount, int maxResultCount)
        {
            return await (await GetDbSetAsync())
                .WhereIf(!string.IsNullOrEmpty(filter), d => d.Name.Contains(filter))
                .PageBy(skipCount, maxResultCount)
                .ToListAsync();
        }
        public virtual async Task<int> GetPagedCountAsync(string filter)
        {
            return await (await GetDbSetAsync())
                .WhereIf(!string.IsNullOrEmpty(filter), d => d.Name.Contains(filter))
                .CountAsync();
        }
        public async Task<bool> AnyAsync(string typeFullName,CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .AnyAsync(d => d.TypeFullName == typeFullName);
        }
    }
}
