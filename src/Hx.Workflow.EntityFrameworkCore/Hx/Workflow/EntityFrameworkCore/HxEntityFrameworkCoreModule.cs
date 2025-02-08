using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.StepBodys;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.Data;
using Volo.Abp;
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
            Configure<AbpDataFilterOptions>(options => {
                options.DefaultStates[typeof(ISoftDelete)] = new DataFilterState(isEnabled: true);
            });
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            context.Services.AddAbpDbContext<WkDbContext>(
                options =>
                {
                    options.AddDefaultRepositories<WkDbContext>();
                    options.AddRepository<WkDefinition, WkDefinitionRespository>();
                    options.AddRepository<WkStepBody, WkStepBodyRespository>();
                    options.AddRepository<WkAuditor, WkAuditorRespository>();
                    options.AddRepository<WkDefinitionGroup, WkDefinitionGroupRepository>();
                });
            Configure<AbpDbContextOptions>(options =>
            {
                options.UseNpgsql();
            });
        }
    }
}
