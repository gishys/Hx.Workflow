using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
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
            var configuration = context.Services.GetConfiguration();
            Configure<HxWorkflowRuntimeOptions>(
                configuration.GetSection(HxWorkflowRuntimeOptions.SectionName));
        }
        public override async Task OnPostApplicationInitializationAsync(ApplicationInitializationContext context)
        {
            var options = context.ServiceProvider
                .GetRequiredService<IOptions<HxWorkflowRuntimeOptions>>()
                .Value;

            if (!options.RunHost)
            {
                return;
            }

            var manager = context.ServiceProvider.GetRequiredService<HxWorkflowManager>();
            await manager.Initialize();
            await manager.StartHostAsync();
        }
        public override async Task OnApplicationShutdownAsync(ApplicationShutdownContext context)
        {
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
