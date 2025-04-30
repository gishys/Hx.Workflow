using Hx.Workflow.Application.Contracts;
using Localization;
using Volo.Abp.Application.Services;

namespace Hx.Workflow.Application
{
    public class BaseAppService : ApplicationService, IWorkflowBaseAppService
    {
        protected BaseAppService()
        {
            LocalizationResource = typeof(WorkflowResource);
        }
        public string GetLocalization(string name)
        {
            return L[name];
        }
    }
}
