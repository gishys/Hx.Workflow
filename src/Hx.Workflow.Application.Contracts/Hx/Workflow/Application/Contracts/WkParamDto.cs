using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkParamDto
    {
        public string WkParamKey { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public object Value { get; set; }
    }
}
