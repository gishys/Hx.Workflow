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
        /// <summary>
        /// 通知外部系统并删除流程实例；若通知失败则中止删除
        /// </summary>
        [HttpPost]
        [Route("notify-delete")]
        public Task NotifyAndDeleteAsync([FromBody] WkNotifyAndDeleteInput input)
        {
            return _instance.NotifyAndDeleteAsync(input);
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
