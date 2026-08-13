using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Services;

namespace Hx.Workflow.Domain;

/// <summary>WorkflowCore 3.10 依赖注入注册兼容处理。</summary>
internal static class WorkflowCoreServiceCollectionCompatibility
{
    /// <summary>
    /// WorkflowCore 3.10 通过 transient <see cref="IBackgroundTask"/> 工厂暴露
    /// singleton 生命周期发布器。Autofac 等容器会因此让子作用域取得该实例的
    /// 所有权，并在作用域结束时提前释放它。这里改用 singleton、非 IDisposable
    /// 的转发对象，确保解析后台任务不会取得发布器的释放权。
    /// </summary>
    internal static bool ReplaceLifeCyclePublisherBackgroundTaskAlias(
        IServiceCollection services)
    {
        var workflowCoreAssembly = typeof(IWorkflowHost).Assembly;
        var publisherDescriptors = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(ILifeCycleEventPublisher) &&
                descriptor.Lifetime == ServiceLifetime.Singleton &&
                descriptor.ImplementationType == typeof(LifeCycleEventPublisher))
            .ToList();
        var aliases = services
            .Where(descriptor =>
                descriptor.ServiceType == typeof(IBackgroundTask) &&
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ImplementationFactory?.Method.DeclaringType?.Assembly ==
                    workflowCoreAssembly)
            .ToList();

        if (publisherDescriptors.Count == 0 && aliases.Count == 0)
        {
            return false;
        }

        if (publisherDescriptors.Count != 1 || aliases.Count != 1)
        {
            throw new InvalidOperationException(
                "Unexpected WorkflowCore life-cycle publisher registrations. " +
                $"Publishers: {publisherDescriptors.Count}, aliases: {aliases.Count}.");
        }

        foreach (var alias in aliases)
        {
            services.Remove(alias);
        }

        services.AddSingleton<IBackgroundTask>(serviceProvider =>
            new NonOwningLifeCycleEventPublisherBackgroundTask(
                serviceProvider.GetRequiredService<ILifeCycleEventPublisher>()));

        return true;
    }

    private sealed class NonOwningLifeCycleEventPublisherBackgroundTask(
        ILifeCycleEventPublisher publisher) : IBackgroundTask
    {
        public void Start()
        {
            publisher.Start();
        }

        public void Stop()
        {
            publisher.Stop();
        }
    }
}
