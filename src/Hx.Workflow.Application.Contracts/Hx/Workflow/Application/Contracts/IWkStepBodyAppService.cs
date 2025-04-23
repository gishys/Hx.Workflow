using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWkStepBodyAppService
    {
        Task CreateAsync(WkSepBodyCreateDto input);
        Task DeleteAsync(Guid id);
        Task<WkStepBodyDto?> GetStepBodyAsync(string name);
        Task<PagedResultDto<WkStepBodyDto>> GetPagedAsync(WkStepBodyPagedInput input);
        Task UpdateAsync(WkStepBodyUpdateDto input);
        Task<List<WkStepBodyDto>> GetAllAsync();
    }
}
