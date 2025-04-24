using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkExecutionPointerDto
    {
        public Guid Id { get; set; }
        public required string StepName { get; set; }
        public required string StepDisplayName { get; set; }
        public int Status { get; set; }
        public int StepId { get; set; }
        public bool Active { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Recipient { get; set; }
        public Guid RecipientId { get; set; }
        public string? Submitter { get; set; }
        public Guid? SubmitterId { get; set; }
        public bool? IsInitMaterials { get; set; }
        public required ICollection<WkParamDto> Params { get; set; }
        public required ICollection<ApplicationFormDto> Forms { get; set; }
        public required ICollection<WkExecutionErrorDto> Errors { get; set; }
        public required WkPointerCandidateDto CurrentCandidateInfo { get; set; }
        public required ICollection<WkNextPointerDto> NextPointers { get; set; }
        public required ICollection<WkExecutionPointerMaterialsDto> Materials { get; set; }
        public required IDictionary<string,object?> ExtensionAttributes {  get; set; }
    }
}
