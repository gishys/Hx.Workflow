using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.StepBodys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        public virtual async Task<WkAuditor> GetAuditorAsync(Guid executionPointerId)
        {
            var queryable = await GetDbSetAsync();
            return await queryable.FirstOrDefaultAsync(d => d.ExecutionPointerId == executionPointerId);
        }
        public virtual async Task<List<Guid>> GetWkInstanceIdsAsync(ICollection<Guid> userIds)
        {
            var queryable = await GetDbSetAsync();
            return await queryable
                .Where(d => userIds.Any(p => p == d.UserId))
                .Select(d => d.WorkflowId).ToListAsync();
        }
    }
}
