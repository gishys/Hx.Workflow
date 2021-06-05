using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain.Persistence
{
    public class WkExecutionPointer : FullAuditedEntity<Guid>, IMultiTenant
    {
        public Guid WkInstanceId { get; protected set; }
        public WkInstance WkInstance { get; protected set; }
        public int StepId { get; protected set; }
        public bool Active { get; protected set; }
        public DateTime? SleepUntil { get; protected set; }
        public string PersistenceData { get; protected set; }
        public DateTime? StartTime { get; protected set; }
        public DateTime? EndTime { get; protected set; }
        public string EventName { get; protected set; }
        public string EventKey { get; protected set; }
        public bool EventPublished { get; protected set; }
        public string EventData { get; protected set; }
        public string StepName { get; protected set; }
        public ICollection<WkExtensionAttribute> ExtensionAttributes { get; protected set; }
        public int RetryCount { get; protected set; }
        public string Children { get; protected set; }
        public string ContextItem { get; protected set; }
        public string PredecessorId { get; protected set; }
        public string Outcome { get; protected set; }
        public PointerStatus Status { get; protected set; }
        public string Scope { get; protected set; }
        public Guid? TenantId { get; protected set; }
        public ICollection<WkSubscription> WkSubscriptions { get; protected set; }
        public ICollection<WkCandidate> WkCandidates { get; protected set; }
        public WkExecutionPointer()
        { }
        public WkExecutionPointer(
            int stepId,
            bool active,
            DateTime? sleepUntil,
            string persistenceData,
            DateTime? startTime,
            DateTime? endTime,
            string eventName,
            string eventKey,
            bool eventPublished,
            string eventData,
            string stepName,
            int retryCount,
            string children,
            string contextItem,
            string predecessorId,
            string outcome,
            PointerStatus status,
            string scope,
            Guid? tenantId = null
            )
        {
            StepId = stepId;
            Active = active;
            SleepUntil = sleepUntil;
            PersistenceData = persistenceData;
            StartTime = startTime;
            EndTime = endTime;
            EventName = eventName;
            EventKey = eventKey;
            EventPublished = eventPublished;
            EventData = eventData;
            StepName = stepName;
            RetryCount = retryCount;
            Children = children;
            ContextItem = contextItem;
            PredecessorId = predecessorId;
            Outcome = outcome;
            Status = status;
            Scope = scope;
            TenantId = tenantId;
            ExtensionAttributes = new List<WkExtensionAttribute>();
        }
        public Task SetStepId(int stepId)
        {
            StepId = stepId;
            return Task.CompletedTask;
        }
        public Task SetActive(bool active)
        {
            Active = active;
            return Task.CompletedTask;
        }
        public Task SetSleepUntil(DateTime? sleepUntil)
        {
            SleepUntil = sleepUntil;
            return Task.CompletedTask;
        }
        public Task SetPersistenceData(string persistenceData)
        {
            PersistenceData = persistenceData;
            return Task.CompletedTask;
        }
        public Task SetStartTime(DateTime? startTime)
        {
            StartTime = startTime;
            return Task.CompletedTask;
        }
        public Task SetEndTime(DateTime? endTime)
        {
            EndTime = endTime;
            return Task.CompletedTask;
        }
        public Task SetEventName(string eventName)
        {
            EventName = eventName;
            return Task.CompletedTask;
        }
        public Task SetEventKey(string eventKey)
        {
            EventKey = eventKey;
            return Task.CompletedTask;
        }
        public Task SetEventPublished(bool eventPublished)
        {
            EventPublished = eventPublished;
            return Task.CompletedTask;
        }
        public Task SetEventData(string eventData)
        {
            EventData = eventData;
            return Task.CompletedTask;
        }
        public Task SetStepName(string stepName)
        {
            StepName = stepName;
            return Task.CompletedTask;
        }
        public Task SetRetryCount(int retryCount)
        {
            RetryCount = retryCount;
            return Task.CompletedTask;
        }
        public Task SetChildren(string children)
        {
            Children = children;
            return Task.CompletedTask;
        }
        public Task SetContextItem(string contextItem)
        {
            ContextItem = contextItem;
            return Task.CompletedTask;
        }
        public Task SetPredecessorId(string predecessorId)
        {
            PredecessorId = predecessorId;
            return Task.CompletedTask;
        }
        public Task SetOutcome(string outcome)
        {
            Outcome = outcome;
            return Task.CompletedTask;
        }
        public Task SetStatus(PointerStatus status)
        {
            Status = status;
            return Task.CompletedTask;
        }
        public Task SetScope(string scope)
        {
            Scope = scope;
            return Task.CompletedTask;
        }
        public Task SetWkInstanceId(Guid instanceId)
        {
            WkInstanceId = instanceId;
            return Task.CompletedTask;
        }
        public Task SetExtensionAttributes(WkExtensionAttribute extensionAttribute)
        {
            ExtensionAttributes.Add(extensionAttribute);
            return Task.CompletedTask;
        }
        public Task AddCandidates(ICollection<WkCandidate> wkCandidates)
        {
            WkCandidates = wkCandidates;
            return Task.CompletedTask;
        }
    }
}