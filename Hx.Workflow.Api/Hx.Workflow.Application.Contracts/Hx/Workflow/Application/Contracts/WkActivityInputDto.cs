using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class WkActivityInputDto
    {
        public string ActivityName { get; set; }
        public string WorkflowId { get; set; }
        public object Data { get; set; }
    }
}