using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWkStepBodyAppService
    {
        Task CreateAsync(WkSepBodyCreateDto input);
        Task DeleteAsync(string name);
        Task<WkStepBodyDto> GetStepBodyAsync(string name);
        Task<PagedResultDto<WkStepBodyDto>> GetPagedAsync(WkStepBodyPagedInput input);
    }
}
