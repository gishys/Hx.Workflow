using System;
using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyUpdateDto : ExtensibleObject
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public ICollection<WkStepBodyParamCreateDto>? Inputs { get; set; }
        public required string TypeFullName { get; set; }
        public required string AssemblyFullName { get; set; }
        public string? Data { get; set; }
    }
}
