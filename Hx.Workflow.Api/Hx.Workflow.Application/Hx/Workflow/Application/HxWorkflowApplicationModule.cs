using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Hx.Workflow.Application
{
    [DependsOn(typeof(HxWorkflowDomainModule))]
    [DependsOn(typeof(AbpAutoMapperModule))]
    [DependsOn(typeof(HxWorkflowApplicationContractsModule))]
    public class HxWorkflowApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAutoMapperObjectMapper<HxWorkflowApplicationModule>();
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddProfile<HxWorkflowAutoMapperProfile>(validate: true);
            });
        }
    }
}
