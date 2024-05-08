using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
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
        public async virtual Task<WkStepBody> GetStepBodyAsync(string name)
        {
            return await (await GetDbSetAsync())
                .Include(d=>d.Inputs)
                .FirstOrDefaultAsync(d => d.Name == name);
        }
        public override async Task<WkStepBody> FindAsync(
            Guid id, 
            bool includeDetails = true, 
            CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .Include(d => d.Inputs)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
    }
}
