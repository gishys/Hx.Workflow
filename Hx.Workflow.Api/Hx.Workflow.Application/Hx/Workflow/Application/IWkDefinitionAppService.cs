using Hx.Workflow.Application.Contracts;
using System.Threading.Tasks;

namespace Hx.Workflow.Application
{
    public interface IWkDefinitionAppService
    {
        Task CreateAsync(WkDefinitionCreateDto input);
    }
}
