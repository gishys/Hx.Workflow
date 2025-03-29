using Hx.Workflow.Domain.Caches;
using Hx.Workflow.Domain.Persistence;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace Hx.Workflow.Domain
{
    public abstract class BaseGroupManager<IRepository> : IDomainService where IRepository : IGroupRepository
    {
        public IDistributedCache<SortCacheItem> SortCache { get; }
        public IDistributedCache<CodeCacheItem> CodeCache { get; }
        public IRepository GroupRepository { get; }
        protected abstract string CachePrefix { get;}
        public BaseGroupManager(
            IDistributedCache<SortCacheItem> sortCache,
            IRepository groupRepository,
            IDistributedCache<CodeCacheItem> codeCache)
        {
            SortCache = sortCache;
            GroupRepository = groupRepository;
            CodeCache = codeCache;
        }
        /// <summary>
        /// 通过父Id获取下一个排序号
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public virtual async Task<double> GetNextOrderNumberAsync(Guid? parentId)
        {
            var key = $"{CachePrefix}:{parentId?.ToString() ?? "root"}";
            var cache = await SortCache.GetOrAddAsync(key,
                    async () =>
                    {
                        var maxNumber = await GroupRepository.GetMaxOrderNumberAsync(parentId);
                        return new SortCacheItem(parentId, maxNumber);
                    },
                    () => new DistributedCacheEntryOptions()
                    {
                    });
            var maxNumber = cache?.Sort ?? 0;
            maxNumber++;
            await SortCache.SetAsync(key, new SortCacheItem(parentId, maxNumber));
            return maxNumber;
        }
        /// <summary>
        /// 通过父Id获取下一个路径枚举
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public virtual async Task<string> GetNextCodeAsync(Guid? parentId)
        {
            var key = $"{CachePrefix}:{parentId?.ToString() ?? "root"}";
            var cache = await CodeCache.GetOrAddAsync(key,
                    async () =>
                    {
                        string maxNumber = await GroupRepository.GetMaxCodeNumberAsync(parentId);
                        return new CodeCacheItem(parentId, maxNumber);
                    },
                    () => new DistributedCacheEntryOptions()
                    {
                    });
            var maxCode = cache?.Code ?? ApplicationFormGroup.CreateCode([0]);
            maxCode = ApplicationFormGroup.CalculateNextCode(maxCode);
            await CodeCache.SetAsync(key, new CodeCacheItem(parentId, maxCode));
            return maxCode;
        }
    }
}