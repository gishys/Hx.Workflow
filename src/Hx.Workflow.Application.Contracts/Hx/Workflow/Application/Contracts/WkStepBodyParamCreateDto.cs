using Hx.Workflow.Domain.Shared;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyParamCreateDto
    {
        public required string Key { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Value { get; set; }
        public StepBodyParaType StepBodyParaType { get; set; }
    }
}
