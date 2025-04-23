using Hx.Workflow.Domain.Shared;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkConditionNodeCreateDto
    {
        public required string NextNodeName { get; set; }
        public WkRoleNodeType NodeType { get; set; }
        public virtual ICollection<WkConNodeConditionCreateDto>? WkConNodeConditions { get; set; }
    }
}