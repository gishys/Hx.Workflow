using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Data;

namespace Hx.Workflow.Application.Contracts
{
    public class DefinitionNodeUpdateDto
    {
        public Guid Id { get; set; }
        public bool RecreateNodes { get; set; } = true;
        public required ExtraPropertyDictionary ExtraProperties { get; set; }
        public required ICollection<WkNodeCreateDto> Nodes { get; set; }
    }
}
