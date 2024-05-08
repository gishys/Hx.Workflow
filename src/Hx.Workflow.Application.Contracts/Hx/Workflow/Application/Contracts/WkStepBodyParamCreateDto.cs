using Hx.Workflow.Domain.Shared;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyParamCreateDto
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Value { get; set; }
        public StepBodyParaType StepBodyParaType { get; set; }
    }
}
