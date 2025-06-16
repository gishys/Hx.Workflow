using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("hxworkflow/instance")]
    public class WkInstanceController(IWkInstanceAppService instance) : AbpController
    {
        private readonly IWkInstanceAppService _instance = instance;
        [HttpDelete]
        public Task DeleteAsync(Guid workflowId)
        {
            return _instance.DeleteAsync(workflowId);
        }
        [HttpDelete]
        [Route("deletes")]
        public Task DeletesAsync(Guid[] ids)
        {
            return _instance.DeletesAsync(ids);
        }
        [HttpPut]
        [Route("terminate")]
        public Task<bool> TerminateWorkflowAsync(string workflowId)
        {
            return _instance.TerminateWorkflowAsync(workflowId);
        }
        [HttpPut]
        [Route("suspend")]
        public Task<bool> SuspendWorkflowAsync(string workflowId)
        {
            return _instance.SuspendWorkflowAsync(workflowId);
        }
        [HttpPut]
        [Route("resume")]
        public Task<bool> ResumeWorkflowAsync(string workflowId)
        {
            return _instance.ResumeWorkflowAsync(workflowId);
        }
        [HttpPut]
        [Route("error")]
        public Task<ICollection<WkExecutionErrorDto>> GetErrorListAsync(Guid wkInstanceId, Guid executionPointerId)
        {
            return _instance.GetErrorListAsync(wkInstanceId, executionPointerId);
        }
    }
}
