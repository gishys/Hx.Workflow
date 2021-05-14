using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkInstance : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        public Guid WkDifinitionId { get; protected set; }
        public WkDefinition WkDefinition { get; protected set; }
        public int Version { get; protected set; }
        public string Description { get; protected set; }
        public string Reference { get; protected set; }
        public virtual ICollection<WkExecutionPointer> ExecutionPointers { get; protected set; }
        public long? NextExecution { get; protected set; }
        public WorkflowStatus Status { get; protected set; }
        public string Data { get; protected set; }
        public DateTime CreateTime { get; protected set; }
        public DateTime? CompleteTime { get; protected set; }
        public Guid? TenantId { get; protected set; }
        //public byte[] RowVersion { get; protected set; }
        public WkInstance()
        { }
        public WkInstance(
            Guid id,
            Guid wkDifinitionId,
            int version,
            string description,
            string reference,
            long? nextExecution,
            WorkflowStatus status,
            string data,
            DateTime createTime,
            DateTime? completeTime,
            Guid? tenantId = null)
        {
            Id = id;
            WkDifinitionId = wkDifinitionId;
            Version = version;
            Description = description;
            Reference = reference;
            NextExecution = nextExecution;
            Status = status;
            Data = data;
            CreateTime = createTime;
            CompleteTime = completeTime;
            TenantId = tenantId;
            ExecutionPointers = new List<WkExecutionPointer>();
        }
        public Task SetWkDifinitionId(Guid wkDifinitionId)
        {
            WkDifinitionId = wkDifinitionId;
            return Task.CompletedTask;
        }
        public Task SetVersion(int version)
        {
            Version = version;
            return Task.CompletedTask;
        }
        public Task SetDescription(string description)
        {
            Description = description;
            return Task.CompletedTask;
        }
        public Task SetReference(string reference)
        {
            Reference = reference;
            return Task.CompletedTask;
        }
        public Task SetNextExecution(long? nextExecution)
        {
            NextExecution = nextExecution;
            return Task.CompletedTask;
        }
        public Task SetStatus(WorkflowStatus status)
        {
            Status = status;
            return Task.CompletedTask;
        }
        public Task SetData(string data)
        {
            Data = data;
            return Task.CompletedTask;
        }
        public Task SetCreateTime(DateTime createTime)
        {
            CreateTime = createTime;
            return Task.CompletedTask;
        }
        public Task SetCompleteTime(DateTime? completeTime)
        {
            CompleteTime = completeTime;
            return Task.CompletedTask;
        }
        public Task AddExecutionPointer(WkExecutionPointer wkExecutionPointer)
        {
            ExecutionPointers.Add(wkExecutionPointer);
            return Task.CompletedTask;
        }
    }
}