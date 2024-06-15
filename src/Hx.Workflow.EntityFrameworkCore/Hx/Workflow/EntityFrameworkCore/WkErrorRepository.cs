using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkErrorRepository
        : EfCoreRepository<WkDbContext, WkExecutionError, Guid>,
        IWkErrorRepository
    {
        public WkErrorRepository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        public virtual async Task<List<WkExecutionError>> GetListByIdAsync(Guid? workflowId, Guid? pointerId)
        {
            return await (await GetDbSetAsync())
                .WhereIf(workflowId != null, d => d.WkInstanceId == workflowId)
                .WhereIf(pointerId != null, d => d.WkExecutionPointerId == pointerId)
                .ToListAsync();
        }
    }
}
