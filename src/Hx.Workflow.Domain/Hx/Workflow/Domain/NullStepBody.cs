using Volo.Abp.DependencyInjection;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain
{
    public class NullStepBody : StepBody, ITransientDependency
    {
        public long UserId { get; set; }
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            return ExecutionResult.Next();
        }
    }
}
