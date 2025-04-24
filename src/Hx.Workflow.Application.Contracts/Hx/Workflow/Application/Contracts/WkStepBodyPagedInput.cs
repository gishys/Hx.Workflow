using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application.Contracts
{
    public class WkStepBodyPagedInput : PagedResultRequestDto
    {
        public string? Filter { get; set; }
    }
}
