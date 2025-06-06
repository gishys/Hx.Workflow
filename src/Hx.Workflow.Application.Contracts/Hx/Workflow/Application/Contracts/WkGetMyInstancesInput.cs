using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkGetMyInstancesInput
    {
        public ICollection<Guid>? CreatorIds { get; set; }
        public ICollection<Guid>? DefinitionIds { get; set; }
        public IDictionary<string, object>? InstanceData { get; set; }
        public ICollection<Guid>? userIds { get; set; }
    }
}