using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;

namespace Hx.Workflow.Application.LocalEvents
{
    public class WkInstanceDeleteEventData(
        Guid instanceId,
        string reference,
        string title,
        string businessType,
        string processType)
    {
        public Guid InstanceId { get; set; } = instanceId;
        public string Reference { get; set; } = reference;
        public string Title { get; set; } = title;
        public string BusinessType { get; set; } = businessType;
        public string ProcessType { get; set; } = processType;
    }
}
