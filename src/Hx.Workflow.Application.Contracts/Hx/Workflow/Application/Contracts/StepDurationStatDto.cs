namespace Hx.Workflow.Application.Contracts
{
    public class StepDurationStatDto
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public double AvgDurationMinutes { get; set; }
        public int PassCount { get; set; }
    }
}
