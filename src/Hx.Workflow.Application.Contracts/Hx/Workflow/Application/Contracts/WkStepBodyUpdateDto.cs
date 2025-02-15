using System;
using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyUpdateDto : ExtensibleObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ICollection<WkStepBodyParamCreateDto> Inputs { get; set; }
        public string TypeFullName { get; set; }
        public string AssemblyFullName { get; set; }
        public string Data { get; set; }
    }
}
