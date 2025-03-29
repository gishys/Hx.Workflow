using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Hx.Workflow.Application.DynamicCode
{
    public class DynamicTypeRegistrar
    {
        public DynamicTypeRegistrar()
        {
        }

        public void RegisterService<TService, TImplementation>(IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<TService, TImplementation>();
                    break;
                case ServiceLifetime.Scoped:
                    services.AddScoped<TService, TImplementation>();
                    break;
                default:
                    services.AddTransient<TService, TImplementation>();
                    break;
            }
        }
        public void RegisterService(IServiceCollection services, Type serviceTyep, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton(serviceTyep);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddSingleton(serviceTyep);
                    break;
                default:
                    services.AddSingleton(serviceTyep);
                    break;
            }
        }
    }
}
