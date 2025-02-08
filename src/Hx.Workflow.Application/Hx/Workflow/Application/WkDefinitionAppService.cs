using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using NUglify.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace Hx.Workflow.Application
{
    public class WkDefinitionAppService : WorkflowAppServiceBase, IWkDefinitionAppService
    {
        private readonly IWkDefinitionRespository _definitionRespository;
        private readonly IWkStepBodyRespository _wkStepBody;
        private readonly HxWorkflowManager _hxWorkflowManager;
        public WkDefinitionAppService(
            IWkDefinitionRespository definitionRespository,
            IWkStepBodyRespository wkStepBody,
            HxWorkflowManager hxWorkflowManager)
        {
            _definitionRespository = definitionRespository;
            _wkStepBody = wkStepBody;
            _hxWorkflowManager = hxWorkflowManager;
        }
        /// <summary>
        /// 创建模板
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public virtual async Task CreateAsync(WkDefinitionCreateDto input)
        {
            var sortNumber = await _definitionRespository.GetMaxSortNumberAsync();
            var entity = new WkDefinition(
                    input.Title,
                    sortNumber,
                    input.Description,
                    input.BusinessType,
                    input.ProcessType,
                    limitTime: input.LimitTime,
                    groupId: input.GroupId,
                    version: input.Version <= 0 ? 1 : input.Version);
            foreach (var candidate in input.WkCandidates)
            {
                entity.WkCandidates.Add(new DefinitionCandidate(
                    candidate.CandidateId,
                    candidate.UserName,
                    candidate.DisplayUserName,
                    candidate.ExecutorType,
                    candidate.DefaultSelection));
            }
            var nodeEntitys = input.Nodes?.ToWkNodes();
            if (nodeEntitys != null)
            {
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
            }
            input.ExtraProperties.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
            await _hxWorkflowManager.CreateAsync(entity);
        }
        /// <summary>
        /// 更新模板
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(WkDefinitionUpdateDto input)
        {
            var entity = await _definitionRespository.GetAsync(input.Id);
            await entity.SetVersion(input.Version);
            await entity.SetTitle(input.Title);
            await entity.SetLimitTime(input.LimitTime);
            await entity.SetDescription(input.Description);
            await entity.SetBusinessType(input.BusinessType);
            await entity.SetProcessType(input.ProcessType);
            await entity.SetEnabled(input.IsEnabled);
            entity.WkCandidates.Clear();
            foreach (var candidate in input.WkCandidates)
            {
                entity.WkCandidates.Add(new DefinitionCandidate(
                    candidate.CandidateId,
                    candidate.UserName,
                    candidate.DisplayUserName,
                    candidate.ExecutorType,
                    candidate.DefaultSelection));
            }
            await _hxWorkflowManager.UpdateAsync(entity);
        }
        /// <summary>
        /// 通过Id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> GetAsync(Guid id)
        {
            var entity = await _definitionRespository.GetAsync(id);
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
        }
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task DeleteAsync(Guid id)
        {
            await _definitionRespository.DeleteAsync(id);
        }
    }
}