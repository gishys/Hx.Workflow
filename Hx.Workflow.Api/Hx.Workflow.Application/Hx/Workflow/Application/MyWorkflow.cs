using WorkflowCore.Interface;

namespace Hx.Workflow.Application
{
    public class MyWorkflow : IWorkflow
    {
        public string Id => "HelloWorld";
        public int Version => 1;
        public void Build(IWorkflowBuilder<object> builder)
        {
            builder
               .StartWith<HelloWorld>()
               //.Input(d => d.StepInput, d => d.BusinessNumber)
               .Then<BayWorld>();
        }
    }
}
