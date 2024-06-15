using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkErrorRepository : IBasicRepository<WkExecutionError, Guid>
    {
        Task<List<WkExecutionError>> GetListByIdAsync(Guid? workflowId, Guid? pointerId);
    }
}
