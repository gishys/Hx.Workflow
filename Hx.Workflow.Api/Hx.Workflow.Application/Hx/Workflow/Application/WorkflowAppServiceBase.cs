using Volo.Abp.Application.Services;

namespace Hx.Workflow.Application
{
    public class WorkflowAppServiceBase : ApplicationService
    {
        public WorkflowAppServiceBase()
        {
            ObjectMapperContext = typeof(HxWorkflowApplicationModule);
        }
    }
}
