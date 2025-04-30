using Volo.Abp.Application.Services;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWorkflowBaseAppService : IApplicationService
    {
        string GetLocalization(string name);
    }
}
