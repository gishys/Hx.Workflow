using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("hxworkflow/instance")]
    public class WkInstanceController(IWkInstanceAppService instance) : AbpController
    {
        private readonly IWkInstanceAppService _instance = instance;
        [HttpDelete]
        public Task DeleteAsync(Guid executionPointerId)
        {
            return _instance.DeleteAsync(executionPointerId);
        }
    }
}
