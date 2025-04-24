using Hx.Workflow.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Hx.Workflow.HttpApi
{
    [DependsOn(typeof(AbpAspNetCoreMvcModule))]
    [DependsOn(typeof(HxWorkflowApplicationContractsModule))]
    public class HxWorkflowHttpApiModule : AbpModule
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            PreConfigure(delegate (IMvcBuilder mvcBuilder)
            {
                mvcBuilder.AddApplicationPartIfNotExists(typeof(HxWorkflowHttpApiModule).Assembly);
            });
        }
    }
}
