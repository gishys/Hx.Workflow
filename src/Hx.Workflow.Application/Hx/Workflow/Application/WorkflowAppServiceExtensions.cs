using AutoMapper.Internal.Mappers;
using Hx.Workflow.Application.BusinessModule;
using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Volo.Abp.Users;
using WorkflowCore.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Hx.Workflow.Application
{
    public static class WorkflowAppServiceExtensions
    {
        public static List<WkNode> ToWkNodes(this ICollection<WkNodeCreateDto> nodes)
        {
            var nodeEntitys = new List<WkNode>();
            foreach (var node in nodes.OrderBy(d => d.SortNumber))
            {
                var nodeEntity = new WkNode(
                        node.Name,
                        node.DisplayName,
                        node.StepNodeType,
                        node.Version,
                        node.NodeFormType,
                        node.SortNumber,
                        node.LimitTime);
                if (node.NextNodes?.Count > 0)
                {
                    foreach (var condition in node.NextNodes)
                    {
                        if (condition.NextNodeName.Length > 0)
                        {
                            var conditionEntity = new WkConditionNode(condition.NextNodeName, condition.NodeType);
                            if (condition.WkConNodeConditions?.Count > 0)
                            {
                                foreach (var conCondition in condition.WkConNodeConditions)
                                {
                                    conditionEntity.AddConNodeCondition(
                                        new WkConNodeCondition(
                                            conCondition.Field,
                                            conCondition.Operator,
                                            conCondition.Value
                                            ));
                                }
                            }
                            nodeEntity.AddNextNode(conditionEntity);
                        }
                    }
                }
                if (node.Position?.Count > 0)
                {
                    foreach (var point in node.Position)
                    {
                        nodeEntity.AddPoint(
                            new WkPoint(
                                point.Left,
                                point.Right,
                                point.Top,
                                point.Bottom));
                    }
                }
                if (node.OutcomeSteps?.Count > 0)
                {
                    foreach (var outcomp in node.OutcomeSteps)
                    {
                        nodeEntity.AddOutcomeSteps(
                            new WkNodePara(
                                outcomp.Key,
                                outcomp.Value));
                    }
                }
                if (node.WkCandidates?.Count > 0)
                {
                    foreach (var candidate in node.WkCandidates)
                    {
                        nodeEntity.AddCandidates(new WkNodeCandidate(
                            candidate.CandidateId,
                            candidate.UserName,
                            candidate.DisplayUserName,
                            candidate.ExecutorType,
                            candidate.DefaultSelection));
                    }
                }
                if (node.ApplicationForms?.Count > 0)
                {
                    foreach (var appForm in node.ApplicationForms)
                    {
                        var form = new ApplicationForm(
                            appForm.ParentId,
                            appForm.Code,
                            appForm.Name,
                            appForm.DisplayName,
                            appForm.ApplicationType,
                            appForm.SequenceNumber);
                        if (appForm.Params?.Count > 0)
                        {
                            foreach (var param in appForm.Params)
                            {
                                form.AddParam(new WkParam(param.WkParamKey, param.Name, param.DisplayName, param.Value));
                            }
                        }
                        nodeEntity.AddApplicationForms(form);
                    }
                }
                if (node.Params?.Count > 0)
                {
                    foreach (var param in node.Params)
                    {
                        nodeEntity.AddParam(new WkParam(param.WkParamKey, param.Name, param.DisplayName, param.Value));
                    }
                }
                nodeEntitys.Add(nodeEntity);
            }
            return nodeEntitys;
        }
        public static ICollection<Candidate> ToWkCandidate(this ICollection<WkCandidateUpdateDto> inputs)
        {
            var resultEntity = new List<Candidate>();
            foreach (var entity in inputs)
            {
                resultEntity.Add(new Candidate(
                    entity.CandidateId,
                    entity.UserName,
                    entity.DisplayUserName,
                    entity.ExecutorType,
                    entity.DefaultSelection));
            }
            return resultEntity;
        }
        public static WkProcessInstanceDto ToProcessInstanceDto(this WkInstance instance, ICollection<Guid> userIds)
        {
            var businessData = JsonSerializer.Deserialize<WkInstanceEventData>(instance.Data);
            //如果节点不是已完成状态则获取当前节点（后续需要确定其他状态的含义）
            WkExecutionPointer pointer = instance.ExecutionPointers.FirstOrDefault(d => d.Status != PointerStatus.Complete);
            var step = pointer != null ? instance.WkDefinition.Nodes.First(d => d.Name == pointer.StepName) : null;
            var processInstance = new WkProcessInstanceDto
            {
                Id = instance.Id,
                EarlyWarning = GetEarlyWarning(businessData, instance),
                Reference = instance.Reference,
                ProcessName = businessData.ProcessName,
                Located = businessData.Located,
                ProcessingStepName = step?.DisplayName,
                Recipient = pointer?.Recipient,
                Submitter = pointer?.Submitter,
                ReceivingTime = instance.CreateTime,
                State = instance.Status.ToString(),
                BusinessType = instance.WkDefinition.BusinessType,
                BusinessCommitmentDeadline = businessData.BusinessCommitmentDeadline,
                ProcessType = instance.WkDefinition.ProcessType,
                IsSign = instance.Status != WorkflowStatus.Runnable || (pointer != null ? userIds.Any(id => id == pointer.RecipientId) : true),
                IsProcessed = pointer != null ? pointer.WkSubscriptions.Any(d => d.ExternalToken != null) : false,
            };
            if (pointer == null)
            {
                //开始节点接收人（创建人）
                processInstance.Recipient = instance.ExecutionPointers.FirstOrDefault(d => d.PredecessorId == null)?.Recipient;
                //最后一个节点的提交人
                processInstance.Submitter = instance.ExecutionPointers.LastOrDefault()?.Submitter;
            }
            return processInstance;
        }
        public static WkCurrentInstanceDetailsDto ToWkInstanceDetailsDto(
            this WkInstance instance,
            Volo.Abp.ObjectMapping.IObjectMapper ObjectMapper,
            WkInstanceEventData businessData,
            WkExecutionPointer pointer,
            Guid? currentUserId,
            List<WkExecutionError> errors
            )
        {
            var step = instance.WkDefinition.Nodes.First(d => d.Name == pointer.StepName);
            var currentPointerDto = ObjectMapper.Map<WkExecutionPointer, WkExecutionPointerDto>(pointer);
            currentPointerDto.Forms = ObjectMapper.Map<ICollection<ApplicationForm>, ICollection<ApplicationFormDto>>(step.ApplicationForms.OrderBy(d => d.SequenceNumber).ToList());
            currentPointerDto.StepDisplayName = step.DisplayName;
            currentPointerDto.Errors = ObjectMapper.Map<List<WkExecutionError>, List<WkExecutionErrorDto>>(errors);
            currentPointerDto.Params = ObjectMapper.Map<List<WkParam>, List<WkParamDto>>(step.Params.ToList());
            currentPointerDto.NextPointers = [];
            WkExecutionPointer prePointer = null;
            if (!string.IsNullOrEmpty(pointer.PredecessorId))
            {
                prePointer = instance.ExecutionPointers.First(d => d.Id.ToString() == pointer.PredecessorId);
            }
            foreach (var next in step.NextNodes)
            {
                currentPointerDto.NextPointers.Add(new WkNextPointerDto()
                {
                    Selectable = true,
                    PreviousStep = prePointer != null && prePointer.StepName == next.NextNodeName,
                    Label = instance.WkDefinition.Nodes.First(d => d.Name == next.NextNodeName).DisplayName,
                    NextNodeName = next.NextNodeName,
                    NodeType = next.NodeType,
                });
            }
            if (currentUserId.HasValue)
            {
                var currentCandidateInfo = pointer.WkCandidates.First(d => d.CandidateId == currentUserId.Value);
                currentPointerDto.CurrentCandidateInfo = ObjectMapper.Map<ExePointerCandidate, WkPointerCandidateDto>(currentCandidateInfo);
            }
            return new WkCurrentInstanceDetailsDto()
            {
                Id = instance.Id,
                DefinitionId = instance.WkDefinition.Id,
                Reference = instance.Reference,
                Receiver = pointer.Recipient,
                ReceiveTime = pointer.StartTime,
                RegistrationCategory = instance.WkDefinition.BusinessType,
                BusinessCommitmentDeadline = businessData.BusinessCommitmentDeadline,
                CurrentExecutionPointer = currentPointerDto,
                ProcessName = businessData.ProcessName,
                Located = businessData.Located,
                CanHandle = instance.Status == WorkflowStatus.Runnable &&
                pointer.Status != PointerStatus.Complete
                && pointer.WkSubscriptions.Any(d => d.ExternalToken == null)
                && currentUserId.HasValue && pointer.WkCandidates.Any(d => d.CandidateId == currentUserId.Value),
            };
        }
        private static string GetEarlyWarning(WkInstanceEventData businessData, WkInstance instance)
        {
            var earlyWarning = "green";
            if (instance.Status == WorkflowStatus.Runnable)
            {
                if (businessData.BusinessCommitmentDeadline <= DateTime.Now)
                {
                    earlyWarning = "red";
                }
                else if (businessData.BusinessCommitmentDeadline.AddHours(2) <= DateTime.Now)
                {
                    earlyWarning = "yellow";
                }
            }
            return earlyWarning;
        }
    }
}