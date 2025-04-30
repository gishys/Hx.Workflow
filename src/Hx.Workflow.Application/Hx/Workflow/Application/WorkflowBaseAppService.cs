using Localization;
using Volo.Abp.Application.Services;

namespace Hx.Workflow.Application
{
    public class WorkflowBaseAppService : ApplicationService
    {
        protected WorkflowBaseAppService()
        {
            LocalizationResource = typeof(WorkflowResource);
        }
        public string GetLocalization(string name)
        {
            return L[name];
        }
    }
}
