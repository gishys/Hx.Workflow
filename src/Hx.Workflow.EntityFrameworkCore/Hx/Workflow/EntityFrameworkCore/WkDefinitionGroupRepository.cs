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
    public class WkDefinitionGroupRepository
        : EfCoreRepository<WkDbContext, WkDefinitionGroup, Guid>
        , IWkDefinitionGroupRepository
    {
        public WkDefinitionGroupRepository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
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
        /// <returns></returns>
        public async Task<List<WkDefinitionGroup>> GetAllWithChildrenAsync(Boolean includeDetails)
        {
            var sql = @"
    WITH RECURSIVE RecursiveGroups AS (
        SELECT * FROM ""HXWKDEFINITION_GROUPS"" WHERE ""PARENT_ID"" IS NULL and ""ISDELETED""=false
        UNION ALL
        select g.* from (SELECT * FROM ""HXWKDEFINITION_GROUPS""  where ""ISDELETED""=false) g
        INNER JOIN RecursiveGroups rg ON g.""PARENT_ID"" = rg.""ID""
    )
    SELECT * FROM RecursiveGroups
";
            var dbSet = await GetDbSetAsync();
            List<WkDefinitionGroup> groups = await dbSet.FromSqlRaw(sql)
                .Include(d => d.Definitions)
                .ToListAsync();
            return groups.Where(d => d.ParentId == null).ToList();
        }
    }
}
