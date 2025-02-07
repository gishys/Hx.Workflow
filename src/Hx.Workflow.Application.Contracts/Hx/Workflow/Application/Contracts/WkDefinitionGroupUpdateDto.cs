using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class WkDefinitionGroupUpdateDto : WkDefinitionGroupCreateOrUpdateDtoBase
    {
        public Guid Id { get; set; }
    }
}
