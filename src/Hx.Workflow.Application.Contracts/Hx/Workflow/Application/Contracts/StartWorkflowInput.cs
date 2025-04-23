using System;
using System.Collections.Generic;
using System.Text;

namespace Hx.Workflow.Application.Contracts
{
    public class StartWorkflowInput
    {
        public required string Id { get; set; }
        public int Version { get; set; }
        public Dictionary<string, object> Inputs { get; set; } = [];
    }
}
