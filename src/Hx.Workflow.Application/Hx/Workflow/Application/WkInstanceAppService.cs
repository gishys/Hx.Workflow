using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace Hx.Workflow.Application
{
    [Authorize]
    public class WkInstanceAppService(IWkInstanceRepository wkInstanceRepository) : WorkflowAppServiceBase, IWkInstanceAppService
    {
        private readonly IWkInstanceRepository _wkInstanceRepository = wkInstanceRepository;
        /// <summary>
        /// delete workflow instance
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual Task DeleteAsync(Guid workflowId)
        {
            return _wkInstanceRepository.DeleteAsync(workflowId);
        }
    }
}
