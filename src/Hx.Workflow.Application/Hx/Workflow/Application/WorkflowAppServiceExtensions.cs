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
        public static ICollection<WkNodeCandidate> ToWkCandidate(this ICollection<WkCandidateUpdateDto> inputs)
        {
            var resultEntity = new List<WkNodeCandidate>();
            foreach (var entity in inputs)
            {
                resultEntity.Add(new WkNodeCandidate(
                    entity.CandidateId,
                    entity.UserName,
                    entity.DisplayUserName,
                    entity.ExecutorType,
                    entity.DefaultSelection));
            }
            return resultEntity;
        }
        public static WkProcessInstanceDto ToProcessInstanceDto(this WkInstance instance)
        {
            var businessData = JsonSerializer.Deserialize<Dictionary<string, object>>(instance.Data);
            //如果节点不是已完成状态则获取当前节点（后续需要确定其他状态的含义）
            WkExecutionPointer? pointer = instance.ExecutionPointers.FirstOrDefault(d => d.Status != PointerStatus.Complete);
            var step = pointer != null ? instance.WkDefinition.Nodes.FirstOrDefault(d => d.Name == pointer.StepName) : null;
            bool isSign = instance.Status == WorkflowStatus.Runnable
                    && pointer != null
                    && pointer.RecipientId == null
                    && pointer.WkCandidates.Any(c => c.ParentState == ExeCandidateState.WaitingReceipt);
            var currentPointer = pointer != null ? new WkCurrentExecutionPointerDto()
            {
                Id = pointer.Id,
                Status = (int)pointer.Status,
            } : null;
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
                IsSign = !isSign,
                IsProcessed = instance.Status == WorkflowStatus.Runnable
                && pointer != null
                && (pointer.Active || pointer.WkSubscriptions.Count == 0 || pointer.Status == PointerStatus.Pending || pointer.Status == PointerStatus.Running),
                Data = businessData,
                CurrentPointer = currentPointer,
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
                foreach (var next in step.NextNodes)
                {
                    if (!instance.WkDefinition.Nodes.Any(d => d.Name == next.NextNodeName))
                        throw new UserFriendlyException(message: $"下一节点不在模板节点中【{next.NextNodeName}】!");
                    var nextNode = instance.WkDefinition.Nodes.First(d => d.Name == next.NextNodeName);
                    var isPreviousStep = nextNode.NextNodes.Any(n => n.NextNodeName == step.Name && n.NodeType == WkRoleNodeType.Forward);
                    currentPointerDto.NextPointers.Add(new WkNextPointerDto()
                    {
                        Selectable = true,
                        PreviousStep = isPreviousStep,
                        Label = instance.WkDefinition.Nodes.First(d => d.Name == next.NextNodeName).DisplayName,
                        NextNodeName = next.NextNodeName,
                        NodeType = next.NodeType,
                    });
                }
            }
            // 优先取当前用户自己的候选人记录（用于前端展示操作类型等信息）；
            // 若当前用户不在候选人列表中（如管理员旁观视角），则回退取第一条
            var currentCandidateInfo = (currentUserId.HasValue
                ? pointer.WkCandidates.FirstOrDefault(d => d.CandidateId == currentUserId.Value)
                : null)
                ?? pointer.WkCandidates.FirstOrDefault();
            if (currentCandidateInfo != null)
            {
                currentPointerDto.CurrentCandidateInfo = ObjectMapper.Map<ExePointerCandidate, WkPointerCandidateDto>(currentCandidateInfo);
            }

            // 仅主办（Host）和会签（Countersign）角色的候选人可以提交审批决定；
            // 抄送（CarbonCopy）、通知（Notify）、委托（Entrusted）角色为信息接收方，不具备办理权限。
            // 同时要求：流程运行中 + 节点等待人工事件 + 当前用户候选人状态为"待完成（Pending）"。
            static bool IsHandlableOperateType(ExePersonnelOperateType t) =>
                t == ExePersonnelOperateType.Host || t == ExePersonnelOperateType.Countersign;

            var canHandle =
                instance.Status == WorkflowStatus.Runnable
                && pointer.Status == PointerStatus.WaitingForEvent
                && currentUserId.HasValue
                && pointer.WkCandidates.Any(d =>
                    d.CandidateId == currentUserId.Value
                    && d.ParentState == ExeCandidateState.Pending
                    && IsHandlableOperateType(d.ExeOperateType));

            return new WkCurrentInstanceDetailsDto()
            {
                Id = instance.Id,
                Version = instance.Version,
                DefinitionId = instance.WkDefinition.Id,
                Reference = instance.Reference,
                Receiver = pointer.Recipient,
                ReceiveTime = pointer.StartTime,
                RegistrationCategory = instance.WkDefinition.BusinessType,
                CurrentExecutionPointer = currentPointerDto,
                BusinessType = instance.WkDefinition.BusinessType,
                ProcessType = instance.WkDefinition.ProcessType,
                WkAuditors = ObjectMapper.Map<ICollection<WkAuditor>, ICollection<WkAuditorDto>>(instance.WkAuditors),
                CanHandle = canHandle,
                Data = businessData,
            };
        }
    }
}
