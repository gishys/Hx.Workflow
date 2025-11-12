using Hx.Workflow.Domain;
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
                : EfCoreRepository<WkDbContext, WkDefinition>(options),
        IWkDefinitionRespository
    {
        public async Task<WkDefinition?> FindAsync(
            object[] id, bool includeDetails = true, CancellationToken cancellation = default)
        {
            if (id.Length != 2 || id[0] is not Guid definitionId || id[1] is not int version)
            {
                return null;
            }
            
            return await (await GetDbSetAsync())
                    .IncludeDetails(includeDetails)
                    .FirstOrDefaultAsync(d => d.Id == definitionId && d.Version == version, cancellation);
        }
        
        public async Task<WkDefinition?> FindAsync(
            Guid id, bool includeDetails = true, CancellationToken cancellation = default)
        {
            // 默认返回最新版本
            return await (await GetDbSetAsync())
                    .IncludeDetails(includeDetails)
                    .Where(d => d.Id == id)
                    .OrderByDescending(d => d.Version)
                    .FirstOrDefaultAsync(cancellation);
        }
        
        /// <summary>
        /// 获取指定ID和版本的实体
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="version">版本号</param>
        /// <param name="includeDetails">是否包含详细信息</param>
        /// <param name="cancellation">取消令牌</param>
        /// <returns></returns>
        public virtual async Task<WkDefinition?> FindAsync(
            Guid id, int version, bool includeDetails = true, CancellationToken cancellation = default)
        {
            return await (await GetDbSetAsync())
                    .IncludeDetails(includeDetails)
                    .FirstOrDefaultAsync(d => d.Id == id && d.Version == version, cancellation);
        }
        
        /// <summary>
        /// Get entity by id an version
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinition?> GetDefinitionAsync(Guid id, int version, CancellationToken cancellation = default)
        {
            return await (await GetDbSetAsync())
                .IncludeDetails(true)
                .FirstOrDefaultAsync(d => d.Id == id && d.Version == version, cancellation);
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
                .GroupBy(d => d.Id)
                .Select(g => g.OrderByDescending(d => d.Version).First())
                .ToListAsync(GetCancellationToken(cancellationToken));
        }
        public virtual async Task<WkDefinition?> GetDefinitionAsync(string name, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .IncludeDetails(includeDetails)
                .Where(d => d.Title == name)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
        }
        public virtual async Task<List<WkNodeCandidate>> GetCandidatesAsync(Guid id, string stepName, CancellationToken cancellationToken = default)
        {
            var definition = await (await GetDbSetAsync()).IncludeDetails(true)
                .Where(d => d.Id == id)
                .OrderByDescending(d => d.Version)
                .FirstOrDefaultAsync(GetCancellationToken(cancellationToken))
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
        
        /// <summary>
        /// 获取指定模板的最大版本号
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>最大版本号，如果没有版本则返回0</returns>
        public virtual async Task<int> GetMaxVersionAsync(Guid definitionId, CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            var versions = await dbSet
                .Where(d => d.Id == definitionId)
                .Select(d => d.Version)
                .ToListAsync(GetCancellationToken(cancellationToken));
            
            return versions.Count != 0 ? versions.Max() : 1;
        }
        
        /// <summary>
        /// 获取指定模板的所有版本
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>所有版本列表</returns>
        public virtual async Task<List<WkDefinition>> GetAllVersionsAsync(Guid definitionId, CancellationToken cancellationToken = default)
        {
            return await (await GetDbSetAsync())
                .IncludeDetails(true)
                .Where(d => d.Id == definitionId)
                .OrderByDescending(d => d.Version)
                .ToListAsync(GetCancellationToken(cancellationToken));
        }
        
        /// <summary>
        /// 检查指定ID和版本是否存在
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="version">版本号</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否存在</returns>
        public virtual async Task<bool> ExistsAsync(Guid id, int version, CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .AnyAsync(d => d.Id == id && d.Version == version, GetCancellationToken(cancellationToken));
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
