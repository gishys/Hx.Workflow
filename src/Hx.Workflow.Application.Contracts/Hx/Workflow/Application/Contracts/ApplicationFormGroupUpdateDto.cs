using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public class ApplicationFormGroupUpdateDto : ApplicationFormGroupCreateOrUpdateDtoBase
    {
        public Guid Id { get; set; }
    }
}
