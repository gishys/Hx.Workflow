using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkConNodeConditionCreateDto
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }
}
