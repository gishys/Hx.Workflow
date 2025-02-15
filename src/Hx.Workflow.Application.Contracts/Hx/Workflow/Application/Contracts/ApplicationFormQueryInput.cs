using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application.Contracts
{
    public class ApplicationFormQueryInput : PagedResultRequestDto
    {
        public string? Filter {  get; set; }
    }
}
