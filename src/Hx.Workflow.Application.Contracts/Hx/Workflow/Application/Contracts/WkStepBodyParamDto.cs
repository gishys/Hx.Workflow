using Hx.Workflow.Domain.Shared;
using System;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyParamDto
    {
        public Guid WkNodeId { get; set; }
        public string Key { get; set; }
        public StepBodyParaType StepBodyParaType { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
    }
}
