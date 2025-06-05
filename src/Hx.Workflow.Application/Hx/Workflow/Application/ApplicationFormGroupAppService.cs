using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;

namespace Hx.Workflow.Application
{
    [Authorize]
    public class ApplicationFormGroupAppService : HxWorkflowAppServiceBase, IApplicationFormGroupAppService
    {
        private IApplicationFormGroupRepository GroupRepository { get; }
        private ApplicationFormGroupManager GroupManager { get; }
        public ApplicationFormGroupAppService(
            IApplicationFormGroupRepository definitionGroupRepository,
            ApplicationFormGroupManager groupManager)
        {
            GroupRepository = definitionGroupRepository;
            GroupManager = groupManager;
        }
        public async virtual Task CreateAsync(ApplicationFormGroupCreateDto dto)
        {
            if (await GroupRepository.ExistByTitleAsync(dto.Title))
            {
                throw new UserFriendlyException(message: "已存在相同标题的模板组！");
            }
            var orderNumber = await GroupManager.GetNextOrderNumberAsync(dto.ParentId);
            var code = await GroupManager.GetNextCodeAsync(dto.ParentId);
            var entity = new ApplicationFormGroup(GuidGenerator.Create(), dto.Title, code, orderNumber, dto.ParentId, dto.Description);
            await GroupRepository.InsertAsync(entity);
        }
        public async virtual Task UpdateAsync(ApplicationFormGroupUpdateDto dto)
        {
            var entity = await GroupRepository.GetAsync(dto.Id);
            if (!string.Equals(entity.Title, dto.Title, StringComparison.OrdinalIgnoreCase))
            {
                entity.SetTitle(dto.Title);
            }
            if (!string.Equals(entity.Description, dto.Description, StringComparison.OrdinalIgnoreCase))
            {
                entity.SetDescription(dto.Description);
            }
            await GroupRepository.UpdateAsync(entity);
        }
        public async virtual Task DeleteAsync(Guid id)
        {
            await GroupRepository.DeleteAsync(id);
        }
        public async virtual Task<List<ApplicationFormGroupDto>> GetAllWithChildrenAsync()
        {
            List<ApplicationFormGroup> list = await GroupRepository.GetAllWithChildrenAsync();
            return ObjectMapper.Map<List<ApplicationFormGroup>, List<ApplicationFormGroupDto>>(list);
        }
    }
}