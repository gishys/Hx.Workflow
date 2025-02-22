﻿using Hx.Workflow.Domain.Shared;
using System.Collections.Generic;
using Volo.Abp.ObjectExtending;

namespace Hx.Workflow.Application.Contracts
{
    public class WkSepBodyCreateDto : ExtensibleObject
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ICollection<WkStepBodyParamCreateDto> Inputs { get; set; }
        public string TypeFullName { get; set; }
        public string AssemblyFullName { get; set; }
        public string Data {  get; set; }
    }
}