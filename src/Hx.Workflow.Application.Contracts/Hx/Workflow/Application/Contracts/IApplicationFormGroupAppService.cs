using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public interface IApplicationFormGroupAppService
    {
        Task CreateAsync(ApplicationFormGroupCreateDto dto);
        Task UpdateAsync(ApplicationFormGroupUpdateDto dto);
        Task DeleteAsync(Guid id);
        Task<List<ApplicationFormGroupDto>> GetAllWithChildrenAsync();
    }
}
