using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkActivityInputDto
    {
        public required string ActivityName { get; set; }
        public required string WorkflowId { get; set; }
        public required Dictionary<string, object> Data { get; set; }
    }
}