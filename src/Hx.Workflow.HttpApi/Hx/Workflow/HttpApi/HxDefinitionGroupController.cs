using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("/hxworkflow/hxdefinitiongroup")]
    public class HxDefinitionGroupController(IWkDefinitionGroupAppService appService) : AbpController
    {
        private readonly IWkDefinitionGroupAppService _appService = appService;

        [HttpPost]
        public Task CreateAsync(WkDefinitionGroupCreateDto input)
        {
            return _appService.CreateAsync(input);
        }
        [HttpPut]
        public Task UpdateAsync(WkDefinitionGroupUpdateDto input)
        {
            return _appService.UpdateAsync(input);
        }
        [HttpGet]
        [Route("all")]
        //[AllowAnonymous]
        public Task<List<WkDefinitionGroupDto>> GetAllAsync(bool includeArchived = false)
        {
            return _appService.GetAllWithChildrenAsync(includeArchived);
        }
        [HttpDelete]
        public Task DeleteAsync(Guid id)
        {
            return _appService.DeleteAsync(id);
        }
    }
}
