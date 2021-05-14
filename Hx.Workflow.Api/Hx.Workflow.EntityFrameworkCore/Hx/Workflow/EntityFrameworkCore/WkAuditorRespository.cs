using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.StepBodys;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkAuditorRespository
        : EfCoreRepository<WkDbContext, WkAuditor, Guid>,
        IWkAuditorRespository
    {
        public WkAuditorRespository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        public virtual async Task<bool> IsVerifyAsync(
            Guid executionPointerId,
            Guid userId,
            EnumAuditStatus status)
        {
            var queryable = await GetDbSetAsync();
            return queryable.Any(d =>
            d.ExecutionPointerId == executionPointerId
            && d.UserId == userId
            && d.Status == status);
        }
    }
}
