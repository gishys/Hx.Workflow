using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Application.DynamicCode
{
    public class StepExecutor : ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider;
        public StepExecutor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ExecuteDynamicStep(string code)
        {
            // 编译代码
            var executor = new DynamicCodeExecutor();
            var assembly = executor.CompileCode(code);

            // 注册到 DI
            var registrar = new DynamicTypeRegistrar(_serviceProvider);
            registrar.RegisterDynamicType(assembly);
        }
    }
}
