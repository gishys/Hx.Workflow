using Hx.Workflow.Domain.Shared;
using System;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyParamDto
    {
        public required Guid WkStepBodyId { get; set; }
        public required string Key { get; set; }
        public required StepBodyParaType StepBodyParaType { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Value { get; set; }
    }
}
