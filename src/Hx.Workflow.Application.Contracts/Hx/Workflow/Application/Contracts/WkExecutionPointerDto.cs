using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkExecutionPointerDto
    {
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
        public ICollection<ApplicationFormDto> Forms { get; set; }
        public ICollection<WkExecutionErrorDto> Errors { get; set; }
    }
}
