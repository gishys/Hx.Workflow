using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace Hx.Workflow.Application
{
    //[Authorize]
    public class WkDefinitionGroupAppService : HxWorkflowAppServiceBase, IWkDefinitionGroupAppService
    {
        private IWkDefinitionGroupRepository GroupRepository { get; }
        private DefinitionGroupManager DefinitionGroupManager { get; }
        public WkDefinitionGroupAppService(IWkDefinitionGroupRepository definitionGroupRepository, DefinitionGroupManager definitionGroupManager)
        {
            GroupRepository = definitionGroupRepository;
            DefinitionGroupManager = definitionGroupManager;
        }
        public async virtual Task CreateAsync(WkDefinitionGroupCreateDto dto)
        {
            if (await GroupRepository.ExistByTitleAsync(dto.Title))
            {
                throw new UserFriendlyException("已存在相同标题的模板组！");
            }
            var orderNumber = await DefinitionGroupManager.GetNextOrderNumberAsync(dto.ParentId);
            var code = await DefinitionGroupManager.GetNextCodeAsync(dto.ParentId);
            var entity = new WkDefinitionGroup(GuidGenerator.Create(), dto.Title, code, orderNumber, dto.ParentId, dto.Description);
            await GroupRepository.InsertAsync(entity);
        }
        public async virtual Task UpdateAsync(WkDefinitionGroupUpdateDto dto)
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
        public async virtual Task<List<WkDefinitionGroupDto>> GetAllWithChildrenAsync()
        {
            List<WkDefinitionGroup> list = await GroupRepository.GetAllWithChildrenAsync(true);
            list = list.OrderBy(x => x.Order).ToList();
            return ObjectMapper.Map<List<WkDefinitionGroup>, List<WkDefinitionGroupDto>>(list);
        }
    }
}