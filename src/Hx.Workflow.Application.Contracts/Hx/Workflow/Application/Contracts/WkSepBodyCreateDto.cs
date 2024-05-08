using Hx.Workflow.Domain.Shared;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkSepBodyCreateDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ICollection<WkStepBodyParamCreateDto> Inputs { get; set; }
        public string TypeFullName { get; set; }
        public string AssemblyFullName { get; set; }
    }
}