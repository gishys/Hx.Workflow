﻿using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkDefinitionRespository(IDbContextProvider<WkDbContext> options)
                : EfCoreRepository<WkDbContext, WkDefinition, Guid>(options),
        IWkDefinitionRespository
    {
        public override async Task<WkDefinition?> FindAsync(
            Guid id, bool includeDetails = true, CancellationToken cancellation = default)
        {
            return await (await GetDbSetAsync())
                    .IncludeDetails(includeDetails)
                    .FirstOrDefaultAsync(d => d.Id == id, cancellation);
        }
        /// <summary>
        /// Get entity by id an version
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinition?> GetDefinitionAsync(Guid id, int version, CancellationToken cancellation = default)
        {
            return (await GetDbSetAsync())
                .IncludeDetails(true)
                .FirstOrDefault(x => x.Id == id && x.Version == version);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeDetails"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<List<WkDefinition>> GetListAsync(bool includeDetails = false, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .IncludeDetails(includeDetails)
                .Where(d => d.IsEnabled)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }
        public async Task<List<WkDefinition>> GetListHasPermissionAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .Where(d => d.IsEnabled && d.WkCandidates.Any(c => c.CandidateId == userId))
                .Include(d => d.Nodes)
                .ThenInclude(d => d.NextNodes)
                .ThenInclude(d => d.Rules)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }
        public virtual async Task<WkDefinition?> GetDefinitionAsync(string name, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .IncludeDetails(includeDetails)
                .FirstOrDefaultAsync(d => d.Title == name,
                GetCancellationToken(cancellationToken));
        }
        public virtual async Task<List<WkNodeCandidate>> GetCandidatesAsync(Guid id, string stepName, CancellationToken cancellationToken = default)
        {
            var definition = await (await GetDbSetAsync()).IncludeDetails(true)
                .FirstOrDefaultAsync(d => d.Id == id, GetCancellationToken(cancellationToken))
                ?? throw new UserFriendlyException(message: $"Id为[{id}]的流程模板不存在！");
            var node = definition.Nodes.FirstOrDefault(n => n.Name == stepName) ?? throw new UserFriendlyException(message: $"Name为[{stepName}]的流程步骤不存在！");
            return [.. node.WkCandidates];
        }
        public async Task<int> GetMaxSortNumberAsync()
        {
            var dbSet = await GetDbSetAsync();
            if (dbSet.Any())
            {
                return await dbSet.Select(d => d.SortNumber).MaxAsync();
            }
            return 0;
        }
        public virtual async Task<WkDefinition?> UpdateCandidatesAsync(
            Guid wkDefinitionId,
            Guid userId,
            ICollection<DefinitionCandidate> wkCandidates)
        {
            var entity = await (await GetDbSetAsync()).Include(d => d.WkCandidates).FirstOrDefaultAsync(d => d.Id == wkDefinitionId);
            if (entity?.WkCandidates != null)
            {
                var updateCnadidates = entity.WkCandidates.Where(d => d.CandidateId == userId).ToList();
                if (updateCnadidates.Count > 0)
                {
                    foreach (var candidate in updateCnadidates.ToList())
                    {
                        DefinitionCandidate? updateCandidate = wkCandidates.FirstOrDefault(
                            d => d.CandidateId == candidate.CandidateId);
                        if (updateCandidate != null)
                            continue;
                        else
                        {
                            updateCandidate = new DefinitionCandidate(
                                candidate.CandidateId,
                                candidate.UserName,
                                candidate.DisplayUserName,
                                candidate.ExecutorType,
                                candidate.DefaultSelection);
                            updateCnadidates.Add(updateCandidate);
                        }
                        await updateCandidate.SetSelection(true);
                    }
                    return await UpdateAsync(entity);
                }
            }
            return null;
        }
    }
}