using Hx.Workflow.Application;
using Hx.Workflow.Domain;
using Hx.Workflow.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Linq;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.AntiForgery;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;

namespace Hx.Workflow.Api
{
    [DependsOn(typeof(AbpMultiTenancyModule))]
    [DependsOn(typeof(AbpAutofacModule))]
    [DependsOn(typeof(AbpAspNetCoreMvcModule))]
    [DependsOn(typeof(HxWorkflowApplicationModule))]
    [DependsOn(typeof(HxEntityFrameworkCoreModule))]
    public class HxWorkflowApiModule : AbpModule
    {
        private const string DefaultCorsPolicyName = "Default";
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAntiForgeryOptions>(options =>
            {
                options.AutoValidateIgnoredHttpMethods.Add("POST");
                options.AutoValidateIgnoredHttpMethods.Add("DELETE");
                options.AutoValidateIgnoredHttpMethods.Add("PUT");
                options.AutoValidateIgnoredHttpMethods.Add("GET");
            });
            var configuration = context.Services.GetConfiguration();
            context.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "应用通讯 Api", Version = "v0.1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            });
            context.Services.Configure<AbpExceptionHandlingOptions>(options =>
            {
                options.SendExceptionsDetailsToClients = true;
            });
            context.Services.AddCors(options =>
            {
                options.AddPolicy(DefaultCorsPolicyName,
                builder =>
                {
                    builder.WithOrigins(configuration["CorsOrigins"]
                                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                                .Select(o => o.RemovePostFix("/"))
                                .ToArray())
                        .WithAbpExposedHeaders()
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            //context.Services.AddSingleton<IHxPersistenceProvider, HxPersistenceProvider>();
            context.Services.AddLogging();
            context.Services.AddWorkflow(
                options =>
                {
                    options.UsePersistence(sp => sp.GetService<HxPersistenceProvider>());
                });
            context.Services.AddWorkflowDSL();
        }
        public override void OnApplicationInitialization(
            ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            var env = context.GetEnvironment();
            app.UseCors(DefaultCorsPolicyName);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hx Base Chat");
            });
            app.UseAuditing();
            app.UseConfiguredEndpoints();
        }
    }
}