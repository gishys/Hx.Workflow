using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.LocalEvents
{
    public class WkInstanceDeleteEventData(
        Guid instanceId,
        string reference
            )
    {
        public Guid InstanceId { get; set; } = instanceId;
        public string Reference { get; set; } = reference;
    }
}
