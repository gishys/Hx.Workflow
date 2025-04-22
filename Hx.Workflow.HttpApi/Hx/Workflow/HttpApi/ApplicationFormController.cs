using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("applicationform")]
    public class ApplicationFormController(
        IWkApplicationFormAppService formAppService
            ) : AbpController
    {
        public IWkApplicationFormAppService FormAppService { get; set; } = formAppService;

        [HttpPost]
        public Task CreateAsync(ApplicationFormCreateDto input)
        {
            return FormAppService.CreateAsync(input);
        }
        [HttpGet]
        [Route("paged")]
        public Task<PagedResultDto<ApplicationFormDto>> GetPagedAsync([FromQuery] ApplicationFormQueryInput input)
        {
            return FormAppService.GetPagedAsync(input);
        }
        [HttpGet]
        public Task<ApplicationFormDto> GetAsync(Guid id)
        {
            return FormAppService.GetAsync(id);
        }
        [HttpPut]
        public Task UpdateAsync(ApplicationFormUpdateDto input)
        {
            return FormAppService.UpdateAsync(input);
        }
        [HttpDelete]
        public Task DeleteAsync(Guid id)
        {
            return FormAppService.DeleteAsync(id);
        }
    }
}
