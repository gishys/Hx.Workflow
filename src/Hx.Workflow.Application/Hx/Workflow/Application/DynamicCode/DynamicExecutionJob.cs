using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Application.DynamicCode
{
    public class DynamicExecutionJob(IDynamicClassExecutor executor) : AsyncBackgroundJob<DynamicExecutionJobArgs>, ITransientDependency
    {
        private readonly IDynamicClassExecutor _executor = executor;

        public override async Task ExecuteAsync(DynamicExecutionJobArgs args)
        {
            try
            {
                await _executor.ExecuteMethodAsync(
                    args.ClassCode,
                    args.MethodName,
                    args.Parameters);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw;
            }
        }
    }
}
