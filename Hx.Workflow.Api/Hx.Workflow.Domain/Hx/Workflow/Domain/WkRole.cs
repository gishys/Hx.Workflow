using Hx.Workflow.Domain.Persistence;
using System.Collections.Generic;
using Volo.Abp.Identity;

namespace Hx.Workflow.Domain
{
    public class WkRole : IdentityRole
    {
        public virtual ICollection<WkDefinition> WkDefinitions { get; protected set; }
        public virtual ICollection<WkNode> WkNodes { get; protected set; }
    }
}
