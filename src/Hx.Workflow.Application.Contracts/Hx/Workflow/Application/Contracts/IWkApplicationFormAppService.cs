using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWkApplicationFormAppService
    {
        Task CreateAsync(ApplicationFormCreateDto input);
        Task<PagedResultDto<ApplicationFormDto>> GetPagedAsync(ApplicationFormQueryInput input);
        Task UpdateAsync(ApplicationFormUpdateDto input);
        Task DeleteAsync(Guid id);
        Task<ApplicationFormDto> GetAsync(Guid id);
    }
}
