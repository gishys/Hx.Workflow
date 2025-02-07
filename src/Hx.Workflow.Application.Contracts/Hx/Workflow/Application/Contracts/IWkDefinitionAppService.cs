using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWkDefinitionAppService
    {
        Task CreateAsync(WkDefinitionCreateDto input);
    }
}
