using Hx.Workflow.Domain.Persistence;
using System;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkExecutionPointerRepository : IBasicRepository<WkExecutionPointer, Guid>
    {
    }
}
