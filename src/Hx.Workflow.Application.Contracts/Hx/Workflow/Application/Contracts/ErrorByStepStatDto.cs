namespace Hx.Workflow.Application.Contracts
{
    public class ErrorByStepStatDto
    {
        public int StepId { get; set; }
        public string StepName { get; set; } = string.Empty;
        public int ErrorCount { get; set; }
    }
}
