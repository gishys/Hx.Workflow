using Hx.Workflow.Domain.Caches;
using Hx.Workflow.Domain.Repositories;
using Volo.Abp.Caching;

namespace Hx.Workflow.Domain
{
    public class DefinitionGroupManager(
    IDistributedCache<SortCacheItem> sortCache,
    IWkDefinitionGroupRepository applicationFormGroupRepository,
    IDistributedCache<CodeCacheItem> codeCache) : BaseGroupManager<IWkDefinitionGroupRepository>
        (sortCache, applicationFormGroupRepository, codeCache)
    {
        protected override string CachePrefix => "DefinitionGroup";
    }
}
