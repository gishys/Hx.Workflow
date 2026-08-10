using Hx.Workflow.Domain.Shared;
using System;

namespace Hx.Workflow.Application.Contracts
{
    public class WkActivitySubmissionResultDto
    {
        public Guid? SubmissionId { get; set; }
        public Guid WorkflowId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public WkActivitySubmissionStatus Status { get; set; }
        public string? Error { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
