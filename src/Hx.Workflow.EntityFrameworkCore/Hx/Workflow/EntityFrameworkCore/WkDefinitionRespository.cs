using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkDefinitionRespository
        : EfCoreRepository<WkDbContext, WkDefinition, Guid>,
        IWkDefinitionRespository
    {
        public WkDefinitionRespository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        /// <summary>
        /// Get entity by id an version
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinition> GetDefinitionAsync(Guid id, int version)
        {
            return (await GetDbSetAsync())
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
                .IncludeDetials(includeDetails)
                .Where(d => d.WkDefinitionState == WkDefinitionState.Enable)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }
        public virtual async Task<WkDefinition> GetDefinitionAsync(string name, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .IncludeDetials(includeDetails)
                .FirstOrDefaultAsync(d => d.Title == name,
                GetCancellationToken(cancellationToken));
        }
        public async Task<int> GetMaxSortNumberAsync()
        {
            return await (await GetDbSetAsync()).MaxAsync(d => d.SortNumber);
        }
        public virtual async Task<WkDefinition> UpdateCandidatesAsync(
            Guid wkDefinitionId,
            Guid userId,
            ICollection<WkCandidate> wkCandidates)
        {
            var entity = await (await GetDbSetAsync()).Include(d => d.WkCandidates).FirstOrDefaultAsync(d => d.Id == wkDefinitionId);
            if (entity?.WkCandidates != null)
            {
                var updateCnadidates = entity.WkCandidates.Where(d => d.CandidateId == userId).ToList();
                if (updateCnadidates.Count() > 0)
                {
                    foreach (var candidate in updateCnadidates.ToList())
                    {
                        WkCandidate updateCandidate = wkCandidates.FirstOrDefault(
                            d => d.CandidateId == candidate.CandidateId);
                        if (updateCandidate != null)
                            continue;
                        else
                        {
                            updateCandidate = new WkCandidate(
                                candidate.CandidateId,
                                candidate.UserName,
                                candidate.DisplayUserName);
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