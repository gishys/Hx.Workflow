using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using System;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkExecutionPointerRepository
        : EfCoreRepository<WkDbContext, WkExecutionPointer, Guid>,
        IWkExecutionPointerRepository
    {
        public WkExecutionPointerRepository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
    }
}
