using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application
{
    [Authorize]
    public class WkApplicationFormAppService(
        IWkApplicationFormRepository wkApplicationFormRepository
            ) : WorkflowAppServiceBase, IWkApplicationFormAppService
    {
        public IWkApplicationFormRepository WkApplicationFormRepository { get; } = wkApplicationFormRepository;

        public virtual async Task CreateAsync(ApplicationFormCreateDto input)
        {
            if (await WkApplicationFormRepository.ExistByNameAsync(input.Name))
            {
                throw new UserFriendlyException(message: "表单名称已存在！");
            }
            if (await WkApplicationFormRepository.ExistByTitleAsync(input.Title, input.GroupId))
            {
                throw new UserFriendlyException(message: "组内表单标题已存在！");
            }
            var form = new ApplicationForm(
                input.Name,
                input.Title,
                input.ApplicationType,
                input.Data,
                input.ApplicationComponentType,
                input.GroupId,
                input.IsPublish,
                input.Description
                );
            if (input.Params?.Count > 0)
            {
                foreach (var param in input.Params)
                {
                    await form.AddParam(new WkParam(param.WkParamKey, param.Name, param.DisplayName, param.Value));
                }
            }
            input.ExtraProperties.ForEach(item => form.ExtraProperties.TryAdd(item.Key, item.Value));
            await WkApplicationFormRepository.InsertAsync(form);
        }
        public virtual async Task<PagedResultDto<ApplicationFormDto>> GetPagedAsync(ApplicationFormQueryInput input)
        {
            var items = await WkApplicationFormRepository.GetPagedAsync(input.Filter, input.SkipCount, input.MaxResultCount);
            var count = await WkApplicationFormRepository.GetPagedCountAsync(input.Filter);
            return new PagedResultDto<ApplicationFormDto>(count, ObjectMapper.Map<List<ApplicationForm>, List<ApplicationFormDto>>(items));
        }
        public virtual async Task UpdateAsync(ApplicationFormUpdateDto input)
        {
            var entity = await WkApplicationFormRepository.GetAsync(input.Id);
            if (!entity.Title.Equals(input.Title, StringComparison.CurrentCultureIgnoreCase))
            {
                if (await WkApplicationFormRepository.ExistByTitleAsync(input.Title, input.GroupId))
                {
                    throw new UserFriendlyException(message: "组内表单标题已存在！");
                }
            }
            if (!entity.Name.Equals(input.Name, StringComparison.CurrentCultureIgnoreCase))
            {
                if (await WkApplicationFormRepository.ExistByNameAsync(input.Name))
                {
                    throw new UserFriendlyException(message: "表单名称已存在！");
                }
            }
            await entity.SetName(input.Name);
            await entity.SetTitle(input.Title);
            await entity.SetApplicationType(input.ApplicationType);
            await entity.SetApplicationComponentType(input.ApplicationComponentType);
            await entity.SetData(input.Data);
            await entity.SetGroupId(input.GroupId);
            await entity.SetIsPublish(input.IsPublish);
            await entity.SetDescription(input.Description);
            entity.Params.Clear();
            if (input.Params?.Count > 0)
            {
                foreach (var param in input.Params)
                {
                    await entity.AddParam(new WkParam(param.WkParamKey, param.Name, param.DisplayName, param.Value));
                }
            }
            entity.ExtraProperties.Clear();
            input.ExtraProperties?.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
            await WkApplicationFormRepository.UpdateAsync(entity);
        }
        public virtual async Task DeleteAsync(Guid id)
        {
            await WkApplicationFormRepository.DeleteAsync(id);
        }
        public virtual async Task<ApplicationFormDto> GetAsync(Guid id)
        {
            return ObjectMapper.Map<ApplicationForm, ApplicationFormDto>(await WkApplicationFormRepository.GetAsync(id));
        }
    }
}