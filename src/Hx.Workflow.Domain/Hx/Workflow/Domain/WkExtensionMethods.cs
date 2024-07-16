using Hx.Workflow.Domain.BusinessModule;
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
            return new WkExecutionError(
                new Guid(instance.WorkflowId),
                new Guid(instance.ExecutionPointerId),
                instance.ErrorTime,
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
            result.CreateTime = instance.CreateTime;
            result.CompleteTime = instance.CompleteTime;
            result.ExecutionPointers = new ExecutionPointerCollection(instance.ExecutionPointers.Count + 8);

            foreach (var ep in instance.ExecutionPointers)
            {
                var pointer = new ExecutionPointer();

                pointer.Id = ep.Id.ToString();
                pointer.StepId = ep.StepId;
                pointer.Active = ep.Active;
                pointer.SleepUntil = ep.SleepUntil;

                pointer.PersistenceData = JsonConvert.DeserializeObject(ep.PersistenceData ?? string.Empty, SerializerSettings);
                pointer.StartTime = ep.StartTime;
                pointer.EndTime = ep.EndTime;
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
            result.EventTime = result.EventTime;
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
                    instance.EventTime,
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
                instance.SubscribeAsOf,
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
            result.SubscribeAsOf = result.SubscribeAsOf;
            result.SubscriptionData = JsonConvert.DeserializeObject(instance.SubscriptionData, SerializerSettings);
            result.ExternalToken = instance.ExternalToken;
            result.ExternalTokenExpiry = instance.ExternalTokenExpiry;
            result.ExternalWorkerId = instance.ExternalWorkerId;
            return result;
        }
        internal static async Task<WkInstance> ToPersistable(this WorkflowInstance instance, WkInstance persistable = null)
        {
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
                    instance.CreateTime,
                    instance.CompleteTime);
            }
            else
            {
                await persistable.SetVersion(instance.Version);
                await persistable.SetDescription(instance.Description);
                await persistable.SetNextExecution(instance.NextExecution);
                await persistable.SetStatus(instance.Status);
                await persistable.SetData(JsonConvert.SerializeObject(instance.Data, SerializerSettings));
                await persistable.SetCreateTime(instance.CreateTime);
                await persistable.SetCompleteTime(instance.CompleteTime);
            }

            foreach (var exe in instance.ExecutionPointers)
            {
                var eventPointerEventData = System.Text.Json.JsonSerializer.Deserialize<WkPointerEventData>(System.Text.Json.JsonSerializer.Serialize(exe.ExtensionAttributes));
                var epTemp = persistable.ExecutionPointers.FirstOrDefault(d => d.Id.ToString() == exe.Id);
                if (epTemp == null)
                {
                    epTemp = new WkExecutionPointer(
                        exe.StepId,
                        exe.Active,
                        exe.SleepUntil,
                        JsonConvert.SerializeObject(exe.PersistenceData, SerializerSettings),
                        exe.StartTime,
                        exe.EndTime,
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
                        string.Join(';', exe.Scope),
                        eventPointerEventData?.CommitmentDeadline);
                    await persistable.AddExecutionPointer(epTemp);
                }
                else
                {
                    await epTemp.SetStepId(exe.StepId);
                    await epTemp.SetActive(exe.Active);
                    await epTemp.SetSleepUntil(exe.SleepUntil);
                    await epTemp.SetPersistenceData(JsonConvert.SerializeObject(exe.PersistenceData, SerializerSettings));
                    await epTemp.SetStartTime(exe.StartTime);
                    await epTemp.SetEndTime(exe.EndTime);
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
                    var eventData = exe.EventData as ActivityResult;
                    await epTemp.SetCommitmentDeadline(eventPointerEventData?.CommitmentDeadline);
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
        public static ICollection<ExePointerCandidate> ToCandidates(this ICollection<WkNodeCandidate> nodes)
        {
            var candidates = new List<ExePointerCandidate>();
            foreach (var node in nodes)
            {
                candidates.Add(new ExePointerCandidate(
                    node.CandidateId,
                    node.UserName,
                    node.DisplayUserName,
                    node.DefaultSelection
                    ));
            }
            return candidates;
        }
    }
}