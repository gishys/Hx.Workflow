using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWkDefinitionGroupAppService
    {
        Task CreateAsync(WkDefinitionGroupCreateDto dto);
        Task UpdateAsync(WkDefinitionGroupUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<List<WkDefinitionGroupDto>> GetAllWithChildrenAsync(bool includeArchived = false);
    }
}
