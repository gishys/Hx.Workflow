using AutoMapper;
using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Stats;
using Hx.Workflow.Domain.StepBodys;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Hx.Workflow.Application
{
    public class HxWorkflowAutoMapperProfile : Profile
    {
        public HxWorkflowAutoMapperProfile()
        {
            CreateMap<WkStepBody, WkStepBodyDto>(MemberList.Destination);
            CreateMap<WkStepBodyParam, WkStepBodyParamDto>(MemberList.Destination);
            CreateMap<WkDefinition, WkDefinitionDto>(MemberList.Destination);
            CreateMap<WkNode, WkNodeDto>(MemberList.Destination);
            CreateMap<WkNodePara, WkNodeParaDto>(MemberList.Destination);
            CreateMap<WkNodeMaterials, WkNodeMaterialsDto>(MemberList.Destination);
            CreateMap<WkNode_ApplicationForms, WkNode_ApplicationFormsDto>(MemberList.Destination);
            CreateMap<ApplicationForm, ApplicationFormDto>(MemberList.Destination);
            CreateMap<WkNodeRelation, WkNodeRelationDto>(MemberList.Destination);
            CreateMap<WkNodeRelationRule, WkNodeRelationRuleDto>(MemberList.Destination);
            CreateMap<ExePointerCandidate, WkCandidateDto>(MemberList.Destination);
            CreateMap<ExePointerCandidate, WkPointerCandidateDto>(MemberList.Destination);
            CreateMap<WkNodeCandidate, WkPointerCandidateDto>(MemberList.None);
            CreateMap<DefinitionCandidate, WkCandidateDto>(MemberList.Destination);
            CreateMap<WkNodeCandidate, WkCandidateDto>(MemberList.Destination);
            CreateMap<WkExecutionPointer, WkExecutionPointerDto>(MemberList.None)
                .ForMember(d => d.Status, d => d.MapFrom(f => (int)f.Status))
                .ForMember(d => d.ExtensionAttributes, d => d.MapFrom(f => ToDic(f.ExtensionAttributes)));
            CreateMap<WkExecutionError, WkExecutionErrorDto>(MemberList.Destination);
            CreateMap<WkParam, WkParamDto>(MemberList.Destination);
            CreateMap<WkExecutionPointerMaterials, WkExecutionPointerMaterialsDto>(MemberList.Destination);
            CreateMap<WkAuditor, WkAuditorDto>(MemberList.Destination);
            CreateMap<WkDefinitionGroup, WkDefinitionGroupDto>(MemberList.Destination);
            CreateMap<ProcessingStatusStat, ProcessingStatusStatDto>(MemberList.Destination);
            CreateMap<ApplicationFormGroup, ApplicationFormGroupDto>(MemberList.Destination);

            CreateMap<ProcessTypeStat, ProcessTypeStatDto>(MemberList.Destination);

            CreateMap<InstanceOverviewStat, InstanceOverviewStatDto>(MemberList.Destination);
            CreateMap<DurationStat, DurationStatDto>(MemberList.Destination);
            CreateMap<OverdueStat, OverdueStatDto>(MemberList.Destination);
            CreateMap<DefinitionStat, DefinitionStatDto>(MemberList.Destination);
            CreateMap<CreatorStat, CreatorStatDto>(MemberList.Destination);
            CreateMap<ErrorStat, ErrorStatDto>(MemberList.Destination);
            CreateMap<ErrorByStepStat, ErrorByStepStatDto>(MemberList.Destination);
            CreateMap<StepDurationStat, StepDurationStatDto>(MemberList.Destination);
            CreateMap<TrendStat, TrendStatDto>(MemberList.Destination);
        }
        public static IDictionary<string,object?> ToDic(ICollection<WkExtensionAttribute> source)
        {
            return source.ToDictionary(key => key.AttributeKey, v => JsonSerializer.Deserialize<object>(v.AttributeValue));
        }
    }
}