using Hx.Workflow.Domain.Shared;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Hx.Workflow.Application.Contracts
{
    [DependsOn(typeof(HxWorkflowDomainSharedModule))]
    [DependsOn(typeof(AbpDddApplicationContractsModule))]
    public class HxWorkflowApplicationContractsModule : AbpModule
    {
    }
}
