using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWkStepBodyAppService
    {
        Task CreateAsync(WkSepBodyCreateDto input);
        Task DeleteAsync(string name);
        Task<WkStepBodyDto> GetStepBodyAsync(string name);
    }
}
