using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.Api
{
    [ApiController]
    [Route("hxdefinition")]
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
        public Task UpdateAsync(WkDefinitionUpdateDto input)
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
    }
}