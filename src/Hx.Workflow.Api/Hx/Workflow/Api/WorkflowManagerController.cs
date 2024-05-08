using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.Api
{
    [ApiController]
    [Route("workflowmanager")]
    public class WorkflowManagerController : AbpController
    {
        private readonly IWorkflowManagerAppService _workflowManager;
        public WorkflowManagerController(
            IWorkflowManagerAppService workflowManager)
        {
            _workflowManager = workflowManager;
        }
        [HttpGet]
        public void Create()
        {
            _workflowManager.Create();
        }
    }
}
