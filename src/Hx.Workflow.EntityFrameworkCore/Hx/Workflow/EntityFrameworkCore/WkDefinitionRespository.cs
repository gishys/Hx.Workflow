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
            // 使用子查询直接获取最大版本，避免排序操作，提高查询性能
            var dbSet = await GetDbSetAsync();
            return await dbSet
                    .IncludeDetails(includeDetails)
                    .Where(d => d.Id == id && 
                               d.Version == dbSet
                                   .Where(d2 => d2.Id == id)
                                   .Max(d2 => (int?)d2.Version))
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
            // 返回指定名称的最新版本
            // 使用子查询直接获取最大版本，避免排序操作，提高查询性能
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .IncludeDetails(includeDetails)
                .Where(d => d.Title == name && 
                           d.Version == dbSet
                               .Where(d2 => d2.Title == name)
                               .Max(d2 => (int?)d2.Version))
                .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
        }
        public virtual async Task<List<WkNodeCandidate>> GetCandidatesAsync(Guid id, string stepName, CancellationToken cancellationToken = default)
        {
            // 获取指定ID的最新版本的候选人
            // 使用子查询直接获取最大版本，避免排序操作，提高查询性能
            var dbSet = await GetDbSetAsync();
            var definition = await dbSet
                .IncludeDetails(true)
                .Where(d => d.Id == id && 
                           d.Version == dbSet
                               .Where(d2 => d2.Id == id)
                               .Max(d2 => (int?)d2.Version))
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
        /// <returns>最大版本号，如果没有版本则返回1</returns>
        public virtual async Task<int> GetMaxVersionAsync(Guid definitionId, CancellationToken cancellationToken = default)
        {
            // 直接在数据库层面获取最大版本号，避免加载所有版本到内存
            var dbSet = await GetDbSetAsync();
            var maxVersion = await dbSet
                .Where(d => d.Id == definitionId)
                .MaxAsync(d => (int?)d.Version, GetCancellationToken(cancellationToken));
            
            return maxVersion ?? 1;
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
            // 获取指定ID的最新版本
            var dbSet = await GetDbSetAsync();
            var entity = await dbSet
                .Include(d => d.WkCandidates)
                .Where(d => d.Id == wkDefinitionId && 
                           d.Version == dbSet
                               .Where(d2 => d2.Id == wkDefinitionId)
                               .Max(d2 => (int?)d2.Version))
                .FirstOrDefaultAsync();
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
