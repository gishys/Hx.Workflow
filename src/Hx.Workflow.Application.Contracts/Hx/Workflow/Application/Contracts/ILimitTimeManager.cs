using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Application.Contracts
{
    public interface ILimitTimeManager : ITransientDependency
    {
        Task<DateTime?> GetAsync(DateTime time, int? minutes);
    }
}
