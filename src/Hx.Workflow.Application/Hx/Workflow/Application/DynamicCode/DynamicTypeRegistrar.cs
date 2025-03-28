using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using Volo.Abp;

namespace Hx.Workflow.Application.DynamicCode
{
    public class DynamicTypeRegistrar
    {
        private readonly IServiceProvider _serviceProvider;

        public DynamicTypeRegistrar(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void RegisterDynamicType(Assembly assembly)
        {
            var stepBodyType = assembly.GetType("Hx.Workflow.Application.StepBodys.StartStepBody");
            if (stepBodyType == null)
                throw new InvalidOperationException("未找到 StartStepBody 类型");
            var services = _serviceProvider.GetRequiredService<IServiceCollection>();
            // 手动注册到 ABP DI
            services.AddTransient(stepBodyType);
        }
    }
}
