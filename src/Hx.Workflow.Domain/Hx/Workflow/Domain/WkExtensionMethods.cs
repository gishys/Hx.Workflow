using Hx.Workflow.Domain.BusinessModule;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using Newtonsoft.Json;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain
{
    public static class WkExtensionMethods
    {
        private static readonly JsonSerializerSettings SerializerSettings = new() { TypeNameHandling = TypeNameHandling.All };
        internal static WkExecutionError ToPersistable(this ExecutionError instance)
        {
            return new WkExecutionError(
                new Guid(instance.WorkflowId),
                new Guid(instance.ExecutionPointerId),
                WorkflowDateTime.NormalizeUtc(instance.ErrorTime),
                instance.Message);
        }
        internal static WorkflowInstance ToWorkflowInstance(this WkInstance instance)
        {
            WorkflowInstance result = new()
            {
                Data = JsonConvert.DeserializeObject(instance.Data, SerializerSettings),
                Description = instance.Description,
                Reference = instance.Reference,
                Id = instance.Id.ToString(),
                NextExecution = instance.NextExecution,
                Version = instance.Version,
                WorkflowDefinitionId = instance.WkDifinitionId.ToString(),
                Status = instance.Status,
                CreateTime = WorkflowDateTime.NormalizeUtc(instance.CreateTime),
                CompleteTime = WorkflowDateTime.NormalizeUtc(instance.CompleteTime),
                ExecutionPointers = new ExecutionPointerCollection(instance.ExecutionPointers.Count + 8)
            };

            foreach (var ep in instance.ExecutionPointers)
            {
                var pointer = new ExecutionPointer
                {
                    Id = ep.Id.ToString(),
                    StepId = ep.StepId,
                    Active = ep.Active,
                    SleepUntil = WorkflowDateTime.NormalizeUtc(ep.SleepUntil),

                    PersistenceData = ep.PersistenceData.SafeDeserialize<Dictionary<string, object>>(SerializerSettings),
                    StartTime = WorkflowDateTime.NormalizeUtc(ep.StartTime),
                    EndTime = WorkflowDateTime.NormalizeUtc(ep.EndTime),
                    StepName = ep.StepName,

                    RetryCount = ep.RetryCount,
                    PredecessorId = ep.PredecessorId,
                    ContextItem = JsonConvert.DeserializeObject(ep.ContextItem ?? string.Empty, SerializerSettings)
                };

                if (!string.IsNullOrEmpty(ep.Children))
                    pointer.Children = [.. ep.Children.Split(';', StringSplitOptions.RemoveEmptyEntries)];

                pointer.EventName = ep.EventName;
                pointer.EventKey = ep.EventKey;
                pointer.EventPublished = ep.EventPublished;
                pointer.EventData = JsonConvert.DeserializeObject(ep.EventData ?? string.Empty, SerializerSettings);
                pointer.Outcome = JsonConvert.DeserializeObject(ep.Outcome ?? string.Empty, SerializerSettings);
                pointer.Status = ep.Status;

                if (!string.IsNullOrEmpty(ep.Scope))
                {
                    pointer.Scope = new List<string>(ep.Scope.Split(';', StringSplitOptions.RemoveEmptyEntries));
                }

                if (ep.ExtensionAttributes != null)
                {
                    foreach (var attr in ep.ExtensionAttributes)
                    {
                        pointer.ExtensionAttributes[attr.AttributeKey] = JsonConvert.DeserializeObject(attr.AttributeValue, SerializerSettings);
                    }
                }

                result.ExecutionPointers.Add(pointer);
            }

            return result;
        }
        internal static Event ToEvent(this WkEvent instance)
        {
            Event result = new()
            {
                Id = instance.Id.ToString(),
                EventKey = instance.Key,
                EventName = instance.Name
            };
            result.EventTime = WorkflowDateTime.NormalizeUtc(instance.Time);
            result.IsProcessed = instance.IsProcessed;
            result.EventData = JsonConvert.DeserializeObject(instance.Data, SerializerSettings);
            return result;
        }
        internal static WkEvent ToPersistable(this Event instance)
        {
            var eventData = instance.EventData;
            if (eventData is ActivityResult activityResult)
            {
                eventData = new ActivityResult
                {
                    Status = activityResult.Status,
                    SubscriptionId = activityResult.SubscriptionId,
                    Data = NormalizeJsonValue(activityResult.Data)
                };
            }

            return new WkEvent(
                    new Guid(instance.Id),
                    instance.EventName,
                    instance.EventKey,
                    JsonConvert.SerializeObject(eventData, SerializerSettings),
                    WorkflowDateTime.NormalizeUtc(instance.EventTime),
                    instance.IsProcessed
                    );
        }

        private static object? NormalizeJsonValue(object? value)
        {
            if (value is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.Object => element.EnumerateObject()
                        .ToDictionary(property => property.Name, property => NormalizeJsonValue(property.Value)),
                    JsonValueKind.Array => element.EnumerateArray()
                        .Select(item => NormalizeJsonValue(item)).ToList(),
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number when element.TryGetInt64(out var integer) => integer,
                    JsonValueKind.Number when element.TryGetDecimal(out var number) => number,
                    JsonValueKind.Number => element.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null or JsonValueKind.Undefined => null,
                    _ => element.GetRawText()
                };
            }

            if (value is IDictionary<string, object> dictionary)
            {
                return dictionary.ToDictionary(
                    item => item.Key,
                    item => NormalizeJsonValue(item.Value));
            }

            if (value is IEnumerable<object> collection && value is not string)
            {
                return collection.Select(NormalizeJsonValue).ToList();
            }

            return value;
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
                WorkflowDateTime.NormalizeUtc(instance.SubscribeAsOf),
                JsonConvert.SerializeObject(instance.SubscriptionData, SerializerSettings),
                instance.ExternalToken,
                instance.ExternalWorkerId,
                WorkflowDateTime.NormalizeUtc(instance.ExternalTokenExpiry)
                );
        }
        internal static EventSubscription ToEventSubscription(this WkSubscription instance)
        {
            EventSubscription result = new()
            {
                Id = instance.Id.ToString(),
                EventKey = instance.EventKey,
                EventName = instance.EventName,
                StepId = instance.StepId,
                ExecutionPointerId = instance.ExecutionPointerId.ToString(),
                WorkflowId = instance.WorkflowId.ToString()
            };
            result.SubscribeAsOf = WorkflowDateTime.NormalizeUtc(instance.SubscribeAsOf);
            result.SubscriptionData = JsonConvert.DeserializeObject(instance.SubscriptionData ?? string.Empty, SerializerSettings);
            result.ExternalToken = instance.ExternalToken;
            result.ExternalTokenExpiry = WorkflowDateTime.NormalizeUtc(instance.ExternalTokenExpiry);
            result.ExternalWorkerId = instance.ExternalWorkerId;
            return result;
        }
        internal static async Task<WkInstance> ToPersistable(this WorkflowInstance instance, WkInstance? persistable = null)
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
                    WorkflowDateTime.NormalizeUtc(instance.CreateTime),
                    WorkflowDateTime.NormalizeUtc(instance.CompleteTime));
            }
            else
            {
                await persistable.SetVersion(instance.Version);
                await persistable.SetDescription(instance.Description);
                await persistable.SetNextExecution(instance.NextExecution);
                await persistable.SetStatus(instance.Status);
                await persistable.SetData(JsonConvert.SerializeObject(instance.Data, SerializerSettings));
                await persistable.SetCreateTime(WorkflowDateTime.NormalizeUtc(instance.CreateTime));
                await persistable.SetCompleteTime(WorkflowDateTime.NormalizeUtc(instance.CompleteTime));
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
                        WorkflowDateTime.NormalizeUtc(exe.SleepUntil),
                        JsonConvert.SerializeObject(exe.PersistenceData, SerializerSettings),
                        WorkflowDateTime.NormalizeUtc(exe.StartTime),
                        WorkflowDateTime.NormalizeUtc(exe.EndTime),
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
                        WorkflowDateTime.NormalizeUtc(eventPointerEventData?.CommitmentDeadline));
                    await persistable.AddExecutionPointer(epTemp);
                }
                else
                {
                    await epTemp.SetStepId(exe.StepId);
                    await epTemp.SetActive(exe.Active);
                    await epTemp.SetSleepUntil(WorkflowDateTime.NormalizeUtc(exe.SleepUntil));
                    await epTemp.SetPersistenceData(exe.PersistenceData != null
                        ? JsonConvert.SerializeObject(exe.PersistenceData, SerializerSettings)
                        : null);
                    await epTemp.SetStartTime(WorkflowDateTime.NormalizeUtc(exe.StartTime));
                    await epTemp.SetEndTime(WorkflowDateTime.NormalizeUtc(exe.EndTime));
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
                    await epTemp.SetCommitmentDeadline(
                        WorkflowDateTime.NormalizeUtc(eventPointerEventData?.CommitmentDeadline));
                }
                if (exe.ExtensionAttributes != null)
                {
                    foreach (var attr in exe.ExtensionAttributes)
                    {
                        var persistedAttr = epTemp.ExtensionAttributes.FirstOrDefault(x => x.AttributeKey == attr.Key);
                        persistedAttr ??= new WkExtensionAttribute(
                                attr.Key,
                                JsonConvert.SerializeObject(attr.Value, SerializerSettings)
                                );
                        await persistedAttr.SetAttributeKey(attr.Key);
                        await persistedAttr.SetAttributeValue(JsonConvert.SerializeObject(attr.Value, SerializerSettings));
                        await epTemp.SetExtensionAttributes(persistedAttr);
                    }
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
                    ExePersonnelOperateType.Host,
                    ExeCandidateState.WaitingReceipt,
                    node.ExecutorType,
                    node.DefaultSelection));
            }
            return candidates;
        }
    }
}
