using Hx.Workflow.Domain.Shared;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkConditionNodeDto
    {
        public string Label { get; set; }
        public string NextNodeName { get; set; }
        public WkRoleNodeType NodeType { get; set; }
        public virtual ICollection<WkConNodeConditionDto> WkConNodeConditions { get; set; }
    }
}
