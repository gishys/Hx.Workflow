using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkDefinitionGroupRepository(IDbContextProvider<WkDbContext> options)
                : EfCoreRepository<WkDbContext, WkDefinitionGroup, Guid>(options)
        , IWkDefinitionGroupRepository
    {
        /// <summary>
        /// 判断是否存在同一标题的组
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public virtual async Task<bool> ExistByTitleAsync(string title)
        {
            return await (await GetDbSetAsync()).AnyAsync(x => x.Title == title && !x.IsDeleted);
        }
        /// <summary>
        /// 通过id获取实体携带children
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionGroup?> GetByIdAsync(Guid id)
        {
            return await (await GetDbSetAsync()).Include(d => d.Children).FirstOrDefaultAsync(x => x.Id == id);
        }
        /// <summary>
        /// 获取某分类最大排序值
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public virtual async Task<double> GetMaxOrderNumberAsync(Guid? parentId)
        {
            var dbSet = await GetDbSetAsync();
            var query = dbSet
                .Where(d => d.ParentId == parentId);
            if (!await query.AnyAsync().ConfigureAwait(false))
            {
                return 0;
            }
            double maxNumber = await query
                .MaxAsync(d => d.Order)
                .ConfigureAwait(false);
            return maxNumber;
        }
        /// <summary>
        /// 获取某分类下 Code 字段最后一个部分的最大值
        /// </summary>
        /// <param name="parentId">父分类ID</param>
        /// <returns>最后一个部分的最大值对应的原始 Code，如果没有记录则返回 "00001"</returns>
        public virtual async Task<string> GetMaxCodeNumberAsync(Guid? parentId)
        {
            var dbSet = await GetDbSetAsync();
            List<string> codes = await dbSet
                .Where(d => d.ParentId == parentId)
                .Select(d => d.Code)
                .ToListAsync();
            if (codes.Count == 0)
            {
                return WkDefinitionGroup.CreateCode([0]);
            }
            string? maxCode = codes
                .OrderByDescending(code =>
                {
                    string[] parts = code.Split('.');
                    if (parts.Length == 0 || !double.TryParse(parts.Last(), out double value))
                    {
                        return double.MinValue;
                    }
                    return value;
                })
                .FirstOrDefault();
            if (string.IsNullOrEmpty(maxCode))
            {
                return WkDefinitionGroup.CreateCode([0]);
            }
            return maxCode;
        }
        /// <summary>
        /// 获取所有节点，包含子节点
        /// </summary>
        /// <param name="includeDetails">是否包含详细信息</param>
        /// <param name="includeArchived">是否包含已归档的模板定义，默认不包含</param>
        /// <returns></returns>
        public async Task<List<WkDefinitionGroup>> GetAllWithChildrenAsync(bool includeDetails, bool includeArchived = false)
        {
            var dbContext = await GetDbContextAsync();
            var groupDbSet = await GetDbSetAsync();
            var definitionDbSet = dbContext.Set<WkDefinition>();

            // 先查询所有组（不包含 Items）
            var groups = await groupDbSet.ToListAsync();

            // 在数据库层面直接查询每个工作流定义的最新版本
            // 使用子查询来获取每个 Id 的最大版本，然后查询完整数据
            // 注意：IncludeDetails 必须在 Select 之前调用
            var query = definitionDbSet
                .IncludeDetails(includeDetails);
            
            if (includeArchived)
            {
                // 包含已归档版本：查询所有版本的最新版本
                query = query.Where(d =>
                    d.Version == definitionDbSet
                        .Where(d2 => d2.Id == d.Id)
                        .Max(d2 => (int?)d2.Version));
            }
            else
            {
                // 不包含已归档版本：只查询未归档版本的最新版本
                query = query.Where(d =>
                    !d.IsArchived &&
                    d.Version == definitionDbSet
                        .Where(d2 => d2.Id == d.Id && !d2.IsArchived)
                        .Max(d2 => (int?)d2.Version));
            }
            
            var latestVersionDefinitions = await query.ToListAsync();

            // 按 GroupId 分组（过滤掉 GroupId 为 null 的记录）
            var itemsByGroupId = latestVersionDefinitions?
                .Where(d => d.GroupId.HasValue)
                .GroupBy(d => d.GroupId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 将 Items 分配给对应的 Group
            foreach (var group in groups)
            {
                if (itemsByGroupId != null && itemsByGroupId.TryGetValue(group.Id, out var items))
                {
                    group.SetItems(items);
                }
                else
                {
                    group.SetItems([]);
                }
            }

            return groups;
        }
    }
}
