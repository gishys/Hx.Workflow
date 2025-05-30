using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("/hxworkflow/hxdefinitiongroup")]
    public class HxDefinitionGroupController : AbpController
    {
        private IWkDefinitionGroupAppService _appService;
        public HxDefinitionGroupController(IWkDefinitionGroupAppService appService)
        {
            _appService = appService;
        }
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
        public Task<List<WkDefinitionGroupDto>> GetAllAsync()
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