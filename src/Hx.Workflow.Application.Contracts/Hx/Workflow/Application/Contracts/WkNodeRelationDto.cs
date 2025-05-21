using Hx.Workflow.Domain.Shared;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkNodeRelationDto
    {
        public string? Label { get; set; }
        public required string NextNodeName { get; set; }
        public WkRoleNodeType NodeType { get; set; }
        public virtual ICollection<WkNodeRelationRuleDto>? Rules { get; set; }
    }
}
