using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Application.DynamicCode;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.Api
{
    [ApiController]
    [Route("stepbody")]
    public class WkStepBodyController : AbpController
    {
        public IWkStepBodyAppService WkStepBody { get; set; }
        public WkStepBodyController(
            IWkStepBodyAppService wkStepBody
            )
        {
            WkStepBody = wkStepBody;
        }
        [HttpPost]
        public Task CreateAsync(WkSepBodyCreateDto input)
        {
            return WkStepBody.CreateAsync(input);
        }
        [HttpDelete]
        public Task DeleteAsync(Guid id)
        {
            return WkStepBody.DeleteAsync(id);
        }
        [HttpGet]
        public Task<WkStepBodyDto> GetStepBodyAsync(string name)
        {
            return WkStepBody.GetStepBodyAsync(name);
        }
        [HttpGet]
        [Route("paged")]
        public Task<PagedResultDto<WkStepBodyDto>> GetPagedAsync(WkStepBodyPagedInput input)
        {
            return WkStepBody.GetPagedAsync(input);
        }
        [HttpPut]
        public Task UpdateAsync(WkStepBodyUpdateDto input)
        {
            return WkStepBody.UpdateAsync(input);
        }
        [HttpGet]
        [Route("all")]
        public Task<List<WkStepBodyDto>> GetAllAsync()
        {
            return WkStepBody.GetAllAsync();
        }
    }
}