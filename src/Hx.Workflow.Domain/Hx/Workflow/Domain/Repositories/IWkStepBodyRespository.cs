using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkStepBodyRespository : IBasicRepository<WkStepBody, Guid>
    {
        Task<WkStepBody> GetStepBodyAsync(string name);
    }
}
