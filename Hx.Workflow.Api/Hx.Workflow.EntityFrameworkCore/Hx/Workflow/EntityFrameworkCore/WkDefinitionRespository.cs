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
    public class WkDefinitionRespository
        : EfCoreRepository<WkDbContext, WkDefinition, Guid>,
        IWkDefinitionRespository
    {
        public WkDefinitionRespository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        /// <summary>
        /// Get entity by id an version
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinition> GetDefinitionAsync(Guid id, int version)
        {
            return (await GetDbSetAsync())
                .FirstOrDefault(x => x.Id == id && x.Version == version);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeDetails"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<List<WkDefinition>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .IncludeDetials(includeDetails)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }
        public virtual async Task<WkDefinition> GetDefinitionAsync(string name, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .IncludeDetials(includeDetails)
                .FirstOrDefaultAsync(d => d.Title == name, GetCancellationToken(cancellationToken));
        }
    }
}
