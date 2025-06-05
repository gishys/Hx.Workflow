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
    }
}
