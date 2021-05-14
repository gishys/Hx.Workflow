using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.StepBodys;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace Hx.Workflow.EntityFrameworkCore
{
    [DependsOn(typeof(HxWorkflowDomainModule))]
    [DependsOn(typeof(AbpEntityFrameworkCoreModule))]
    public class HxEntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<WkDbContext>(
                options =>
                {
                    options.AddDefaultRepositories<WkDbContext>();
                    options.AddRepository<WkDefinition, WkDefinitionRespository>();
                    options.AddRepository<WkStepBody, WkStepBodyRespository>();
                    options.AddRepository<WkAuditor, WkAuditorRespository>();
                });
            Configure<AbpDbContextOptions>(options =>
            {
                options.UseMySQL();
            });
        }
    }
}
