using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using System;
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
    }
}
