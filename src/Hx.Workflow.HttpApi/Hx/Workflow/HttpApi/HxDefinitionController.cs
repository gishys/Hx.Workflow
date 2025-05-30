using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("/hxworkflow/hxdefinition")]
    public class HxDefinitionController : AbpController
    {
        private IWkDefinitionAppService _appService;
        public HxDefinitionController(IWkDefinitionAppService appService)
        {
            _appService = appService;
        }
        [HttpPost]
        public Task CreateAsync(WkDefinitionCreateDto input)
        {
            return _appService.CreateAsync(input);
        }
        [HttpPut]
        public Task<WkDefinitionDto> UpdateAsync(WkDefinitionUpdateDto input)
        {
            return _appService.UpdateAsync(input);
        }
        [HttpPut]
        [Route("nodes")]
        public Task<List<WkNodeDto>> UpdateAsync(DefinitionNodeUpdateDto input)
        {
            return _appService.UpdateAsync(input);
        }
        [HttpGet]
        public Task<WkDefinitionDto> GetAsync(Guid id)
        {
            return _appService.GetAsync(id);
        }
        [HttpDelete]
        public Task DeleteAsync(Guid id)
        {
            return _appService.DeleteAsync(id);
        }
        [HttpGet]
        [Route("details")]
        public Task<WkDefinitionDto> GetAsync(Guid id, int version = 1)
        {
            return _appService.GetDefinitionAsync(id, version);
        }
    }
}