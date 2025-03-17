using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Application.StepBodys;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;

namespace Hx.Workflow.Application
{
    [DependsOn(typeof(AbpAutoMapperModule))]
    [DependsOn(typeof(HxWorkflowDomainModule))]
    [DependsOn(typeof(AbpDddApplicationModule))]
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
        public async override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            using (var scope = context.ServiceProvider.CreateScope())
            {
                var stepbodyRespository = scope.ServiceProvider.GetService<IWkStepBodyRespository>();
                var stepbodys = ReflectionHelper.GetStepBodyAsyncDerivatives();
                var sList = new List<WkStepBody>();
                List<WkStepBodyParam> ps = [
                    new WkStepBodyParam(
                Guid.NewGuid(),
                "Candidates",
                "Candidates",
                "审核人",
                "data.Candidates",
                StepBodyParaType.Inputs),
                new WkStepBodyParam(
                Guid.NewGuid(),
                "DecideBranching",
                "DecideBranching",
                "步骤分支",
                "step.DecideBranching",
                StepBodyParaType.Outputs),
                ];
                foreach (var stepbody in stepbodys)
                {
                    if (string.IsNullOrEmpty(stepbody.Name) || string.IsNullOrEmpty(stepbody.DisplayName))
                        continue;
                    if (!await stepbodyRespository.AnyAsync(stepbody.TypeFullName))
                    {
                        var s = new WkStepBody(stepbody.Name, stepbody.DisplayName, "", ps, stepbody.TypeFullName, stepbody.AssemblyFullName);
                        if (!sList.Any(d => d.Name == s.Name))
                            sList.Add(s);
                    }
                }
                if (sList.Count > 0)
                {
                    await stepbodyRespository.InsertManyAsync(sList);
                }
            }
        }
    }
}