using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using WorkflowCore.Models;

namespace Hx.Workflow.EntityFrameworkCore
{
    public partial class WkInstanceRepository
        : EfCoreRepository<WkDbContext, WkInstance, Guid>,
        IWkInstanceRepository
    {
        public WkInstanceRepository(
            IDbContextProvider<WkDbContext> options)
            : base(options)
        {
        }
        public virtual async Task<List<WkInstance>> GetInstancesAsync(string difinitionId, int version)
        {
            return await (await GetDbSetAsync())
                .Where(d =>
                d.WkDifinitionId == new Guid(difinitionId)
                && d.Version == version).ToListAsync();
        }
        public virtual async Task<List<Guid>> GetRunnableInstancesAsync(DateTime nextExecute)
        {
            return await (from p in await GetDbSetAsync()
                          where p.NextExecution.HasValue
                          && p.NextExecution <= nextExecute.Ticks
                          && p.Status == WorkflowStatus.Runnable
                          select p.Id).ToListAsync();
        }
        public override async Task<WkInstance> FindAsync(
            Guid id, bool includeDetails = true, CancellationToken cancellation = default)
        {
            return (await GetDbSetAsync())
                    .IncludeDetials(includeDetails)
                    .FirstOrDefault(d => d.Id == id);
        }
        public virtual async Task<WkExecutionPointer> GetPointerAsync(Guid pointerId)
        {
            var entitys = (await GetDbSetAsync()).IncludeDetials(true);
            var pointers = await (from p in entitys
                                  where p.ExecutionPointers.Any(d => d.Id == pointerId)
                                  select p.ExecutionPointers).FirstOrDefaultAsync();
            return pointers?.FirstOrDefault(d => d.Id == pointerId);
        }
        public virtual async Task<IQueryable<WkInstance>> GetDetails(bool tracking = false)
        {
            return (await GetDbSetAsync())
                .IncludeDetials(true)
                .AsTracking(tracking ?
                QueryTrackingBehavior.TrackAll :
                QueryTrackingBehavior.NoTracking);
        }
        public virtual async Task<List<WkInstance>> GetDetails(List<Guid> ids)
        {
            return await (from p in await GetDbSetAsync()
                          where ids.Contains(p.Id)
                          select p).Include(x => x.ExecutionPointers)
                   .ThenInclude(x => x.ExtensionAttributes)
                   .ToListAsync();
        }
        public virtual async Task<List<WkInstance>> GetMyInstancesAsync(
            ICollection<Guid> ids,
            WorkflowStatus? status,
            int skipCount,
            int maxResultCount)
        {
            var queryable = (await GetDbSetAsync())
                .IncludeDetials(true)
                .WhereIf(status != null, d => d.Status == status)
                .Where(d => d.WkAuditors.Any(a => a.Status == EnumAuditStatus.UnAudited && ids.Any(id => id == a.UserId)));
            return await queryable.PageBy(skipCount, maxResultCount).ToListAsync();
        }
        public virtual async Task<int> GetMyInstancesCountAsync(
            ICollection<Guid> ids,
            WorkflowStatus? status)
        {
            var queryable = (await GetDbSetAsync())
                .WhereIf(status != null, d => d.Status == status)
                .Where(d => d.WkAuditors.Any(a => ids.Any(id => id == a.UserId)));
            return await queryable.CountAsync();
        }
        public virtual async Task<ICollection<ExePointerCandidate>> GetCandidatesAsync(Guid wkInstanceId)
        {
            var queryable = (await GetDbSetAsync()).IncludeDetials(true);
            return (await queryable.FirstOrDefaultAsync(d => d.Id == wkInstanceId))
                ?.WkDefinition.WkCandidates.ToList() as ICollection<ExePointerCandidate>;
        }
        public virtual async Task<WkInstance> UpdateCandidateAsync(
            Guid wkinstanceId, Guid executionPointerId, ICollection<ExePointerCandidate> wkCandidates)
        {
            var dbSet = await GetDbSetAsync();
            var updateEntity = await dbSet.IncludeDetials(true).FirstOrDefaultAsync(d => d.Id == wkinstanceId);
            if (updateEntity != null)
            {
                var executionPointer = updateEntity.ExecutionPointers.FirstOrDefault(d => d.Id == executionPointerId);
                if (executionPointer != null)
                    await executionPointer.AddCandidates(wkCandidates.ToCandidates(executionPointer.WkCandidates));
            }
            return await UpdateAsync(updateEntity);
        }
        /// <summary>
        /// 获取当天编号最大值
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetMaxNumberAsync()
        {
            var dbSet = await GetDbSetAsync();
            int maxNumber = dbSet
                .AsEnumerable()
                .Where(d => d.CreateTime.ToString("d") == DateTime.Now.ToString("d"))
                .OrderByDescending(d => int.Parse(IntRegex().Match(d.BusinessNumber).Value))
                .Select(d => int.Parse(IntRegex().Match(d.BusinessNumber).Value))
                .FirstOrDefault();
            return maxNumber;
        }

        [GeneratedRegex(@"\d+")]
        private static partial Regex IntRegex();
        public async Task<WkInstance> RecipientExePointerAsync(Guid workflowId, Guid currentUserId)
        {
            var instance = await FindAsync(workflowId);
            var exePointer = instance.ExecutionPointers.First(d => d.Status == PointerStatus.Running);
            if (!exePointer.WkCandidates.Any(d => d.CandidateId == currentUserId))
            {
                throw new UserFriendlyException("没有权限接收实例！");
            }
            exePointer.WkCandidates.RemoveAll(d => d.CandidateId != currentUserId);
            return await UpdateAsync(instance);
        }
    }
}