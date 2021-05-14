using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ICollection<WkStepBodyParamDto> Inputs { get; set; }
        public string TypeFullName { get; set; }
        public string AssemblyFullName { get; set; }
    }
}
