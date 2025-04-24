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
    public class WkStepBodyRespository(IDbContextProvider<WkDbContext> options)
                : EfCoreRepository<WkDbContext, WkStepBody, Guid>(options),
        IWkStepBodyRespository
    {
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
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }
        public virtual async Task<List<WkStepBody>> GetPagedAsync(string? filter, int skipCount, int maxResultCount)
        {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
            return await (await GetDbSetAsync())
                .WhereIf(!string.IsNullOrEmpty(filter), d => d.Name.Contains(filter))
                .OrderBy(d => d.CreationTime)
                .PageBy(skipCount, maxResultCount)
                .ToListAsync();
#pragma warning restore CS8604 // 引用类型参数可能为 null。
        }
        public virtual async Task<int> GetPagedCountAsync(string? filter)
        {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
            return await (await GetDbSetAsync())
                .WhereIf(!string.IsNullOrEmpty(filter), d => d.Name.Contains(filter))
                .CountAsync();
#pragma warning restore CS8604 // 引用类型参数可能为 null。
        }
        public async Task<bool> AnyAsync(string typeFullName, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .AnyAsync(d => d.TypeFullName == typeFullName, cancellationToken);
        }
    }
}
