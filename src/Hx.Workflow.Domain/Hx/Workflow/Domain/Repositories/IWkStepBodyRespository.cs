using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkStepBodyRespository : IBasicRepository<WkStepBody, Guid>
    {
        Task<WkStepBody?> GetStepBodyAsync(string name);
        Task<List<WkStepBody>> GetPagedAsync(string filter, int skipCount, int maxResultCount);
        Task<int> GetPagedCountAsync(string filter);
        Task<bool> AnyAsync(string typeFullName, CancellationToken cancellationToken = default);
    }
}
