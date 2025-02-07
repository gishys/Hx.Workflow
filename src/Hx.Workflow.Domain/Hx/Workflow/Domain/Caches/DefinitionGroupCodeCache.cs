using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Domain.Caches
{
    public class DefinitionGroupCodeCache
    {
        public Guid? ParentId { get; set; }
        public string Code { get; set; }
        public DefinitionGroupCodeCache(Guid? parentId, string code)
        {
            ParentId = parentId;
            Code = code;
        }
    }
}
