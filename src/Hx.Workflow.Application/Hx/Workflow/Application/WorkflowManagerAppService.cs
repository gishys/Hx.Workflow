using Hx.Workflow.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp.DependencyInjection;
using WorkflowCore.Interface;

namespace Hx.Workflow.Application
{
    [Dependency(ServiceLifetime.Singleton, ReplaceServices = true)]
    public class WorkflowManagerAppService 
        : WorkflowAppServiceBase, IWorkflowManagerAppService
    {
        private readonly IWorkflowHost _workflowHost;
        public WorkflowManagerAppService(
            IWorkflowHost workflowHost)
        {
            _workflowHost = workflowHost;
        }
        public void Create()
        {
            //if (!_workflowHost.Registry.IsRegistered("HelloWorld", 1))
            //    _workflowHost.RegisterWorkflow<dynamic>();
            //_workflowHost.Start();
            //Console.WriteLine("开始");
            //_workflowHost.StartWorkflow("HelloWorld", 1, null);
            //Console.WriteLine("结束");
            //_workflowHost.Stop();
        }
    }
}
