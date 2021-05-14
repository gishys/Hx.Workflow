using Volo.Abp.Application.Services;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWorkflowManagerAppService : IApplicationService
    {
        void Create();
    }
}
