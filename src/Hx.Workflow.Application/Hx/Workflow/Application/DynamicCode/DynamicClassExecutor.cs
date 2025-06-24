using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

            // 创建新的服务集合
            var serviceCollection = new ServiceCollection();

            // 添加父容器中的所有服务描述符
            var parentServiceCollection = services.GetRequiredService<IEnumerable<ServiceDescriptor>>();
            foreach (var descriptor in parentServiceCollection)
            {
                serviceCollection.Add(descriptor);
            }

            // 注册动态类型
            serviceCollection.AddTransient(type);

            var childProvider = serviceCollection.BuildServiceProvider();

            // 解析实例
            var instance = childProvider.GetRequiredService(type);

            // 查找方法
            MethodInfo? method = null;

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

            try
            {
                // 异步执行方法
                return await AsyncMethodExecutor.ExecuteAsync(method, instance, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing dynamic method {Method} in {Type}",
                    methodName, type.Name);
                throw new AbpException($"Dynamic execution failed: {ex.Message}", ex);
            }
        }
    }

    internal static class ServiceProviderExtensions
    {
        public static IEnumerable<ServiceDescriptor> GetServiceDescriptors(
            this IServiceProvider serviceProvider)
        {
            // 通过标准方式获取服务描述符
            return serviceProvider.GetRequiredService<IEnumerable<ServiceDescriptor>>();
        }
    }
}