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
    public class ApplicationFormGroupManager : IDomainService
    {
        public IDistributedCache<ApplicationFormGroupSortCache> SortCache { get; }
        public IDistributedCache<ApplicationFormGroupCodeCache> CodeCache { get; }
        public IApplicationFormGroupRepository ApplicationFormGroupRepository { get; }
        public ApplicationFormGroupManager(
            IDistributedCache<ApplicationFormGroupSortCache> sortCache,
            IApplicationFormGroupRepository applicationFormGroupRepository,
            IDistributedCache<ApplicationFormGroupCodeCache> codeCache)
        {
            SortCache = sortCache;
            ApplicationFormGroupRepository = applicationFormGroupRepository;
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
                        var maxNumber = await ApplicationFormGroupRepository.GetMaxOrderNumberAsync(parentId);
                        return new ApplicationFormGroupSortCache(parentId, maxNumber);
                    },
                    () => new DistributedCacheEntryOptions()
                    {
                    });
            var maxNumber = cache?.Sort ?? 0;
            maxNumber++;
            await SortCache.SetAsync(key, new ApplicationFormGroupSortCache(parentId, maxNumber));
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
                        string maxNumber = await ApplicationFormGroupRepository.GetMaxCodeNumberAsync(parentId);
                        return new ApplicationFormGroupCodeCache(parentId, maxNumber);
                    },
                    () => new DistributedCacheEntryOptions()
                    {
                    });
            var maxCode = cache?.Code ?? ApplicationFormGroup.CreateCode([0]);
            maxCode = ApplicationFormGroup.CalculateNextCode(maxCode);
            await CodeCache.SetAsync(key, new ApplicationFormGroupCodeCache(parentId, maxCode));
            return maxCode;
        }
    }
}
