﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkExecutionPointerDto
    {
        public Guid Id { get; set; }
        public string StepName { get; set; }
        public string StepDisplayName { get; set; }
        public int Status { get; set; }
        public int StepId { get; set; }
        public bool Active { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Recipient { get; set; }
        public Guid RecipientId { get; set; }
        public string Submitter { get; set; }
        public Guid? SubmitterId { get; set; }
        public bool? IsInitMaterials { get; set; }
        public ICollection<WkParamDto> Params { get; set; }
        public ICollection<ApplicationFormDto> Forms { get; set; }
        public ICollection<WkExecutionErrorDto> Errors { get; set; }
        public WkPointerCandidateDto CurrentCandidateInfo { get; set; }
        public ICollection<WkNextPointerDto> NextPointers { get; set; }
        public ICollection<WkExecutionPointerMaterialsDto> Materials { get; set; }
        public IDictionary<string,object> ExtensionAttributes {  get; set; }
    }
}
