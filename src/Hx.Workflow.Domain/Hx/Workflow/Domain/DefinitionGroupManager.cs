using Hx.Workflow.Domain.Caches;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace Hx.Workflow.Domain
{
    public class DefinitionGroupManager : IDomainService
    {
        public IDistributedCache<DefinitionGroupSortCache> SortCache { get; }
        public IDistributedCache<DefinitionGroupCodeCache> CodeCache { get; }
        public IWkDefinitionGroupRepository DefinitionGroupRepository { get; }
        public DefinitionGroupManager(
            IDistributedCache<DefinitionGroupSortCache> sortCache,
            IWkDefinitionGroupRepository definitionGroupRepository,
            IDistributedCache<DefinitionGroupCodeCache> codeCache)
        {
            SortCache = sortCache;
            DefinitionGroupRepository = definitionGroupRepository;
            CodeCache = codeCache;
        }
        /// <summary>
        /// 通过父Id获取下一个排序号
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public virtual async Task<double> GetNextOrderNumberAsync(Guid? parentId)
        {
            var key = parentId?.ToString() ?? "root";
            var cache = await SortCache.GetOrAddAsync(key,
                    async () =>
                    {
                        var maxNumber = await DefinitionGroupRepository.GetMaxOrderNumberAsync(parentId);
                        return new DefinitionGroupSortCache(parentId, maxNumber);
                    },
                    () => new DistributedCacheEntryOptions()
                    {
                    });
            var maxNumber = cache?.Sort ?? 0;
            maxNumber++;
            await SortCache.SetAsync(key, new DefinitionGroupSortCache(parentId, maxNumber));
            return maxNumber;
        }
        /// <summary>
        /// 通过父Id获取下一个路径枚举
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public virtual async Task<string> GetNextCodeAsync(Guid? parentId)
        {
            var key = parentId?.ToString() ?? "root";
            var cache = await CodeCache.GetOrAddAsync(key,
                    async () =>
                    {
                        string maxNumber = await DefinitionGroupRepository.GetMaxCodeNumberAsync(parentId);
                        return new DefinitionGroupCodeCache(parentId, maxNumber);
                    },
                    () => new DistributedCacheEntryOptions()
                    {
                    });
            var maxCode = cache?.Code ?? WkDefinitionGroup.CreateCode([0]);
            maxCode = WkDefinitionGroup.CalculateNextCode(maxCode);
            await CodeCache.SetAsync(key, new DefinitionGroupCodeCache(parentId, maxCode));
            return maxCode;
        }
    }
}
