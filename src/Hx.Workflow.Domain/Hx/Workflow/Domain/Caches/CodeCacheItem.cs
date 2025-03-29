using System;

namespace Hx.Workflow.Domain.Caches
{
    public class CodeCacheItem
    {
        public CodeCacheItem(Guid? parentId, string code)
        {
            ParentId = parentId;
            Code = code;
        }
        public Guid? ParentId { get; set; }
        public string Code { get; set; }
    }
}
