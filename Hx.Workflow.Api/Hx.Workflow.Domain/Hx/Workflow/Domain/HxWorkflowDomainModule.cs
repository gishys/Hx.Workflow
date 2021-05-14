using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Hx.Workflow.Domain
{
    [DependsOn(typeof(AbpDddDomainModule))]
    public class HxWorkflowDomainModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {

        }
        public async override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            var manager = context.ServiceProvider.GetRequiredService<HxWorkflowManager>();
            await manager.Initialize();
            await manager.StartHostAsync();
        }
        public async override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            base.OnApplicationShutdown(context);
            var manager = context.ServiceProvider.GetRequiredService<HxWorkflowManager>();
            await manager.StopAsync();
        }
    }
}