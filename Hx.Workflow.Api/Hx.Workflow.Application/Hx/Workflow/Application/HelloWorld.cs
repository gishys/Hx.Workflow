using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application
{
    public class HelloWorld : StepBody
    {
        public string StepInput { get; set; }
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine("Hello world" + Counter.StartAddtion++);
            return ExecutionResult.Next();
        }
    }
}
