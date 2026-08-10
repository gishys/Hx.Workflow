using Hx.Workflow.Domain.Shared;
using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkActivitySubmission : Entity<Guid>, IMultiTenant
    {
        public Guid WorkflowId { get; protected set; }
        public string ActivityName { get; protected set; }
        public string Payload { get; protected set; }
        public string RequestHash { get; protected set; }
        public WkActivitySubmissionStatus Status { get; protected set; }
        public string? Error { get; protected set; }
        public DateTime CreationTime { get; protected set; }
        public DateTime LastModificationTime { get; protected set; }
        public DateTime? LockedUntil { get; protected set; }
        public int AttemptCount { get; protected set; }
        public Guid? TenantId { get; protected set; }

#pragma warning disable CS8618
        protected WkActivitySubmission()
#pragma warning restore CS8618
        {
        }

        public WkActivitySubmission(
            Guid id,
            Guid workflowId,
            string activityName,
            string payload,
            string requestHash,
            DateTime creationTime,
            Guid? tenantId = null)
        {
            Id = id;
            WorkflowId = workflowId;
            ActivityName = activityName;
            Payload = payload;
            RequestHash = requestHash;
            Status = WkActivitySubmissionStatus.Accepted;
            CreationTime = creationTime;
            LastModificationTime = creationTime;
            TenantId = tenantId;
        }
    }
}
