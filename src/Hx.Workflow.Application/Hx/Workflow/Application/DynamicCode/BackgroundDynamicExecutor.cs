using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Hx.Workflow.Application.DynamicCode
{
    public class BackgroundDynamicExecutor(IBackgroundJobManager backgroundJobManager) : ITransientDependency
    {
        private readonly IBackgroundJobManager _backgroundJobManager = backgroundJobManager;

        public async Task<string> ExecuteInBackgroundAsync(
            string classCode,
            string methodName = "Execute",
            object[]? parameters = null)
        {
            var jobArgs = new DynamicExecutionJobArgs
            {
                ClassCode = classCode,
                MethodName = methodName,
                Parameters = parameters
            };

            return await _backgroundJobManager
                .EnqueueAsync(jobArgs, BackgroundJobPriority.High);
        }
    }
}
