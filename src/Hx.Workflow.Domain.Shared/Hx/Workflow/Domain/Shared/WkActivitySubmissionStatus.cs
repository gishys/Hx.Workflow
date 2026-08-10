namespace Hx.Workflow.Domain.Shared
{
    public enum WkActivitySubmissionStatus
    {
        Accepted = 0,
        Processing = 1,
        EventPublished = 2,
        Succeeded = 3,
        Failed = 4,
        Rejected = 5
    }
}
