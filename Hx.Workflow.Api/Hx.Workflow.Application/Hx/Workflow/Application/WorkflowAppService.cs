using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using WorkflowCore.Models;

namespace Hx.Workflow.Application
{
    public class WorkflowAppService : HxWorkflowAppServiceBase, IWorkflowAppService
    {
        private readonly IWkStepBodyRespository _wkStepBody;
        private readonly HxWorkflowManager _hxWorkflowManager;
        private readonly IWkAuditorRespository _wkAuditor;
        public WorkflowAppService(
            IWkStepBodyRespository wkStepBody,
            HxWorkflowManager hxWorkflowManager,
            IWkAuditorRespository wkAuditor)
        {
            _wkStepBody = wkStepBody;
            _hxWorkflowManager = hxWorkflowManager;
            _wkAuditor = wkAuditor;
        }
        public virtual async Task CreateAsync(WkDefinitionCreateDto input)
        {
            var entity = new WkDefinition(
                    input.Title,
                    input.Icon,
                    input.Color,
                    input.SortNumber,
                    input.Discription,
                    version: input.Version <= 0 ? 1 : input.Version);
            var nodeEntitys = input.Nodes.ToWkNodes();
            foreach (var node in nodeEntitys)
            {
                var wkStepBodyId = input.Nodes.FirstOrDefault(d => d.Name == node.Name).WkStepBodyId;
                if (wkStepBodyId?.Length > 0)
                {
                    Guid.TryParse(wkStepBodyId, out Guid guidStepBodyId);
                    var stepBodyEntity = await _wkStepBody.FindAsync(guidStepBodyId);
                    if (stepBodyEntity == null)
                        throw new BusinessException(message: "StepBody没有查询到");
                    await node.SetWkStepBody(stepBodyEntity);
                }
                await entity.AddWkNode(node);
            }
            await _hxWorkflowManager.CreateAsync(entity);
        }
        public virtual async Task<WkDefinitionDto> GetDefinitionAsync(string name)
        {
            var entity = await _hxWorkflowManager.GetDefinitionAsync(name);
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
        }
        public virtual async Task StartAsync(StartWorkflowInput input)
        {
            await _hxWorkflowManager.StartWorlflowAsync(input.Id, input.Version, input.Inputs);
        }
        public virtual async Task StartActivityAsync(string actName, string workflowId, Dictionary<string, object> data = null)
        {
            await _hxWorkflowManager.StartActivityAsync(actName, workflowId, data);
        }
        /// <summary>
        /// 查询我的办理件（在办、废弃、已完成、挂起），如果为空则查询所有
        /// </summary>
        /// <param name="status"></param>
        /// <param name="userIds"></param>
        /// <param name="skipCount"></param>
        /// <param name="maxResultCount"></param>
        /// <returns></returns>
        public virtual async Task<List<WkInstancesDto>> GetMyWkInstanceAsync(
            WorkflowStatus? status = WorkflowStatus.Runnable,
            ICollection<Guid> userIds = null,
            int skipCount = 0,
            int maxResultCount = 20)
        {
            if (userIds?.Count <= 0 && CurrentUser?.Id != null)
            {
                userIds = new List<Guid>();
                userIds.Add((Guid)CurrentUser.Id);
            }
            var wkinstanceIds = await _wkAuditor.GetWkInstanceIdsAsync(userIds);
            if (wkinstanceIds?.Count > 0)
            {
                var instances =
                    await _hxWorkflowManager.WkInstanceRepository.GetMyInstancesAsync(
                        wkinstanceIds,
                        status,
                        skipCount,
                        maxResultCount);
                return ObjectMapper.Map<List<WkInstance>, List<WkInstancesDto>>(instances);
            }
            return null;
        }
        /// <summary>
        /// terminate workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> TerminateWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.TerminateWorkflowAsync(workflowId);
        }
        /// <summary>
        /// suspend workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> SuspendWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.SuspendWorkflowAsync(workflowId);
        }
        /// <summary>
        /// resume workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> ResumeWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.ResumeWorkflowAsync(workflowId);
        }
    }
}