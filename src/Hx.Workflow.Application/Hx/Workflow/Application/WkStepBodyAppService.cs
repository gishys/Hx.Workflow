using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application
{
    [Authorize]
    public class WkStepBodyAppService(
        IWkStepBodyRespository wkStepBody) : WorkflowAppServiceBase, IWkStepBodyAppService
    {
        private readonly IWkStepBodyRespository _wkStepBody = wkStepBody;

        public virtual async Task CreateAsync(WkSepBodyCreateDto input)
        {
            var entity = await _wkStepBody.GetStepBodyAsync(input.Name);
            if (entity != null)
                throw new UserFriendlyException(message: "已存在相同名称的StepBody!");
            
            entity = new WkStepBody(
                    input.Name,
                    input.DisplayName,
                    input.Data,
                    [], // 先创建空的Inputs集合
                    input.TypeFullName,
                    input.AssemblyFullName);
            
            entity.ExtraProperties.Clear();
            input.ExtraProperties.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
            await _wkStepBody.InsertAsync(entity);
            
            // 保存后，为Inputs设置正确的WkStepBodyId
            if (input.Inputs != null)
            {
                var bodyParams = input.Inputs.Select(d =>
                new WkStepBodyParam(
                    GuidGenerator.Create(),
                    entity.Id, // 使用已保存的entity的Id
                    d.Key,
                    d.Name,
                    d.DisplayName,
                    d.Value,
                    d.StepBodyParaType)).ToList();
                
                await entity.UpdateInputs(bodyParams);
                await _wkStepBody.UpdateAsync(entity);
            }
        }
        public virtual async Task DeleteAsync(Guid id)
        {
            await _wkStepBody.DeleteAsync(id);
        }
        public virtual async Task<WkStepBodyDto?> GetStepBodyAsync(string name)
        {
            var entity = await _wkStepBody.GetStepBodyAsync(name);
            return ObjectMapper.Map<WkStepBody?, WkStepBodyDto?>(entity);
        }
        public virtual async Task<PagedResultDto<WkStepBodyDto>> GetPagedAsync(WkStepBodyPagedInput input)
        {
            var items = await _wkStepBody.GetPagedAsync(input.Filter, input.SkipCount, input.MaxResultCount);
            var count = await _wkStepBody.GetPagedCountAsync(input.Filter);
            return new PagedResultDto<WkStepBodyDto>(count, ObjectMapper.Map<List<WkStepBody>, List<WkStepBodyDto>>(items));
        }
        public virtual async Task UpdateAsync(WkStepBodyUpdateDto input)
        {
            var entity = await _wkStepBody.FindAsync(input.Id) ?? throw new UserFriendlyException(message: $"Id为：[{input.Id}]的stepbody不存在!");
            await entity.SetName(input.Name);
            await entity.SetDisplayName(input.DisplayName);
            await entity.SetData(input.Data);
            await entity.SetTypeFullName(input.TypeFullName);
            await entity.SetAssemblyFullName(input.AssemblyFullName);
            
            if (input.Inputs != null)
            {
                var bodyParams = input.Inputs.Select(d =>
                new WkStepBodyParam(
                    GuidGenerator.Create(),
                    entity.Id, // 使用entity的Id作为WkStepBodyId
                    d.Key,
                    d.Name,
                    d.DisplayName,
                    d.Value,
                    d.StepBodyParaType)).ToList();
                await entity.UpdateInputs(bodyParams);
            }
            
            entity.ExtraProperties.Clear();
            input.ExtraProperties.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
            await _wkStepBody.UpdateAsync(entity);
        }
        public virtual async Task<List<WkStepBodyDto>> GetAllAsync()
        {
            var entity = await _wkStepBody.GetListAsync();
            return ObjectMapper.Map<List<WkStepBody>, List<WkStepBodyDto>>(entity);
        }
    }
}