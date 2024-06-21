using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Hx.Workflow.Application.Contracts
{
    [DependsOn(typeof(AbpDddApplicationContractsModule))]
    public class HxWorkflowApplicationContractsModule : AbpModule
    {
    }
}
