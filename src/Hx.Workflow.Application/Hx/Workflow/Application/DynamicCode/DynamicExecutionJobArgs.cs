using Volo.Abp.BackgroundJobs;

namespace Hx.Workflow.Application.DynamicCode
{
    [BackgroundJobName("DynamicExecutionJob")]
    public class DynamicExecutionJobArgs
    {
        public required string ClassCode { get; set; }
        public string MethodName { get; set; } = "Execute";
        public object[]? Parameters { get; set; }
    }
}
