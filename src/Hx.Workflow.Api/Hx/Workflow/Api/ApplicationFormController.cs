using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.Api
{
    [ApiController]
    [Route("applicationform")]
    public class ApplicationFormController : AbpController
    {
        public IWkApplicationFormAppService FormAppService { get; set; }
        public ApplicationFormController(
            IWkApplicationFormAppService formAppService
            )
        {
            FormAppService = formAppService;
        }
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
