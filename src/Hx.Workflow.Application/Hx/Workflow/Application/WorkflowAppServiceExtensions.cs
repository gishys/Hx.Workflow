using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.StepBodys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Guids;
using WorkflowCore.Models;

namespace Hx.Workflow.Application
{
    public static class WorkflowAppServiceExtensions
    {
        public static List<WkNode> ToWkNodes(this ICollection<WkNodeCreateDto> nodes, IGuidGenerator GuidGenerator)
        {
            var nodeEntitys = new List<WkNode>();
            int count = 0;
            foreach (var node in nodes)
            {
                var nodeEntity = new WkNode(
                        node.Name,
                        node.DisplayName,
                        node.StepNodeType,
                        node.Version,
                        count++,
                        node.LimitTime,
                        node.Id == null ? GuidGenerator.Create() : node.Id);
                if (node.NextNodes?.Count > 0)
                {
                    foreach (var condition in node.NextNodes)
                    {
                        var conditionEntity = new WkNodeRelation(condition.NextNodeName, condition.NodeType);
                        if (condition.Rules?.Count > 0)
                        {
                            foreach (var conCondition in condition.Rules)
                            {
                                conditionEntity.AddConNodeCondition(
                                    new WkNodeRelationRule(
                                        conCondition.Field,
                                        conCondition.Operator,
                                        conCondition.Value
                                        ));
                            }
                        }
                        nodeEntity.AddNextNode(conditionEntity);
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
                if (node.Materials?.Count > 0)
                {
                    foreach (var ma in node.Materials)
                    {
                        nodeEntity.AddMaterails(new WkNodeMaterials(
                            ma.AttachReceiveType,
                            ma.ReferenceType,
                            ma.CatalogueName,
                            ma.SequenceNumber,
                            ma.IsRequired,
                            ma.IsStatic,
                            ma.IsVerification,
                            ma.VerificationPassed));
                    }
                }
                if (node.ApplicationForms?.Count > 0)
                {
                    foreach (var form in node.ApplicationForms)
                    {
                        var ps = new List<WkParam>();
                        if (form.Params?.Count > 0)
                        {
                            foreach (var param in form.Params)
                            {
                                ps.Add(new WkParam(param.WkParamKey, param.Name, param.DisplayName, param.Value));
                            }
                        }
                        nodeEntity.AddApplicationForms(form.ApplicationFormId, form.SequenceNumber, ps);
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
            var businessData = JsonSerializer.Deserialize<Dictionary<string, object>>(instance.Data);
            //如果节点不是已完成状态则获取当前节点（后续需要确定其他状态的含义）
            WkExecutionPointer? pointer = instance.ExecutionPointers.FirstOrDefault(d => d.Status != PointerStatus.Complete);
            var step = pointer != null ? instance.WkDefinition.Nodes.FirstOrDefault(d => d.Name == pointer.StepName) : null;
            var processInstance = new WkProcessInstanceDto
            {
                Id = instance.Id,
                Reference = instance.Reference,
                ProcessingStepName = step?.DisplayName,
                Recipient = pointer?.Recipient,
                Submitter = pointer?.Submitter,
                ReceivingTime = instance.CreateTime,
                State = instance.Status.ToString(),
                ProcessType = instance.WkDefinition.ProcessType,
                IsSign =
                !(instance.Status == WorkflowStatus.Runnable
                && pointer != null
                && pointer.RecipientId == null
                && pointer.WkCandidates.Any(c => userIds.Any(u => u == c.CandidateId && c.ParentState == ExeCandidateState.WaitingReceipt))),
                IsProcessed = !(pointer != null && (pointer.Status == PointerStatus.WaitingForEvent || pointer.Status == PointerStatus.Failed)),
                Data = businessData
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
            Dictionary<string, object>? businessData,
            WkExecutionPointer pointer,
            Guid? currentUserId,
            List<WkExecutionError> errors
            )
        {
            var step = instance.WkDefinition.Nodes.First(d => d.Name == pointer.StepName);
            var currentPointerDto = ObjectMapper.Map<WkExecutionPointer, WkExecutionPointerDto>(pointer);
            var forms = new List<ApplicationFormDto>();
            foreach (var form in step.ApplicationForms.OrderBy(d => d.SequenceNumber))
            {
                var f = ObjectMapper.Map<ApplicationForm, ApplicationFormDto>(form.ApplicationForm);
                f.Params = ObjectMapper.Map<ICollection<WkParam>, ICollection<WkParamDto>>(form.Params);
                forms.Add(f);
            }
            currentPointerDto.Forms = forms;
            currentPointerDto.StepDisplayName = step.DisplayName;
            currentPointerDto.Errors = ObjectMapper.Map<List<WkExecutionError>, List<WkExecutionErrorDto>>(errors);
            currentPointerDto.Params = ObjectMapper.Map<List<WkParam>, List<WkParamDto>>([.. step.Params]);
            currentPointerDto.NextPointers = [];
            if (instance.Status != WorkflowStatus.Complete)
            {
                WkExecutionPointer? prePointer = null;
                if (!string.IsNullOrEmpty(pointer.PredecessorId))
                {
                    prePointer = instance.ExecutionPointers.First(d => d.Id.ToString() == pointer.PredecessorId);
                }
                foreach (var next in step.NextNodes)
                {
                    if (!instance.WkDefinition.Nodes.Any(d => d.Name == next.NextNodeName))
                        throw new UserFriendlyException(message: $"下一节点不在模板节点中【{next.NextNodeName}】!");
                    currentPointerDto.NextPointers.Add(new WkNextPointerDto()
                    {
                        Selectable = true,
                        PreviousStep = prePointer != null && prePointer.StepName == next.NextNodeName,
                        Label = instance.WkDefinition.Nodes.First(d => d.Name == next.NextNodeName).DisplayName,
                        NextNodeName = next.NextNodeName,
                        NodeType = next.NodeType,
                    });
                }
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
                CurrentExecutionPointer = currentPointerDto,
                BusinessType = instance.WkDefinition.BusinessType,
                ProcessType = instance.WkDefinition.ProcessType,
                WkAuditors = ObjectMapper.Map<ICollection<WkAuditor>, ICollection<WkAuditorDto>>(instance.WkAuditors),
                CanHandle =
                instance.Status == WorkflowStatus.Runnable
                && pointer.Status != PointerStatus.Complete
                && pointer.WkSubscriptions.Any(d => d.ExternalToken == null)
                && currentUserId.HasValue && pointer.WkCandidates.Any(d => d.CandidateId == currentUserId.Value),
                Data = businessData,
            };
        }
    }
}