using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Identity;

namespace Hx.Workflow.Domain
{
    public class WkUser : IdentityUser
    {
        public virtual ICollection<WkDefinition> WkDefinitions { get; protected set; }
        public virtual ICollection<WkNode> WkNodes { get; protected set; }
    }
}
