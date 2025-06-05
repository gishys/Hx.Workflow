using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using System.Linq;
using WorkflowCore.Models;

namespace Hx.Workflow.Application
{
    [Authorize]
    public class WkInstanceAppService(
        IWkInstanceRepository wkInstanceRepository,
        IWkErrorRepository errorRepository
        ) : WorkflowAppServiceBase, IWkInstanceAppService
    {
        private readonly IWkErrorRepository _errorRepository = errorRepository;
        private readonly IWkInstanceRepository _wkInstanceRepository = wkInstanceRepository;
        /// <summary>
        /// delete workflow instance
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual Task DeleteAsync(Guid workflowId)
        {
            return _wkInstanceRepository.DeleteAsync(workflowId);
        }
        /// <summary>
        /// 通过业务编号获得实例详细信息
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public virtual async Task<WkCurrentInstanceDetailsDto> GetWkInstanceAsync(string reference)
        {
            var instance = await _wkInstanceRepository.GetByReferenceAsync(reference) ?? throw new UserFriendlyException(message: $"不存在reference为：[{reference}]流程实例！");
            var businessData = JsonSerializer.Deserialize<Dictionary<string, object>>(instance.Data);
            WkExecutionPointer pointer = instance.ExecutionPointers.First(d => d.Status != PointerStatus.Complete);
            var errors = await _errorRepository.GetListByIdAsync(instance.Id, pointer.Id);
            return instance.ToWkInstanceDetailsDto(ObjectMapper, businessData, pointer, CurrentUser.Id, errors);
        }
    }
}
