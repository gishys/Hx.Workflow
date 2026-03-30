using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWkInstanceAppService
    {
        /// <summary>
        /// delete workflow instance
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid workflowId);
        Task<bool> ResumeWorkflowAsync(string workflowId);
        Task<bool> SuspendWorkflowAsync(string workflowId);
        Task<bool> TerminateWorkflowAsync(string workflowId);
        Task DeletesAsync(Guid[] ids);
        Task<ICollection<WkExecutionErrorDto>> GetErrorListAsync(Guid wkInstanceId, Guid executionPointerId);
        /// <summary>
        /// 通知外部系统并删除流程实例；若通知事件执行失败则中止删除
        /// </summary>
        Task NotifyAndDeleteAsync(WkNotifyAndDeleteInput input);
    }
}
