using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Volo.Abp;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Hx.Workflow.Domain
{
    [DependsOn(typeof(AbpDddDomainModule))]
    [DependsOn(typeof(AbpAspNetCoreSignalRModule))]
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
        public override void PostConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.Configure<AbpSignalROptions>(options =>
            {
                var hubs = options.Hubs.DistinctBy(x => x.HubType).ToList();
                options.Hubs.Clear();
                options.Hubs.AddRange(hubs);
            });
        }
    }
}