using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Application.DynamicCode
{
    public class DynamicClassExecutor(
        IServiceProvider serviceProvider,
        DynamicTypeLoader typeLoader,
        ILogger<DynamicClassExecutor> logger) : IDynamicClassExecutor, ITransientDependency
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly DynamicTypeLoader _typeLoader = typeLoader;
        private readonly ILogger<DynamicClassExecutor> _logger = logger;

        public async Task ExecuteClassAsync(string classCode, string methodName = "Execute")
        {
            await ExecuteMethodAsync(classCode, methodName);
        }

        public async Task<object?> ExecuteMethodAsync(
            string classCode,
            string methodName = "Execute",
            object[]? parameters = null,
            Type[]? genericArguments = null)
        {
            parameters ??= Array.Empty<object>();

            // 加载动态类型
            var (_, types) = await _typeLoader.LoadTypesFromCodeAsync(classCode);
            var type = types.First();

            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // 使用当前 ABP 作用域构造动态类型。复制父容器全部描述符并重新
            // BuildServiceProvider 会造成 Singleton 所有权重复；临时容器释放时
            // 可能连正在运行的 WorkflowCore 服务一起释放。
            var instance = ActivatorUtilities.CreateInstance(services, type);

            try
            {
                // 查找方法也必须处于 finally 保护中，确保绑定失败时释放实例。
                MethodInfo? method;
                if (genericArguments != null && genericArguments.Length > 0)
                {
                    method = type.GetMethod(methodName)?.MakeGenericMethod(genericArguments);
                }
                else
                {
                    method = type.GetMethod(methodName);
                }

                if (method == null)
                {
                    throw new AbpException($"Method {methodName} not found in type {type.Name}");
                }

                // 异步执行方法
                return await AsyncMethodExecutor.ExecuteAsync(method, instance, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing dynamic method {Method} in {Type}",
                    methodName, type.Name);
                throw new AbpException($"Dynamic execution failed: {ex.Message}", ex);
            }
            finally
            {
                switch (instance)
                {
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync();
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
            }
        }
    }
}
