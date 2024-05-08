using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
    public class ReceiveAcceptanceStepBody : StepBody
    {
        public string UserId { get; set; }
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine($"{UserId}：收件受理");
            UserId = UserId + ":收件受理完成";
            return ExecutionResult.Next();
        }
    }
}
