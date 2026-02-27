using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Stats;
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
    public class WkErrorRepository(IDbContextProvider<WkDbContext> options)
        : EfCoreRepository<WkDbContext, WkExecutionError, Guid>(options),
        IWkErrorRepository
    {
        public virtual async Task<List<WkExecutionError>> GetListByIdAsync(Guid? workflowId, Guid? pointerId)
        {
            return await (await GetDbSetAsync())
                .WhereIf(workflowId != null, d => d.WkInstanceId == workflowId)
                .WhereIf(pointerId != null, d => d.WkExecutionPointerId == pointerId)
                .ToListAsync();
        }

        public virtual async Task<ErrorSummaryStat> GetErrorSummaryAsync(DateTime? startTime, DateTime? endTime, Guid? definitionId, Guid? tenantId)
        {
            var dbContext = await GetDbContextAsync();
            var errors = dbContext.WkExecutionErrors.AsQueryable();
            var instances = dbContext.WkInstances.AsQueryable();
            var joined = from e in errors
                         join i in instances on e.WkInstanceId equals i.Id
                         where (!tenantId.HasValue || i.TenantId == tenantId.Value)
                               && (!definitionId.HasValue || i.WkDifinitionId == definitionId.Value)
                               && (!startTime.HasValue || e.ErrorTime >= startTime.Value)
                               && (!endTime.HasValue || e.ErrorTime <= endTime.Value)
                         select e;
            var totalErrors = await joined.CountAsync();
            var affectedInstances = await joined.Select(x => x.WkInstanceId).Distinct().CountAsync();
            return new ErrorSummaryStat { TotalErrorCount = totalErrors, AffectedInstanceCount = affectedInstances };
        }

        public virtual async Task<List<ErrorStat>> GetErrorStatByDefinitionAsync(DateTime? startTime, DateTime? endTime, Guid? definitionId, Guid? tenantId)
        {
            var dbContext = await GetDbContextAsync();
            var errors = dbContext.WkExecutionErrors.AsQueryable();
            var instances = dbContext.WkInstances.Include(i => i.WkDefinition).AsQueryable();
            var joined = from e in errors
                         join i in instances on e.WkInstanceId equals i.Id
                         where (!tenantId.HasValue || i.TenantId == tenantId.Value)
                               && (!definitionId.HasValue || i.WkDifinitionId == definitionId.Value)
                               && (!startTime.HasValue || e.ErrorTime >= startTime.Value)
                               && (!endTime.HasValue || e.ErrorTime <= endTime.Value)
                         select new { e, i };
            var list = await joined
                .GroupBy(x => new { x.i.WkDifinitionId, x.i.WkDefinition.Title })
                .Select(g => new ErrorStat
                {
                    DefinitionId = g.Key.WkDifinitionId,
                    DefinitionTitle = g.Key.Title,
                    ErrorCount = g.Count()
                })
                .ToListAsync();
            return list;
        }

        public virtual async Task<List<ErrorByStepStat>> GetErrorByStepStatListAsync(DateTime? startTime, DateTime? endTime, Guid? definitionId, Guid? tenantId)
        {
            var dbContext = await GetDbContextAsync();
            var errors = dbContext.WkExecutionErrors.AsQueryable();
            var instances = dbContext.WkInstances.AsQueryable();
            var pointers = dbContext.WkExecutionPointers.AsQueryable();
            var joined = from e in errors
                         join i in instances on e.WkInstanceId equals i.Id
                         join p in pointers on e.WkExecutionPointerId equals p.Id
                         where (!tenantId.HasValue || i.TenantId == tenantId.Value)
                               && (!definitionId.HasValue || i.WkDifinitionId == definitionId.Value)
                               && (!startTime.HasValue || e.ErrorTime >= startTime.Value)
                               && (!endTime.HasValue || e.ErrorTime <= endTime.Value)
                         select new { p.StepId, p.StepName };
            var list = await joined
                .GroupBy(x => new { x.StepId, x.StepName })
                .Select(g => new ErrorByStepStat
                {
                    StepId = g.Key.StepId,
                    StepName = g.Key.StepName ?? "",
                    ErrorCount = g.Count()
                })
                .ToListAsync();
            return list;
        }
    }
}
