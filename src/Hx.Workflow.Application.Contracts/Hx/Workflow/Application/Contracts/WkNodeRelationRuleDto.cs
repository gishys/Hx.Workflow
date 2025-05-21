namespace Hx.Workflow.Application.Contracts
{
    public class WkNodeRelationRuleDto
    {
        public required string Field { get; set; }
        public required string Operator { get; set; }
        public required string Value { get; set; }
    }
}
