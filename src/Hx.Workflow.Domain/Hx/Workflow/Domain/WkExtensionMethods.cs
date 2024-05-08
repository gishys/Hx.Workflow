using Hx.Workflow.Domain.Persistence;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain
{
    public static class WkExtensionMethods
    {
        private static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        internal static WkExecutionError ToPersistable(this ExecutionError instance)
        {
            var errorTime = DateTime.SpecifyKind(instance.ErrorTime, DateTimeKind.Unspecified);
            return new WkExecutionError(
                new Guid(instance.WorkflowId),
                new Guid(instance.ExecutionPointerId),
                errorTime,
                instance.Message);
        }
        internal static WorkflowInstance ToWorkflowInstance(this WkInstance instance)
        {
            WorkflowInstance result = new WorkflowInstance();
            result.Data = JsonConvert.DeserializeObject(instance.Data, SerializerSettings);
            result.Description = instance.Description;
            result.Reference = instance.Reference;
            result.Id = instance.Id.ToString();
            result.NextExecution = instance.NextExecution;
            result.Version = instance.Version;
            result.WorkflowDefinitionId = instance.WkDifinitionId.ToString();
            result.Status = instance.Status;
            result.CreateTime = DateTime.SpecifyKind(instance.CreateTime, DateTimeKind.Unspecified);
            if (instance.CompleteTime.HasValue)
                result.CompleteTime = DateTime.SpecifyKind(instance.CompleteTime.Value, DateTimeKind.Unspecified);

            result.ExecutionPointers = new ExecutionPointerCollection(instance.ExecutionPointers.Count + 8);

            foreach (var ep in instance.ExecutionPointers)
            {
                var pointer = new ExecutionPointer();

                pointer.Id = ep.Id.ToString();
                pointer.StepId = ep.StepId;
                pointer.Active = ep.Active;

                if (ep.SleepUntil.HasValue)
                    pointer.SleepUntil = DateTime.SpecifyKind(ep.SleepUntil.Value, DateTimeKind.Unspecified);

                pointer.PersistenceData = JsonConvert.DeserializeObject(ep.PersistenceData ?? string.Empty, SerializerSettings);

                if (ep.StartTime.HasValue)
                    pointer.StartTime = DateTime.SpecifyKind(ep.StartTime.Value, DateTimeKind.Unspecified);

                if (ep.EndTime.HasValue)
                    pointer.EndTime = DateTime.SpecifyKind(ep.EndTime.Value, DateTimeKind.Unspecified);

                pointer.StepName = ep.StepName;

                pointer.RetryCount = ep.RetryCount;
                pointer.PredecessorId = ep.PredecessorId;
                pointer.ContextItem = JsonConvert.DeserializeObject(ep.ContextItem ?? string.Empty, SerializerSettings);

                if (!string.IsNullOrEmpty(ep.Children))
                    pointer.Children = ep.Children.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                pointer.EventName = ep.EventName;
                pointer.EventKey = ep.EventKey;
                pointer.EventPublished = ep.EventPublished;
                pointer.EventData = JsonConvert.DeserializeObject(ep.EventData ?? string.Empty, SerializerSettings);
                pointer.Outcome = JsonConvert.DeserializeObject(ep.Outcome ?? string.Empty, SerializerSettings);
                pointer.Status = ep.Status;

                if (!string.IsNullOrEmpty(ep.Scope))
                    pointer.Scope = new List<string>(ep.Scope.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));

                foreach (var attr in ep.ExtensionAttributes)
                {
                    pointer.ExtensionAttributes[attr.AttributeKey] = JsonConvert.DeserializeObject(attr.AttributeValue, SerializerSettings);
                }

                result.ExecutionPointers.Add(pointer);
            }

            return result;
        }
        internal static Event ToEvent(this WkEvent instance)
        {
            Event result = new Event();
            result.Id = instance.Id.ToString();
            result.EventKey = instance.Key;
            result.EventName = instance.Name;
            result.EventTime = DateTime.SpecifyKind(instance.Time, DateTimeKind.Unspecified);
            result.IsProcessed = instance.IsProcessed;
            result.EventData = JsonConvert.DeserializeObject(instance.Data, SerializerSettings);
            return result;
        }
        internal static WkEvent ToPersistable(this Event instance)
        {
            return new WkEvent(
                    new Guid(instance.Id),
                    instance.EventName,
                    instance.EventKey,
                    JsonConvert.SerializeObject(instance.EventData, SerializerSettings),
                    DateTime.SpecifyKind(instance.EventTime, DateTimeKind.Unspecified),
                    instance.IsProcessed
                    );
        }
        internal static WkSubscription ToPersistable(this EventSubscription instance)
        {
            return new WkSubscription(
                new Guid(instance.Id),
                new Guid(instance.WorkflowId),
                instance.StepId,
                new Guid(instance.ExecutionPointerId),
                instance.EventName,
                instance.EventKey,
                DateTime.SpecifyKind(instance.SubscribeAsOf, DateTimeKind.Unspecified),
                JsonConvert.SerializeObject(instance.SubscriptionData, SerializerSettings),
                instance.ExternalToken,
                instance.ExternalWorkerId,
                instance.ExternalTokenExpiry
                );
        }
        internal static EventSubscription ToEventSubscription(this WkSubscription instance)
        {
            EventSubscription result = new EventSubscription();
            result.Id = instance.Id.ToString();
            result.EventKey = instance.EventKey;
            result.EventName = instance.EventName;
            result.StepId = instance.StepId;
            result.ExecutionPointerId = instance.ExecutionPointerId.ToString();
            result.WorkflowId = instance.WorkflowId.ToString();
            result.SubscribeAsOf = DateTime.SpecifyKind(instance.SubscribeAsOf, DateTimeKind.Unspecified);
            result.SubscriptionData = JsonConvert.DeserializeObject(instance.SubscriptionData, SerializerSettings);
            result.ExternalToken = instance.ExternalToken;
            result.ExternalTokenExpiry = instance.ExternalTokenExpiry;
            result.ExternalWorkerId = instance.ExternalWorkerId;
            return result;
        }
        internal static async Task<WkInstance> ToPersistable(this WorkflowInstance instance, WkInstance persistable = null)
        {
            var createTime = DateTime.SpecifyKind(instance.CreateTime, DateTimeKind.Unspecified);
            DateTime? completeTime = instance.CompleteTime.HasValue ? DateTime.SpecifyKind(instance.CompleteTime.Value, DateTimeKind.Unspecified) : null;
            if (persistable == null)
            {
                persistable = new WkInstance(
                    new Guid(instance.Id),
                    new Guid(instance.WorkflowDefinitionId),
                    instance.Version,
                    instance.Description,
                    instance.Reference,
                    instance.NextExecution,
                    instance.Status,
                    JsonConvert.SerializeObject(instance.Data, SerializerSettings),
                    createTime,
                    completeTime);
            }
            else
            {
                await persistable.SetVersion(instance.Version);
                await persistable.SetDescription(instance.Description);
                await persistable.SetReference(instance.Reference);
                await persistable.SetNextExecution(instance.NextExecution);
                await persistable.SetStatus(instance.Status);
                await persistable.SetData(JsonConvert.SerializeObject(instance.Data, SerializerSettings));
                await persistable.SetCreateTime(createTime);
                await persistable.SetCompleteTime(completeTime);
            }

            foreach (var exe in instance.ExecutionPointers)
            {
                DateTime? startTime = exe.StartTime.HasValue ? DateTime.SpecifyKind(exe.StartTime.Value, DateTimeKind.Unspecified) : null;
                DateTime? endTime = exe.EndTime.HasValue ? DateTime.SpecifyKind(exe.EndTime.Value, DateTimeKind.Unspecified) : null;
                var epTemp = persistable.ExecutionPointers.FirstOrDefault(d => d.Id.ToString() == exe.Id);
                if (epTemp == null)
                {
                    epTemp = new WkExecutionPointer(
                        exe.StepId,
                        exe.Active,
                        exe.SleepUntil,
                        JsonConvert.SerializeObject(exe.PersistenceData, SerializerSettings),
                        startTime,
                        endTime,
                        exe.EventName,
                        exe.EventKey,
                        exe.EventPublished,
                        JsonConvert.SerializeObject(exe.EventData, SerializerSettings),
                        exe.StepName,
                        exe.RetryCount,
                        string.Join(';', exe.Children),
                        JsonConvert.SerializeObject(exe.ContextItem, SerializerSettings),
                        exe.PredecessorId,
                        JsonConvert.SerializeObject(exe.Outcome, SerializerSettings),
                        exe.Status,
                        string.Join(';', exe.Scope));
                    await persistable.AddExecutionPointer(epTemp);
                }
                else
                {
                    await epTemp.SetStepId(exe.StepId);
                    await epTemp.SetActive(exe.Active);
                    await epTemp.SetSleepUntil(exe.SleepUntil);
                    await epTemp.SetPersistenceData(JsonConvert.SerializeObject(exe.PersistenceData, SerializerSettings));
                    await epTemp.SetStartTime(startTime);
                    await epTemp.SetEndTime(endTime);
                    await epTemp.SetEventName(exe.EventName);
                    await epTemp.SetEventKey(exe.EventKey);
                    await epTemp.SetEventPublished(exe.EventPublished);
                    await epTemp.SetEventData(JsonConvert.SerializeObject(exe.EventData, SerializerSettings));
                    await epTemp.SetStepName(exe.StepName);
                    await epTemp.SetRetryCount(exe.RetryCount);
                    await epTemp.SetChildren(string.Join(';', exe.Children));
                    await epTemp.SetContextItem(JsonConvert.SerializeObject(exe.ContextItem, SerializerSettings));
                    await epTemp.SetPredecessorId(exe.PredecessorId);
                    await epTemp.SetOutcome(JsonConvert.SerializeObject(exe.Outcome, SerializerSettings));
                    await epTemp.SetStatus(exe.Status);
                    await epTemp.SetScope(string.Join(';', exe.Scope));
                }
                foreach (var attr in exe.ExtensionAttributes)
                {
                    var persistedAttr = epTemp.ExtensionAttributes.FirstOrDefault(x => x.AttributeKey == attr.Key);
                    if (persistedAttr == null)
                    {
                        persistedAttr = new WkExtensionAttribute(

                            attr.Key,
                            JsonConvert.SerializeObject(attr.Value, SerializerSettings));
                    }
                    await persistedAttr.SetAttributeKey(attr.Key);
                    await persistedAttr.SetAttributeValue(JsonConvert.SerializeObject(attr.Value, SerializerSettings));
                    await epTemp.SetExtensionAttributes(persistedAttr);
                }
            }
            return persistable;
        }
        public static WkExecutionPointer ToWkExecutionPointer(this ExecutionPointer exe)
        {
            DateTime? startTime = exe.StartTime.HasValue ? DateTime.SpecifyKind(exe.StartTime.Value, DateTimeKind.Unspecified) : null;
            DateTime? endTime = exe.EndTime.HasValue ? DateTime.SpecifyKind(exe.EndTime.Value, DateTimeKind.Unspecified) : null;
            return new WkExecutionPointer(
                exe.StepId,
                exe.Active,
                exe.SleepUntil,
                JsonConvert.SerializeObject(exe.PersistenceData, SerializerSettings),
                startTime,
                endTime,
                exe.EventName,
                exe.EventKey,
                exe.EventPublished,
                JsonConvert.SerializeObject(exe.EventData, SerializerSettings),
                exe.StepName,
                exe.RetryCount,
                string.Join(';', exe.Children),
                JsonConvert.SerializeObject(exe.ContextItem, SerializerSettings),
                exe.PredecessorId,
                JsonConvert.SerializeObject(exe.Outcome, SerializerSettings),
                exe.Status,
                string.Join(';', exe.Scope));
        }
        public static ICollection<WkCandidate> ToCandidates(
            this ICollection<WkCandidate> entity,
            ICollection<WkCandidate> sourceEntitys)
        {
            foreach (var candidate in sourceEntitys)
            {
                WkCandidate updateCandidate = entity.FirstOrDefault(
                    d => d.CandidateId == candidate.CandidateId);
                if (updateCandidate != null)
                    continue;
                else
                {
                    updateCandidate = new WkCandidate(
                        candidate.CandidateId,
                        candidate.UserName,
                        candidate.DisplayUserName);
                    entity.Add(updateCandidate);
                }
                updateCandidate.SetSelection(true);
            }
            return entity;
        }
    }
}