using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("/hxworkflow/applicationformgroup")]
    public class ApplicationFormGroupController : AbpController
    {
        private IApplicationFormGroupAppService _appService;
        public ApplicationFormGroupController(IApplicationFormGroupAppService appService)
        {
            _appService = appService;
        }
        [HttpPost]
        public Task CreateAsync(ApplicationFormGroupCreateDto input)
        {
            return _appService.CreateAsync(input);
        }
        [HttpPut]
        public Task UpdateAsync(ApplicationFormGroupUpdateDto input)
        {
            return _appService.UpdateAsync(input);
        }
        [HttpGet]
        [Route("all")]
        public Task<List<ApplicationFormGroupDto>> GetAllAsync()
        {
            return _appService.GetAllWithChildrenAsync();
        }
        [HttpDelete]
        public Task DeleteAsync(Guid id)
        {
            return _appService.DeleteAsync(id);
        }
    }
}