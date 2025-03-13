using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyDto : ExtensibleObject
    {
        public string Id {  get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ICollection<WkStepBodyParamDto> Inputs { get; set; }
        public string TypeFullName { get; set; }
        public string AssemblyFullName { get; set; }
        public string Data { get; set; }
    }
}
