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
using Hx.Workflow.Domain;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Application
{
    [Authorize]
    public class WkInstanceAppService(
        IWkInstanceRepository wkInstanceRepository,
        IWkErrorRepository errorRepository,
        HxWorkflowManager hxWorkflowManager,
        IWkExecutionPointerRepository wkExecutionPointerRepository
        ) : WorkflowAppServiceBase, IWkInstanceAppService
    {
        private readonly IWkErrorRepository _errorRepository = errorRepository;
        private readonly IWkInstanceRepository _wkInstanceRepository = wkInstanceRepository;
        private readonly HxWorkflowManager _hxWorkflowManager = hxWorkflowManager;
        private readonly IWkExecutionPointerRepository _wkExecutionPointerRepository = wkExecutionPointerRepository;
        /// <summary>
        /// delete workflow instance
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(Guid workflowId)
        {
            var entity = await _wkInstanceRepository.FindAsync(workflowId);
            var pointers = await _wkExecutionPointerRepository.GetListAsync(workflowId);
            await _wkExecutionPointerRepository.HardDeleteAsync(pointers);
            if (CurrentUnitOfWork != null)
                await CurrentUnitOfWork.SaveChangesAsync();
            if (entity != null)
            {
                await _wkInstanceRepository.HardDeleteAsync([entity], true);
            }
        }
        /// <summary>
        /// delete workflow instances
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public virtual async Task DeletesAsync(Guid[] ids)
        {
            foreach (var id in ids)
            {
                var entity = await _wkInstanceRepository.FindAsync(id);
                var pointers = await _wkExecutionPointerRepository.GetListAsync(id);
                await _wkExecutionPointerRepository.HardDeleteAsync(pointers);
                if (CurrentUnitOfWork != null)
                    await CurrentUnitOfWork.SaveChangesAsync();
                if (entity != null)
                {
                    await _wkInstanceRepository.HardDeleteAsync([entity], true);
                }
            }
        }
        /// <summary>
        /// 终止工作流
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> TerminateWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.TerminateWorkflowAsync(workflowId);
        }
        /// <summary>
        /// 挂起工作流
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> SuspendWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.SuspendWorkflowAsync(workflowId);
        }
        /// <summary>
        /// 恢复工作流
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> ResumeWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.ResumeWorkflowAsync(workflowId);
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