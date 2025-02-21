using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkApplicationFormRepository : IBasicRepository<ApplicationForm, Guid>
    {
        Task<List<ApplicationForm>> GetPagedAsync(string? filter, int skipCount, int maxResultCount);
        Task<int> GetPagedCountAsync(string? filter);
        Task<bool> ExistByNameAsync(string name);
        Task<bool> ExistByTitleAsync(string title, Guid? groupId);
    }
}
