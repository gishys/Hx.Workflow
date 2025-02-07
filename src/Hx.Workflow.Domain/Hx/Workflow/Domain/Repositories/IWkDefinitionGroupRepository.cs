using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkDefinitionGroupRepository : IBasicRepository<WkDefinitionGroup, Guid>
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
        Task<WkDefinitionGroup?> GetByIdAsync(Guid id);
        /// <summary>
        /// 获取某分类最大排序值
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        Task<double> GetMaxOrderNumberAsync(Guid? parentId);
        /// <summary>
        /// 获取某分类下 Code 字段最后一个部分的最大值
        /// </summary>
        /// <param name="parentId">父分类ID</param>
        /// <returns>最后一个部分的最大值对应的原始 Code，如果没有记录则返回 "00001"</returns>
        Task<string> GetMaxCodeNumberAsync(Guid? parentId);
        /// <summary>
        /// 获取所有节点，包含子节点
        /// </summary>
        /// <returns></returns>
        Task<List<WkDefinitionGroup>> GetAllWithChildrenAsync(Boolean includeDetails);
    }
}
