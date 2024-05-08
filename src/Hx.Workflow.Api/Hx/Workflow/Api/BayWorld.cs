using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Api
{
    public class BayWorld : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine("stop world");
            return ExecutionResult.Next();
        }
    }
}
