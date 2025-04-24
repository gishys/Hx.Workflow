using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyDto : ExtensibleObject
    {
        public required string Id {  get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public required ICollection<WkStepBodyParamDto> Inputs { get; set; }
        public required string TypeFullName { get; set; }
        public required string AssemblyFullName { get; set; }
        public string? Data { get; set; }
    }
}
