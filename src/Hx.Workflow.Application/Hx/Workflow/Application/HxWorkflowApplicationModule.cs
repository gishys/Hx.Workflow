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
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace Hx.Workflow.Application
{
    
    [DependsOn(typeof(AbpAutoMapperModule))]
    [DependsOn(typeof(HxWorkflowDomainModule))]
    [DependsOn(typeof(AbpDddApplicationModule))]
    [DependsOn(typeof(AbpBackgroundJobsAbstractionsModule))]
    [DependsOn(typeof(HxWorkflowApplicationContractsModule))]
    public class HxWorkflowApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // 配置后台作业选项
            Configure<AbpBackgroundJobOptions>(options =>
            {

            });

            context.Services.AddAutoMapperObjectMapper<HxWorkflowApplicationModule>();
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddProfile<HxWorkflowAutoMapperProfile>(validate: true);
            });
        }
        public async override void OnPostApplicationInitialization(ApplicationInitializationContext context)
        {
            using var scope = context.ServiceProvider.CreateScope();
            var stepbodyRespository = scope.ServiceProvider.GetService<IWkStepBodyRespository>() ?? throw new UserFriendlyException(message: "IWkStepBodyRespository服务依赖注入失败！");
            var stepbodys = ReflectionHelper.GetStepBodyAsyncDerivatives();
            var sList = new List<WkStepBody>();
            foreach (var stepbody in stepbodys)
            {
                if (string.IsNullOrEmpty(stepbody.Name) || string.IsNullOrEmpty(stepbody.DisplayName))
                    continue;
                if (!await stepbodyRespository.AnyAsync(stepbody.TypeFullName))
                {
                    var s = new WkStepBody(stepbody.Name, stepbody.DisplayName, null, [], stepbody.TypeFullName, stepbody.AssemblyFullName);
                    
                    // 先保存WkStepBody，然后创建参数
                    await stepbodyRespository.InsertAsync(s);
                    
                    List<WkStepBodyParam> ps = [
                        new WkStepBodyParam(
                                Guid.NewGuid(),
                                s.Id, // 使用已保存的WkStepBody的Id
                                "Candidates",
                                "Candidates",
                                "审核人",
                                "data.Candidates",
                                StepBodyParaType.Inputs),
                               new WkStepBodyParam(
                                Guid.NewGuid(),
                                s.Id, // 使用已保存的WkStepBody的Id
                                "DecideBranching",
                                "DecideBranching",
                                "步骤分支",
                                "data.DecideBranching",
                                StepBodyParaType.Inputs),
                                new WkStepBodyParam(
                                Guid.NewGuid(),
                                s.Id, // 使用已保存的WkStepBody的Id
                                "DecideBranching",
                                "DecideBranching",
                                "步骤分支",
                                "step.DecideBranching",
                                StepBodyParaType.Outputs),
                                new WkStepBodyParam(
                                Guid.NewGuid(),
                                s.Id, // 使用已保存的WkStepBody的Id
                                "Candidates",
                                "Candidates",
                                "下一步骤审核人",
                                "step.NextCandidates",
                                StepBodyParaType.Outputs),
                ];
                    
                    await s.UpdateInputs(ps);
                    await stepbodyRespository.UpdateAsync(s);
                    
                    if (!sList.Any(d => d.Name == s.Name))
                        sList.Add(s);
                }
            }
            // 注意：我们已经在上面的循环中单独插入了每个WkStepBody
            // 所以这里不需要批量插入
        }
    }
}