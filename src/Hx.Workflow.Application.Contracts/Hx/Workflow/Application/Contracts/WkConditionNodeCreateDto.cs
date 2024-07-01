using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkConditionNodeCreateDto
    {
        public string Label { get; set; }
        public string NextNodeName { get; set; }
        public int NodeType { get; set; }
        public virtual ICollection<WkConNodeConditionCreateDto> WkConNodeConditions { get; set; }
    }
}