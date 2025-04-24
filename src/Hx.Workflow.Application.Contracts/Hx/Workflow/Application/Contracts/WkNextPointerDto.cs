using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkNextPointerDto
    {
        public bool Selectable {  get; set; }
        public string? Label { get; set; }
        public required string NextNodeName { get; set; }
        public WkRoleNodeType NodeType { get; set; }
        public bool PreviousStep {  get; set; }
    }
}
