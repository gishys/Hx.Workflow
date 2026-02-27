using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Stats;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using WorkflowCore.Models;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkExecutionPointerRepository(IDbContextProvider<WkDbContext> options)
        : EfCoreRepository<WkDbContext, WkExecutionPointer, Guid>(options),
        IWkExecutionPointerRepository
    {
        /// <summary>
        /// 标记初始化物料
        /// </summary>
        /// <param name="wkinstanceId"></param>
        /// <param name="executionPointerId"></param>
        /// <returns></returns>
        public virtual async Task InitMaterialsAsync(Guid executionPointerId)
        {
            var dbSet = await GetDbSetAsync();
            var updateEntity = await dbSet
                .FirstOrDefaultAsync(d => d.Id == executionPointerId);
            if (updateEntity != null)
            {
                await updateEntity.InitMaterials();
                await UpdateAsync(updateEntity);
            }
        }
        public override async Task<WkExecutionPointer?> FindAsync(Guid id, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Include(d => d.WkCandidates)
                .Include(d => d.ExtensionAttributes)
                .FirstAsync(d => d.Id == id, cancellationToken: cancellationToken);
        }
        public virtual async Task<List<WkExecutionPointer>> GetListAsync(Guid wkInstanceId, CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Include(d => d.WkCandidates)
                .Include(d => d.ExtensionAttributes)
                .Where(d => d.WkInstanceId == wkInstanceId).ToListAsync(cancellationToken);
        }
        public async Task UpdateDataAsync(Guid id, Dictionary<string, string> data)
        {
            var entity = await FindAsync(id) ?? throw new UserFriendlyException(message: $"Id为：[{id}]执行点为空");
            foreach (var item in data)
            {
                entity.ExtensionAttributes.RemoveAll(d => d.AttributeKey == item.Key);
                var persistedAttr = new WkExtensionAttribute(item.Key, item.Value);
                await entity.SetExtensionAttributes(persistedAttr);
            }
            await UpdateAsync(entity);
        }
        public async Task RetryAsync(Guid id)
        {
            var entity = await FindAsync(id) ?? throw new UserFriendlyException(message: $"Id为：[{id}]执行点为空");
            await entity.SetSleepUntil(null);
            await UpdateAsync(entity);
        }

        public virtual async Task<List<StepDurationStat>> GetStepDurationStatListAsync(Guid definitionId, int version, DateTime? startTime, DateTime? endTime, Guid? tenantId)
        {
            var dbContext = await GetDbContextAsync();
            var pointers = dbContext.WkExecutionPointers.AsQueryable();
            var instances = dbContext.WkInstances.AsQueryable();
            var joined = from p in pointers
                         join i in instances on p.WkInstanceId equals i.Id
                         where i.WkDifinitionId == definitionId && i.Version == version
                               && p.Status == PointerStatus.Complete
                               && p.StartTime != null && p.EndTime != null
                               && (!tenantId.HasValue || i.TenantId == tenantId.Value)
                               && (!startTime.HasValue || p.EndTime >= startTime)
                               && (!endTime.HasValue || p.EndTime <= endTime)
                         select new { p.StepId, p.StepName, p.StartTime, p.EndTime };
            var list = await joined
                .GroupBy(x => new { x.StepId, x.StepName })
                .Select(g => new StepDurationStat
                {
                    StepId = g.Key.StepId,
                    StepName = g.Key.StepName ?? "",
                    PassCount = g.Count(),
                    AvgDurationMinutes = g.Average(x => (x.EndTime!.Value - x.StartTime!.Value).TotalMinutes)
                })
                .ToListAsync();
            return list;
        }
    }
}
