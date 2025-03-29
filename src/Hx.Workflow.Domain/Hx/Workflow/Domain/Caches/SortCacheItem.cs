using System;

namespace Hx.Workflow.Domain.Caches
{
    public class SortCacheItem
    {
        public SortCacheItem(Guid? parentId, double sort)
        {
            ParentId = parentId;
            Sort = sort;
        }
        public Guid? ParentId { get; set; }
        public double Sort { get; set; }
    }
}
