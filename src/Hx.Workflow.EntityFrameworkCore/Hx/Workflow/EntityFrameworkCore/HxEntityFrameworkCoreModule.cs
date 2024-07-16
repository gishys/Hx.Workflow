using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.StepBodys;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.Modularity;

namespace Hx.Workflow.EntityFrameworkCore
{
    [DependsOn(typeof(HxWorkflowDomainModule))]
    [DependsOn(typeof(AbpEntityFrameworkCoreModule))]
    [DependsOn(typeof(AbpEntityFrameworkCorePostgreSqlModule))]
    public class HxEntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
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
                options.UseNpgsql();
            });
        }
    }
}
