using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.Stats;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Users;
using WorkflowCore.Models;

namespace Hx.Workflow.EntityFrameworkCore
{
    public partial class WkInstanceRepository(
        IDbContextProvider<WkDbContext> options)
                : EfCoreRepository<WkDbContext, WkInstance, Guid>(options),
        IWkInstanceRepository
    {
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
        public async Task<WkInstance?> FindNoTrackAsync(
    Guid id, bool includeDetails = true, CancellationToken cancellation = default)
        {
            return await (await GetDbSetAsync())
                    .IncludeDetails(includeDetails)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == id, cancellation);
        }
        public virtual async Task<WkExecutionPointer?> GetPointerAsync(Guid pointerId)
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
        public virtual async Task<WkInstance?> GetByReferenceAsync(string reference)
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
            ICollection<Guid>? creatorIds,
            ICollection<Guid>? definitionIds,
            IDictionary<string, object>? instanceData,
            ICollection<Guid> ids,
            string? reference,
            MyWorkState? state,
            int skipCount,
            int maxResultCount)
        {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
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
                .WhereIf(!string.IsNullOrEmpty(reference), d => d.Reference.Contains(reference))
                .WhereIf(creatorIds != null, a => creatorIds.Any(c => c == a.CreatorId))
                .WhereIf(definitionIds != null, a => definitionIds.Any(d => d == a.WkDefinition.Id))
                .WhereIf(instanceData != null && instanceData.Count > 0, a => instanceData.Any(d => d.Value != null && !string.IsNullOrEmpty(d.Value.ToString()) && d.Value.ToString().Contains(a.Data)))
                .OrderByDescending(d => d.CreateTime);
#pragma warning restore CS8602 // 解引用可能出现空引用。
#pragma warning restore CS8604 // 引用类型参数可能为 null。
            return await queryable.OrderByDescending(d => d.CreationTime).PageBy(skipCount, maxResultCount).ToListAsync();
        }
        public virtual async Task<int> GetMyInstancesCountAsync(
            ICollection<Guid>? creatorIds,
            ICollection<Guid>? definitionIds,
            IDictionary<string, object>? instanceData,
            ICollection<Guid> ids,
            string? reference,
            MyWorkState? state)
        {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
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
                .WhereIf(reference != null, d => d.Reference.Contains(reference))
                .WhereIf(creatorIds != null, a => creatorIds.Any(c => c == a.CreatorId))
                .WhereIf(definitionIds != null, a => definitionIds.Any(d => d == a.WkDefinition.Id))
                .WhereIf(instanceData != null && instanceData.Count > 0, a => instanceData.Any(d => d.Value != null && !string.IsNullOrEmpty(d.Value.ToString()) && d.Value.ToString().Contains(a.Data)));
#pragma warning restore CS8602 // 解引用可能出现空引用。
#pragma warning restore CS8604 // 引用类型参数可能为 null。
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
            return [];
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
                return await UpdateAsync(updateEntity);
            }
            throw new UserFriendlyException(message: $"Id为：[{wkinstanceId}]的实例为空！");
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
                if (executionPointer != null)
                {
                    foreach (var item in executionPointer.WkCandidates)
                    {
                        item.SetParentState(parentState);
                    }
                    await UpdateAsync(updateEntity);
                }
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
        public async Task<WkInstance> RecipientExePointerAsync(Guid workflowId, ICurrentUser currentUser, bool isManager)
        {
            var currentUserId = currentUser.Id ?? throw new UserFriendlyException(message: $"未获取到当前登录用户信息！");
            var instance = await FindAsync(workflowId) ?? throw new UserFriendlyException(message: $"Id为：[{workflowId}]的实例为空！");
            var exePointer = instance.ExecutionPointers.First(d => d.Status != PointerStatus.Complete);
            if (!exePointer.WkCandidates.Any(d => d.CandidateId == currentUserId))
            {
                if (isManager && currentUser.UserName != null && currentUser.Name != null)
                {
                    var cand = exePointer.WkCandidates.First();
                    await cand.SetCandidateId(currentUserId);
                    await cand.SetUserName(currentUser.UserName);
                    await cand.SetDisplayUserName(currentUser.Name);
                }
                else
                {
                    throw new UserFriendlyException(message: "没有权限接收实例！");
                }
            }
            exePointer.WkCandidates.RemoveAll(d => d.CandidateId != currentUserId);
            var candidate = exePointer.WkCandidates.First(d => d.CandidateId == currentUserId);
            candidate.SetParentState(ExeCandidateState.Pending);
            await exePointer.SetRecipientInfo(candidate.UserName, currentUserId);
            return await UpdateAsync(instance);
        }
        public async Task<WkInstance> RecipientExePointerAsync(Guid workflowId, Guid currentUserId, string recepient, Guid recepientId)
        {
            var instance = await FindAsync(workflowId) ?? throw new UserFriendlyException(message: $"Id为：[{workflowId}]的实例为空！");
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
            var instance = await FindAsync(workflowId) ?? throw new UserFriendlyException(message: $"Id为：[{workflowId}]的实例为空！");
            var instanceData = JsonSerializer.Deserialize<IDictionary<string, object>>(instance.Data);
            if (instanceData != null)
            {
                await instance.SetData(JsonSerializer.Serialize(instanceData.ConcatenateAndReplace(data)));
                await UpdateAsync(instance);
            }
        }

        public virtual async Task<List<WkInstance>> GetMyInstancesWithVersionAsync(
            ICollection<Guid>? creatorIds,
            ICollection<Guid>? definitionIds,
            ICollection<int>? definitionVersions,
            IDictionary<string, object>? instanceData,
            ICollection<Guid> ids,
            string? reference,
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
                .ThenInclude(x => x.WkCandidates)
                .Include(x => x.WkDefinition)
                .ThenInclude(x => x.WkCandidates)
                .AsQueryable();

            // 添加版本过滤条件
            if (definitionIds != null && definitionIds.Count != 0)
            {
                if (definitionVersions != null && definitionVersions.Count != 0)
                {
                    // 同时过滤模板ID和版本号
                    queryable = queryable.Where(x => 
                        definitionIds.Contains(x.WkDifinitionId) && 
                        definitionVersions.Contains(x.Version));
                }
                else
                {
                    // 只过滤模板ID，不限制版本
                    queryable = queryable.Where(x => definitionIds.Contains(x.WkDifinitionId));
                }
            }

            // 其他过滤条件保持不变
            if (creatorIds != null && creatorIds.Count != 0)
            {
#pragma warning disable CS8629 // 可为 null 的值类型可为 null。
                queryable = queryable.Where(x => creatorIds.Contains(x.CreatorId.Value));
#pragma warning restore CS8629 // 可为 null 的值类型可为 null。
            }

            if (!string.IsNullOrEmpty(reference))
            {
                queryable = queryable.Where(x => x.Reference == reference);
            }

            if (state.HasValue)
            {
                queryable = queryable.Where(x => x.Status == (WorkflowStatus)state.Value);
            }

            if (ids != null && ids.Count != 0)
            {
                queryable = queryable.Where(x => x.ExecutionPointers.Any(a =>
                    a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Contains(c.CandidateId))));
            }

            return await queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }

        public virtual async Task<int> GetMyInstancesCountWithVersionAsync(
            ICollection<Guid>? creatorIds,
            ICollection<Guid>? definitionIds,
            ICollection<int>? definitionVersions,
            IDictionary<string, object>? instanceData,
            ICollection<Guid> ids,
            string? reference,
            MyWorkState? state)
        {
            var queryable = (await GetDbSetAsync()).AsQueryable();

            // 添加版本过滤条件
            if (definitionIds != null && definitionIds.Count != 0)
            {
                if (definitionVersions != null && definitionVersions.Count != 0)
                {
                    // 同时过滤模板ID和版本号
                    queryable = queryable.Where(x => 
                        definitionIds.Contains(x.WkDifinitionId) && 
                        definitionVersions.Contains(x.Version));
                }
                else
                {
                    // 只过滤模板ID，不限制版本
                    queryable = queryable.Where(x => definitionIds.Contains(x.WkDifinitionId));
                }
            }

            // 其他过滤条件保持不变
            if (creatorIds != null && creatorIds.Count != 0)
            {
#pragma warning disable CS8629 // 可为 null 的值类型可为 null。
                queryable = queryable.Where(x => creatorIds.Contains(x.CreatorId.Value));
#pragma warning restore CS8629 // 可为 null 的值类型可为 null。
            }

            if (!string.IsNullOrEmpty(reference))
            {
                queryable = queryable.Where(x => x.Reference == reference);
            }

            if (state.HasValue)
            {
                queryable = queryable.Where(x => x.Status == (WorkflowStatus)state.Value);
            }

            if (ids != null && ids.Count != 0)
            {
                queryable = queryable.Where(x => x.ExecutionPointers.Any(a =>
                    a.Status == PointerStatus.WaitingForEvent && a.WkCandidates.Any(c => ids.Contains(c.CandidateId))));
            }

            return await queryable.CountAsync();
        }

        public virtual async Task<List<WkInstance>> GetInstancesByDefinitionVersionAsync(Guid definitionId, int version)
        {
            return await (await GetDbSetAsync())
                .IncludeDetails(true)
                .Where(x => x.WkDifinitionId == definitionId && x.Version == version)
                .ToListAsync();
        }

        public virtual async Task<List<WkInstance>> GetRunningInstancesByVersionAsync(Guid definitionId, int version)
        {
            return await (await GetDbSetAsync())
                .IncludeDetails(true)
                .Where(x => x.WkDifinitionId == definitionId && 
                           x.Version == version && 
                           x.Status == WorkflowStatus.Runnable)
                .ToListAsync();
        }
        
        public virtual async Task<int> GetRunningInstancesCountByVersionAsync(Guid definitionId, int version)
        {
            return await (await GetDbSetAsync())
                .Where(x => x.WkDifinitionId == definitionId && 
                           x.Version == version && 
                           x.Status == WorkflowStatus.Runnable)
                .CountAsync();
        }
        
        public virtual async Task<int> GetInstancesCountByVersionAsync(Guid definitionId, int version)
        {
            return await (await GetDbSetAsync())
                .Where(x => x.WkDifinitionId == definitionId && x.Version == version)
                .CountAsync();
        }

        public virtual async Task<InstanceOverviewStat> GetInstanceOverviewStatAsync(DateTime? startTime, DateTime? endTime, Guid? tenantId)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.AsQueryable();
            if (tenantId.HasValue)
                query = query.Where(x => x.TenantId == tenantId.Value);
            if (startTime.HasValue)
                query = query.Where(x => x.CreateTime >= startTime.Value);
            if (endTime.HasValue)
                query = query.Where(x => x.CreateTime <= endTime.Value);

            var total = await query.CountAsync();
            var running = await query.CountAsync(x => x.Status == WorkflowStatus.Runnable);
            var complete = await query.CountAsync(x => x.Status == WorkflowStatus.Complete);
            var terminated = await query.CountAsync(x => x.Status == WorkflowStatus.Terminated);
            var suspended = await query.CountAsync(x => x.Status == WorkflowStatus.Suspended);

            return new InstanceOverviewStat
            {
                TotalCount = total,
                RunningCount = running,
                CompleteCount = complete,
                TerminatedCount = terminated,
                SuspendedCount = suspended
            };
        }

        public virtual async Task<List<DurationStat>> GetDurationStatListAsync(DateTime? startTime, DateTime? endTime, Guid? definitionId, string? groupBy, Guid? tenantId)
        {
            var dbSet = (await GetDbSetAsync()).Include(x => x.WkDefinition);
            var query = dbSet
                .Where(x => x.Status == WorkflowStatus.Complete && x.CompleteTime != null)
                .Where(x => !tenantId.HasValue || x.TenantId == tenantId.Value)
                .Where(x => !definitionId.HasValue || x.WkDifinitionId == definitionId.Value)
                .Where(x => !startTime.HasValue || x.CompleteTime >= startTime)
                .Where(x => !endTime.HasValue || x.CompleteTime <= endTime);

            List<DurationStat> result;
            if (groupBy == "Definition" || string.IsNullOrEmpty(groupBy))
            {
                result = await query
                    .GroupBy(x => new { x.WkDifinitionId, x.Version, x.WkDefinition.Title, x.WkDefinition.BusinessType })
                    .Select(g => new DurationStat
                    {
                        DefinitionId = g.Key.WkDifinitionId,
                        DefinitionTitle = g.Key.Title,
                        BusinessType = g.Key.BusinessType,
                        AvgDurationMinutes = g.Average(x => (x.CompleteTime!.Value - x.CreateTime).TotalMinutes),
                        MedianDurationMinutes = 0,
                        CompletedCount = g.Count()
                    })
                    .ToListAsync();
            }
            else if (groupBy == "BusinessType")
            {
                result = await query
                    .GroupBy(x => x.WkDefinition.BusinessType)
                    .Select(g => new DurationStat
                    {
                        DefinitionTitle = g.Key,
                        BusinessType = g.Key,
                        AvgDurationMinutes = g.Average(x => (x.CompleteTime!.Value - x.CreateTime).TotalMinutes),
                        MedianDurationMinutes = 0,
                        CompletedCount = g.Count()
                    })
                    .ToListAsync();
            }
            else
            {
                var all = await query.ToListAsync();
                var avg = all.Count > 0 ? all.Average(x => (x.CompleteTime!.Value - x.CreateTime).TotalMinutes) : 0;
                var sorted = all.Select(x => (x.CompleteTime!.Value - x.CreateTime).TotalMinutes).OrderBy(x => x).ToList();
                var median = sorted.Count > 0 ? (sorted.Count % 2 == 1 ? sorted[sorted.Count / 2] : (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2) : 0;
                result = [new DurationStat { AvgDurationMinutes = avg, MedianDurationMinutes = median, CompletedCount = all.Count }];
            }
            return result;
        }

        public virtual async Task<List<OverdueStat>> GetOverdueStatListAsync(DateTime? startTime, DateTime? endTime, Guid? definitionId, Guid? tenantId)
        {
            var dbSet = (await GetDbSetAsync())
                .Include(x => x.WkDefinition);
            var query = dbSet
                .Where(x => x.WkDefinition.LimitTime.HasValue && x.WkDefinition.LimitTime > 0)
                .Where(x => x.Status == WorkflowStatus.Runnable || x.Status == WorkflowStatus.Complete)
                .Where(x => !tenantId.HasValue || x.TenantId == tenantId.Value)
                .Where(x => !definitionId.HasValue || x.WkDifinitionId == definitionId.Value)
                .Where(x => !startTime.HasValue || x.CreateTime >= startTime.Value)
                .Where(x => !endTime.HasValue || x.CreateTime <= endTime.Value);

            var instances = await query.ToListAsync();
            var now = DateTime.UtcNow;
            var grouped = instances
                .GroupBy(x => new { x.WkDifinitionId, x.Version, x.WkDefinition.Title })
                .Select(g =>
                {
                    var total = g.Count();
                    var overdue = g.Count(i =>
                    {
                        var deadline = i.CreateTime.AddMinutes(i.WkDefinition.LimitTime ?? 0);
                        return (i.Status == WorkflowStatus.Runnable && now > deadline) || (i.Status == WorkflowStatus.Complete && i.CompleteTime.HasValue && i.CompleteTime.Value > deadline);
                    });
                    return new OverdueStat
                    {
                        DefinitionId = g.Key.WkDifinitionId,
                        DefinitionTitle = g.Key.Title,
                        TotalCount = total,
                        OverdueCount = overdue,
                        OverdueRate = total > 0 ? (double)overdue / total : 0
                    };
                })
                .ToList();
            return grouped;
        }

        public virtual async Task<List<DefinitionStat>> GetDefinitionStatListAsync(DateTime? startTime, DateTime? endTime, Guid? tenantId)
        {
            var dbSet = (await GetDbSetAsync()).Include(x => x.WkDefinition);
            var query = dbSet.AsQueryable()
                .Where(x => !tenantId.HasValue || x.TenantId == tenantId.Value)
                .Where(x => !startTime.HasValue || x.CreateTime >= startTime.Value)
                .Where(x => !endTime.HasValue || x.CreateTime <= endTime.Value);

            var list = await query
                .GroupBy(x => new { x.WkDifinitionId, x.Version, x.WkDefinition.Title })
                .Select(g => new DefinitionStat
                {
                    DefinitionId = g.Key.WkDifinitionId,
                    Title = g.Key.Title ?? "",
                    Version = g.Key.Version,
                    TotalCount = g.Count(),
                    RunningCount = g.Count(x => x.Status == WorkflowStatus.Runnable),
                    CompleteCount = g.Count(x => x.Status == WorkflowStatus.Complete),
                    TerminatedCount = g.Count(x => x.Status == WorkflowStatus.Terminated)
                })
                .ToListAsync();
            return list;
        }

        public virtual async Task<List<CreatorStat>> GetCreatorStatListAsync(DateTime? startTime, DateTime? endTime, Guid? creatorId, Guid? tenantId)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.AsQueryable()
                .Where(x => x.CreatorId != null)
                .Where(x => !tenantId.HasValue || x.TenantId == tenantId.Value)
                .Where(x => !creatorId.HasValue || x.CreatorId == creatorId.Value)
                .Where(x => !startTime.HasValue || x.CreateTime >= startTime.Value)
                .Where(x => !endTime.HasValue || x.CreateTime <= endTime.Value);

            var list = await query
                .GroupBy(x => x.CreatorId!.Value)
                .Select(g => new CreatorStat
                {
                    CreatorId = g.Key,
                    CreatedCount = g.Count(),
                    CompletedCount = g.Count(x => x.Status == WorkflowStatus.Complete),
                    CompletedRate = 0
                })
                .ToListAsync();
            foreach (var item in list)
                item.CompletedRate = item.CreatedCount > 0 ? (double)item.CompletedCount / item.CreatedCount : 0;
            return list;
        }

        public virtual async Task<List<TrendStat>> GetTrendStatListAsync(DateTime startTime, DateTime endTime, string granularity, Guid? tenantId)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet.AsQueryable()
                .Where(x => !tenantId.HasValue || x.TenantId == tenantId.Value)
                .Where(x => x.CreateTime >= startTime && x.CreateTime <= endTime);

            List<TrendStat> result;
            if (granularity == "month")
            {
                var createdByMonth = await query
                    .GroupBy(x => new { x.CreateTime.Year, x.CreateTime.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Created = g.Count() })
                    .ToListAsync();
                var completedByMonth = await dbSet
                    .Where(x => !tenantId.HasValue || x.TenantId == tenantId.Value)
                    .Where(x => x.CompleteTime >= startTime && x.CompleteTime <= endTime)
                    .GroupBy(x => new { x.CompleteTime!.Value.Year, x.CompleteTime.Value.Month })
                    .Select(g => new { g.Key.Year, g.Key.Month, Completed = g.Count() })
                    .ToListAsync();
                var periodStart = new DateTime(startTime.Year, startTime.Month, 1);
                var endMonth = new DateTime(endTime.Year, endTime.Month, 1);
                result = [];
                for (var d = periodStart; d <= endMonth; d = d.AddMonths(1))
                {
                    var created = createdByMonth.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Created ?? 0;
                    var completed = completedByMonth.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Completed ?? 0;
                    result.Add(new TrendStat { PeriodStart = d, CreatedCount = created, CompletedCount = completed });
                }
            }
            else if (granularity == "week")
            {
                var start = startTime.Date;
                var end = endTime.Date;
                var createdByWeek = await query
                    .GroupBy(x => StartOfWeek(x.CreateTime))
                    .Select(g => new { Period = g.Key, Created = g.Count() })
                    .ToListAsync();
                var completedByWeek = await dbSet
                    .Where(x => !tenantId.HasValue || x.TenantId == tenantId.Value)
                    .Where(x => x.CompleteTime >= startTime && x.CompleteTime <= endTime)
                    .GroupBy(x => StartOfWeek(x.CompleteTime!.Value))
                    .Select(g => new { Period = g.Key, Completed = g.Count() })
                    .ToListAsync();
                var periods = createdByWeek.Select(x => x.Period).Union(completedByWeek.Select(x => x.Period)).Distinct().OrderBy(x => x).ToList();
                result = [.. periods.Select(p => new TrendStat
                {
                    PeriodStart = p,
                    CreatedCount = createdByWeek.FirstOrDefault(x => x.Period == p)?.Created ?? 0,
                    CompletedCount = completedByWeek.FirstOrDefault(x => x.Period == p)?.Completed ?? 0
                })];
            }
            else
            {
                var createdByDay = await query
                    .GroupBy(x => x.CreateTime.Date)
                    .Select(g => new { Period = g.Key, Created = g.Count() })
                    .ToListAsync();
                var completedByDay = await dbSet
                    .Where(x => !tenantId.HasValue || x.TenantId == tenantId.Value)
                    .Where(x => x.CompleteTime >= startTime && x.CompleteTime <= endTime)
                    .GroupBy(x => x.CompleteTime!.Value.Date)
                    .Select(g => new { Period = g.Key, Completed = g.Count() })
                    .ToListAsync();
                var days = createdByDay.Select(x => x.Period).Union(completedByDay.Select(x => x.Period)).Distinct().OrderBy(x => x).ToList();
                result = [.. days.Select(p => new TrendStat
                {
                    PeriodStart = DateTime.SpecifyKind(p, DateTimeKind.Utc),
                    CreatedCount = createdByDay.FirstOrDefault(x => x.Period == p)?.Created ?? 0,
                    CompletedCount = completedByDay.FirstOrDefault(x => x.Period == p)?.Completed ?? 0
                })];
            }
            return result;
        }

        private static DateTime StartOfWeek(DateTime d)
        {
            var diff = (7 + (d.DayOfWeek - DayOfWeek.Monday)) % 7;
            return d.AddDays(-1 * diff).Date;
        }
    }
}
