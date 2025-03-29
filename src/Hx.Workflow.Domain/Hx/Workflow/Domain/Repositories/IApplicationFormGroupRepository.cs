using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IApplicationFormGroupRepository : IBasicRepository<ApplicationFormGroup, Guid>, IGroupRepository
    {
        /// <summary>
        /// 判断是否存在同一标题的组
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        Task<bool> ExistByTitleAsync(string title);
        /// <summary>
        /// 通过id获取实体携带children
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApplicationFormGroup?> GetByIdAsync(Guid id);
        /// <summary>
        /// 获取所有节点，包含子节点
        /// </summary>
        /// <returns></returns>
        Task<List<ApplicationFormGroup>> GetAllWithChildrenAsync();
    }
}
