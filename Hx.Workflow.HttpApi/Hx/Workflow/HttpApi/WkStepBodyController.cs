using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("stepbody")]
    public class WkStepBodyController(
        IWkStepBodyAppService wkStepBody
            ) : AbpController
    {
        public IWkStepBodyAppService WkStepBody { get; set; } = wkStepBody;

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