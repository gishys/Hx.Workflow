using Hx.Workflow.Domain.Persistence;
using System;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkErrorRepository : IBasicRepository<WkExecutionError, Guid>
    {
    }
}
