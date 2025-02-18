using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Repositories;
using NUglify.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Hx.Workflow.Application
{
    public class WkApplicationFormAppService : WorkflowAppServiceBase, IWkApplicationFormAppService
    {
        public IWkApplicationFormRepository WkApplicationFormRepository { get; }
        public WkApplicationFormAppService(
            IWkApplicationFormRepository wkApplicationFormRepository
            )
        {
            WkApplicationFormRepository = wkApplicationFormRepository;
        }
        public virtual async Task CreateAsync(ApplicationFormCreateDto input)
        {
            var form = new ApplicationForm(
                input.Name,
                input.Title,
                input.ApplicationType,
                input.Data,
                input.ApplicationComponentType,
                input.GroupId
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
            await entity.SetName(input.Name);
            await entity.SetTitle(input.Title);
            await entity.SetApplicationType(input.ApplicationType);
            await entity.SetApplicationComponentType(input.ApplicationComponentType);
            await entity.SetData(input.Data);
            entity.ExtraProperties.Clear();
            input.ExtraProperties.ForEach(item => entity.ExtraProperties.TryAdd(item.Key, item.Value));
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