using Hx.Workflow.Domain.Caches;
using Hx.Workflow.Domain.Repositories;
using Volo.Abp.Caching;

namespace Hx.Workflow.Domain
{
    public class ApplicationFormGroupManager(
    IDistributedCache<SortCacheItem> sortCache,
    IApplicationFormGroupRepository applicationFormGroupRepository,
    IDistributedCache<CodeCacheItem> codeCache) : BaseGroupManager<IApplicationFormGroupRepository>
        (sortCache, applicationFormGroupRepository, codeCache)
    {
        protected override string CachePrefix => "ApplicationFormGroup";
    }
}
