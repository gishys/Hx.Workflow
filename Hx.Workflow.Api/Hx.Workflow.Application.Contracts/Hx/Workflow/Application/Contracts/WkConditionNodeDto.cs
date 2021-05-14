using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkConditionNodeDto
    {
        public string Label { get; set; }
        public string NextNodeName { get; set; }
        public virtual ICollection<WkConNodeConditionDto> WkConNodeConditions { get; set; }
    }
}
