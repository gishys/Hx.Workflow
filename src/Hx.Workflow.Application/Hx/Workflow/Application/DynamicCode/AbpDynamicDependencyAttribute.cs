using System;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Application.DynamicCode
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AbpDynamicDependencyAttribute : Attribute, ITransientDependency
    {
    }
}
