using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application
{
    public class BayWorld : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine("stop world" + Counter.StopAddtion++);
            return ExecutionResult.Next();
        }
    }
}
