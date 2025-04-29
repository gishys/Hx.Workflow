using Volo.Abp.Application.Services;

namespace Hx.Workflow.Application.Contracts
{
    public interface IBaseAppService : IApplicationService
    {
        string GetLocalization(string name);
    }
}
