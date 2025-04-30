using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [Route("workflow/base")]
    [ApiController]
    public class BaseController(IWorkflowBaseAppService baseAppService) : AbpControllerBase
    {
        private readonly IWorkflowBaseAppService _baseAppService = baseAppService;
        [HttpGet]
        public string Get(string permissionName)
        {
            return _baseAppService.GetLocalization(permissionName);
        }
    }
}
