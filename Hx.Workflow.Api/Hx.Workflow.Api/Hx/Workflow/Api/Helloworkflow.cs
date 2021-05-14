using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Api
{
    public class MyWorkflow : IWorkflow
    {
        public string Id => "HelloWorld";
        public int Version => 1;
        public void Build(IWorkflowBuilder<object> builder)
        {
            builder
               .StartWith<HelloWorld>()
               .Then<BayWorld>();
        }
    }
}
