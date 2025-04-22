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
    public class WkStepBodyAppService : WorkflowAppServiceBase, IWkStepBodyAppService
    {
        private readonly IWkStepBodyRespository _wkStepBody;
        public WkStepBodyAppService(
            IWkStepBodyRespository wkStepBody)
        {
            _wkStepBody = wkStepBody;
        }
        public virtual async Task CreateAsync(WkSepBodyCreateDto input)
        {
            var bodyParams = input.Inputs?.Select(d =>
            new WkStepBodyParam(
                GuidGenerator.Create(),
                d.Key,
                d.Name,
                d.DisplayName,
                d.Value,
                d.StepBodyParaType)).ToList();
            var entity = await _wkStepBody.GetStepBodyAsync(input.Name);
            if (entity != null)
                throw new UserFriendlyException("已存在相同名称的StepBody!");
            entity.ExtraProperties.Clear();
            input.ExtraProperties.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
            await _wkStepBody.InsertAsync(new WkStepBody(
                    input.Name,
                    input.DisplayName,
                    input.Data,
                    bodyParams,
                    input.TypeFullName,
                    input.AssemblyFullName));
        }
        public virtual async Task DeleteAsync(Guid id)
        {
            await _wkStepBody.DeleteAsync(id);
        }
        public virtual async Task<WkStepBodyDto> GetStepBodyAsync(string name)
        {
            var entity = await _wkStepBody.GetStepBodyAsync(name);
            return ObjectMapper.Map<WkStepBody, WkStepBodyDto>(entity);
        }
        public virtual async Task<PagedResultDto<WkStepBodyDto>> GetPagedAsync(WkStepBodyPagedInput input)
        {
            var items = await _wkStepBody.GetPagedAsync(input.Filter, input.SkipCount, input.MaxResultCount);
            var count = await _wkStepBody.GetPagedCountAsync(input.Filter);
            return new PagedResultDto<WkStepBodyDto>(count, ObjectMapper.Map<List<WkStepBody>, List<WkStepBodyDto>>(items));
        }
        public virtual async Task UpdateAsync(WkStepBodyUpdateDto input)
        {
            var entity = await _wkStepBody.FindAsync(input.Id);
            await entity.SetName(input.Name);
            await entity.SetDisplayName(input.Name);
            await entity.SetData(input.Name);
            await entity.SetTypeFullName(input.Name);
            await entity.SetAssemblyFullName(input.Name);
            var bodyParams = input.Inputs?.Select(d =>
            new WkStepBodyParam(
                GuidGenerator.Create(),
                d.Key,
                d.Name,
                d.DisplayName,
                d.Value,
                d.StepBodyParaType)).ToList();
            await entity.UpdateInputs(bodyParams);
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