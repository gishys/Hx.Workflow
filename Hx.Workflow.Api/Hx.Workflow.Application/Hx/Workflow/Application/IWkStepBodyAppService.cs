using Hx.Workflow.Application.Contracts;
using System.Threading.Tasks;

namespace Hx.Workflow.Application
{
    public interface IWkStepBodyAppService
    {
        Task CreateAsync(WkSepBodyCreateDto input);
        Task DeleteAsync(string name);
        Task<WkStepBodyDto> GetStepBodyAsync(string name);
    }
}
