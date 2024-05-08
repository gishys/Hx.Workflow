using Volo.Abp.Application.Services;

namespace Hx.Workflow.Application
{
    public class HxWorkflowAppServiceBase : ApplicationService
    {
        public HxWorkflowAppServiceBase()
        {
            ObjectMapperContext = typeof(HxWorkflowApplicationModule);
        }
    }
}
