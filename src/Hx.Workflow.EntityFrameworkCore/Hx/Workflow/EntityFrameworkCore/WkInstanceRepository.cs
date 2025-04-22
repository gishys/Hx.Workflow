using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.Stats;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
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
        public override async Task<WkInstance?> FindAsync(
            Guid id, bool includeDetails = true, CancellationToken cancellation = default)
        {
            return await (await GetDbSetAsync())
                    .IncludeDetails(includeDetails)
                    .FirstOrDefaultAsync(d => d.Id == id, cancellation);
        }
        public virtual async Task<WkExecutionPointer> GetPointerAsync(Guid pointerId)
        {
            var entitys = (await GetDbSetAsync()).IncludeDetails(true);
            var pointers = await (from p in entitys
                                  where p.ExecutionPointers.Any(d => d.Id == pointerId)
                                  select p.ExecutionPointers).FirstOrDefaultAsync();
            return pointers?.FirstOrDefault(d => d.Id == pointerId);
        }
        public virtual async Task<IQueryable<WkInstance>> GetDetails(bool tracking = false)
        {
            return (await GetDbSetAsync())
                .IncludeDetails(true)
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
        public virtual async Task<WkInstance> GetByReferenceAsync(string reference)
        {
            return await (await GetDbSetAsync())
                .IncludeDetails(true)
                .FirstOrDefaultAsync(d => d.Reference == reference);
        }
        public virtual async Task<List<ProcessingStatusStat>> GetProcessingStatusStatListAsync(Guid transactorId)
        {
            var result = new List<ProcessingStatusStat>();
            var dbSet = await GetDbSetAsync();

            var beingProcessed = await dbSet.Where(d => d.ExecutionPointers.Any(a =>
                a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => transactorId == c.CandidateId))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "BeingProcessed", Count = beingProcessed });

            var waitingReceipt = await dbSet.Where(d =>
            d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c =>
            c.CandidateId == transactorId && c.ParentState == ExeCandidateState.WaitingReceipt))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "WaitingReceipt", Count = waitingReceipt });

            var pending = await dbSet.Where(d =>
            d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c =>
            c.CandidateId == transactorId && c.ParentState == ExeCandidateState.Pending))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "Pending", Count = pending });
            //所有我办理过的业务(不包含在办)
            var participation = await dbSet.Where(d =>
            d.ExecutionPointers.Any(a => a.Status == PointerStatus.Complete && a.WkCandidates.Any(c => c.CandidateId == transactorId))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "Participation", Count = participation });

            var entrusted = await dbSet.Where(d => d.ExecutionPointers.Any(a =>
                a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => transactorId == c.CandidateId &&
                c.ExeOperateType == ExePersonnelOperateType.Entrusted))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "Entrusted", Count = entrusted });

            var handled = await dbSet.Where(d =>
            d.ExecutionPointers.Any(a => a.StepId == 0 && a.WkCandidates.Any(c => c.CandidateId == transactorId))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "Handled", Count = handled });

            var follow = await dbSet.Where(d =>
            d.ExecutionPointers.Any(a => a.WkCandidates.Any(c => c.CandidateId == transactorId && c.Follow == true))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "Follow", Count = follow });

            var suspended = await dbSet.Where(d => d.Status == WorkflowStatus.Suspended &&
            d.ExecutionPointers.Any(a => a.WkCandidates.Any(c => c.CandidateId == transactorId))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "Suspended", Count = suspended });

            var countersign = await dbSet.Where(d => d.ExecutionPointers.Any(a =>
            a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => transactorId == c.CandidateId &&
            c.ExeOperateType == ExePersonnelOperateType.Countersign))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "Countersign", Count = countersign });

            var carbonCopy = await dbSet.Where(d => d.ExecutionPointers.Any(a =>
            a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => transactorId == c.CandidateId &&
            c.ExeOperateType == ExePersonnelOperateType.CarbonCopy))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "CarbonCopy", Count = carbonCopy });

            var abnormal = await dbSet.Where(d => d.ExecutionPointers.Any(a =>
            a.Status == PointerStatus.Failed && a.WkCandidates.Any(c => transactorId == c.CandidateId))).CountAsync();
            result.Add(new ProcessingStatusStat() { TransactorId = transactorId, Status = "Abnormal", Count = abnormal });
            return result;
        }
        public virtual async Task<List<ProcessTypeStat>> GetBusinessTypeListAsync()
        {
            var dbSet = await GetDbSetAsync();
            var result = await dbSet.GroupBy(d => d.WkDefinition.BusinessType).Select(d => new ProcessTypeStat() { Count = d.Count(), PClassification = d.Key }).ToListAsync();
            return result;
        }
        public virtual async Task<List<ProcessTypeStat>> GetProcessTypeStatListAsync()
        {
            var dbSet = await GetDbSetAsync();
            var result = await dbSet.GroupBy(d => new
            {
                d.WkDefinition.ProcessType,
                Mouth = d.CreateTime.Month.ToString().PadLeft(2, '0'),
            }).Select(d => new ProcessTypeStat()
            {
                Count = d.Count(),
                PClassification = d.Key.ProcessType,
                SClassification = d.Key.Mouth
            }).ToListAsync();
            return result;
        }
        public virtual async Task<List<WkInstance>> GetMyInstancesAsync(
            ICollection<Guid> ids,
            string reference,
            MyWorkState? state,
            int skipCount,
            int maxResultCount)
        {
            var queryable = (await GetDbSetAsync())
                .Include(x => x.ExecutionPointers)
                .ThenInclude(x => x.WkCandidates)
                .Include(x => x.ExecutionPointers)
                .ThenInclude(x => x.WkSubscriptions)
                .Include(x => x.WkDefinition)
                .ThenInclude(x => x.Nodes)
                .Include(x => x.WkAuditors)
                .WhereIf(state == MyWorkState.BeingProcessed, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))))
                .WhereIf(state == MyWorkState.WaitingReceipt, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ParentState == ExeCandidateState.WaitingReceipt)))
                .WhereIf(state == MyWorkState.Pending, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ParentState == ExeCandidateState.Pending)))
                .WhereIf(state == MyWorkState.Participation, d =>
                d.ExecutionPointers.Any(a => a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))))
                .WhereIf(state == MyWorkState.Entrusted, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ExeOperateType == ExePersonnelOperateType.Entrusted)))
                .WhereIf(state == MyWorkState.Handled, d =>
                d.ExecutionPointers.Any(a => a.StepId == 0 && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))))
                .WhereIf(state == MyWorkState.Follow, d =>
                d.ExecutionPointers.Any(a => a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.Follow == true)))
                .WhereIf(state == MyWorkState.Suspended, d =>
                d.ExecutionPointers.Any(a => a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))) && d.Status == WorkflowStatus.Suspended)
                .WhereIf(state == MyWorkState.Countersign, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ExeOperateType == ExePersonnelOperateType.Countersign)))
                .WhereIf(state == MyWorkState.CarbonCopy, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ExeOperateType == ExePersonnelOperateType.CarbonCopy)))
                .WhereIf(state == MyWorkState.Abnormal, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.Failed && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))))
                .WhereIf(reference != null, d => d.Reference.Contains(reference))
                .OrderByDescending(d => d.CreateTime);
            return await queryable.PageBy(skipCount, maxResultCount).ToListAsync();
        }
        public virtual async Task<int> GetMyInstancesCountAsync(
            ICollection<Guid> ids,
            string reference,
            MyWorkState? state)
        {
            var queryable = (await GetDbSetAsync())
                .WhereIf(state == MyWorkState.BeingProcessed, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))))
                .WhereIf(state == MyWorkState.WaitingReceipt, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ParentState == ExeCandidateState.WaitingReceipt)))
                .WhereIf(state == MyWorkState.Pending, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ParentState == ExeCandidateState.Pending)))
                .WhereIf(state == MyWorkState.Participation, d =>
                d.ExecutionPointers.Any(a => a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))))
                .WhereIf(state == MyWorkState.Entrusted, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ExeOperateType == ExePersonnelOperateType.Entrusted)))
                .WhereIf(state == MyWorkState.Handled, d =>
                d.ExecutionPointers.Any(a => a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))) && (d.Status == WorkflowStatus.Runnable || d.Status == WorkflowStatus.Suspended))
                .WhereIf(state == MyWorkState.Follow, d =>
                d.ExecutionPointers.Any(a => a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.Follow == true)))
                .WhereIf(state == MyWorkState.Suspended, d =>
                d.ExecutionPointers.Any(a => a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))) && d.Status == WorkflowStatus.Suspended)
                .WhereIf(state == MyWorkState.Countersign, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ExeOperateType == ExePersonnelOperateType.Countersign)))
                .WhereIf(state == MyWorkState.CarbonCopy, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId) && c.ExeOperateType == ExePersonnelOperateType.CarbonCopy)))
                .WhereIf(state == MyWorkState.Abnormal, d =>
                d.ExecutionPointers.Any(a => a.Status == PointerStatus.Failed && a.WkCandidates.Any(c => ids.Any(id => id == c.CandidateId))))
                .WhereIf(reference != null, d => d.Reference.Contains(reference));
            return await queryable.CountAsync();
        }
        public virtual async Task<ICollection<ExePointerCandidate>> GetCandidatesAsync(Guid wkInstanceId)
        {
            var queryable = (await GetDbSetAsync())
                .Include(d => d.ExecutionPointers)
                .ThenInclude(d => d.WkCandidates)
                .Select(d => new
                {
                    d.Id,
                    d.Status,
                    ExecutionPointers = d.ExecutionPointers.Select(e => new
                    {
                        e.Active,
                        e.Status,
                        e.WkCandidates
                    })
                });
            var instance = await queryable.FirstOrDefaultAsync(d =>
            d.Id == wkInstanceId
            && d.Status == WorkflowStatus.Runnable);
            if (instance != null)
            {
                var currentPointer = instance.ExecutionPointers.First(d => d.Status != PointerStatus.Complete);
                return currentPointer.WkCandidates;
            }
            return new Collection<ExePointerCandidate>();
        }
        public virtual async Task<WkInstance> UpdateCandidateAsync(
            Guid wkinstanceId, Guid executionPointerId, ICollection<ExePointerCandidate> wkCandidates, ExePersonnelOperateType type)
        {
            var dbSet = await GetDbSetAsync();
            var updateEntity = await dbSet
                .Include(d => d.ExecutionPointers)
                .ThenInclude(d => d.WkCandidates)
                .FirstOrDefaultAsync(d => d.Id == wkinstanceId);
            if (updateEntity != null)
            {
                var executionPointer = updateEntity.ExecutionPointers.FirstOrDefault(d => d.Id == executionPointerId);
                if (executionPointer != null)
                {
                    if (
                        type == ExePersonnelOperateType.CarbonCopy ||
                        type == ExePersonnelOperateType.Countersign ||
                        type == ExePersonnelOperateType.Notify)
                        await executionPointer.AddCandidates(wkCandidates);
                    else
                    {
                        executionPointer.WkCandidates.RemoveAll(_ => true);
                        await executionPointer.AddCandidates(wkCandidates);
                    }
                }
            }
            return await UpdateAsync(updateEntity);
        }
        /// <summary>
        /// 修改候选人办理状态
        /// </summary>
        /// <param name="wkinstanceId"></param>
        /// <param name="executionPointerId"></param>
        /// <param name="parentState"></param>
        /// <returns></returns>
        public virtual async Task UpdateCandidateAsync(Guid wkinstanceId, Guid executionPointerId, ExeCandidateState parentState)
        {
            var dbSet = await GetDbSetAsync();
            var updateEntity = await dbSet
                .Include(d => d.ExecutionPointers)
                .ThenInclude(d => d.WkCandidates)
                .FirstOrDefaultAsync(d => d.Id == wkinstanceId);
            if (updateEntity != null)
            {
                var executionPointer = updateEntity.ExecutionPointers.FirstOrDefault(d => d.Id == executionPointerId);
                foreach (var item in executionPointer.WkCandidates)
                {
                    item.SetParentState(parentState);
                }
                await UpdateAsync(updateEntity);
            }
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
                .OrderByDescending(d => int.Parse(IntRegex().Match(d.Reference.Right(5)).Value))
                .Select(d => int.Parse(IntRegex().Match(d.Reference.Right(5)).Value))
                .FirstOrDefault();
            return maxNumber;
        }

        [GeneratedRegex(@"\d+")]
        private static partial Regex IntRegex();
        public async Task<WkInstance> RecipientExePointerAsync(Guid workflowId, Guid currentUserId)
        {
            var instance = await FindAsync(workflowId);
            var exePointer = instance.ExecutionPointers.First(d => d.Status != PointerStatus.Complete);
            if (!exePointer.WkCandidates.Any(d => d.CandidateId == currentUserId))
            {
                throw new UserFriendlyException("没有权限接收实例！");
            }
            exePointer.WkCandidates.RemoveAll(d => d.CandidateId != currentUserId);
            var candidate = exePointer.WkCandidates.First(d => d.CandidateId == currentUserId);
            candidate.SetParentState(ExeCandidateState.Pending);
            await exePointer.SetRecipientInfo(candidate.UserName, currentUserId);
            return await UpdateAsync(instance);
        }
        public async Task<WkInstance> RecipientExePointerAsync(Guid workflowId, Guid currentUserId, string recepient, Guid recepientId)
        {
            var instance = await FindAsync(workflowId);
            var exePointer = instance.ExecutionPointers.First(d => d.Status != PointerStatus.Complete);
            await exePointer.SetRecipientInfo(recepient, recepientId);
            return await UpdateAsync(instance);
        }
        /// <summary>
        /// 流程实例添加业务数据
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task UpdateDataAsync(Guid workflowId, IDictionary<string, object> data)
        {
            var instance = await FindAsync(workflowId);
            var instanceData = JsonSerializer.Deserialize<IDictionary<string, object>>(instance.Data);
            await instance.SetData(JsonSerializer.Serialize(instanceData.ConcatenateAndReplace(data)));
            await UpdateAsync(instance);
        }
    }
}