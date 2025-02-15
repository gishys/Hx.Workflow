using Hx.Workflow.Domain;
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
    public class WkApplicationFormRepository
        : EfCoreRepository<WkDbContext, ApplicationForm, Guid>,
        IWkApplicationFormRepository
    {
        public WkApplicationFormRepository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        public virtual async Task<List<ApplicationForm>> GetPagedAsync(string? filter, int skipCount, int maxResultCount)
        {
            return await (await GetDbSetAsync())
                .WhereIf(!string.IsNullOrEmpty(filter), d => d.DisplayName.Contains(filter))
                .PageBy(skipCount, maxResultCount).ToListAsync();
        }
        public virtual async Task<int> GetPagedCountAsync(string? filter)
        {
            return await (await GetDbSetAsync())
                .WhereIf(!string.IsNullOrEmpty(filter), d => d.DisplayName.Contains(filter))
                .CountAsync();
        }
    }
}
