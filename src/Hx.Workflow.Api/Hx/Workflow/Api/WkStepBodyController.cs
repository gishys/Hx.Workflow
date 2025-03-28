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
        private StepExecutor _stepExecutor { get; set; }
        public WkStepBodyController(
            IWkStepBodyAppService wkStepBody,
            StepExecutor stepExecutor
            )
        {
            WkStepBody = wkStepBody;
            _stepExecutor = stepExecutor;
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
        [HttpPut]
        [Route("code")]
        public void ExecutionCode()
        {
            var code = @"using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Local;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Hx.Workflow.Application.StepBodys
{
    public class StartStepBody : StepBodyAsync, ITransientDependency
    {
        private readonly ILocalEventBus _localEventBus;
        private readonly IWkInstanceRepository _wkInstance;
        public const string Name = ""StartStepBody"";
        public const string DisplayName = ""开始"";
        public StartStepBody(ILocalEventBus localEventBus, IWkInstanceRepository wkInstance)
        {
            _localEventBus = localEventBus;
            _wkInstance = wkInstance;
        }
        /// <summary>
        /// 审核人
        /// </summary>
        public string Candidates { get; set; }
        /// <summary>
        /// 分支判断
        /// </summary>
        public string DecideBranching { get; set; } = null;
        public async override Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var instance = await _wkInstance.FindAsync(new Guid(context.Workflow.Id));
            try
            {
                await _localEventBus.PublishAsync(new StartStepBodyChangeEvent(
                    new Guid(context.Workflow.Id),
                    instance.Reference,
                    context.Workflow.Data as IDictionary<string, object>));
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($""StartStepBodyChangeEvent 改变事件异常：{ex.Message}"");
            }
            return ExecutionResult.Next();
        }
    }
}
";
            _stepExecutor.ExecuteDynamicStep(code);
        }
    }
}