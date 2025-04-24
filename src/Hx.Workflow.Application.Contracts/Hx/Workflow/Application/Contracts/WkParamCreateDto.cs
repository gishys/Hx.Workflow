namespace Hx.Workflow.Application.Contracts
{
    public class WkParamCreateDto
    {
        public required string WkParamKey { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required string Value { get; set; }
    }
}
