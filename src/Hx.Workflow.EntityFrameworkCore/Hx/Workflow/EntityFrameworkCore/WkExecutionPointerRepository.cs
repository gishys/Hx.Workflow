using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class WkExecutionPointerRepository
        : EfCoreRepository<WkDbContext, WkExecutionPointer, Guid>,
        IWkExecutionPointerRepository
    {
        public WkExecutionPointerRepository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        /// <summary>
        /// 标记初始化物料
        /// </summary>
        /// <param name="wkinstanceId"></param>
        /// <param name="executionPointerId"></param>
        /// <returns></returns>
        public virtual async Task InitMaterialsAsync(Guid executionPointerId)
        {
            var dbSet = await GetDbSetAsync();
            var updateEntity = await dbSet
                .FirstOrDefaultAsync(d => d.Id == executionPointerId);
            if (updateEntity != null)
            {
                await updateEntity.InitMaterials();
                await UpdateAsync(updateEntity);
            }
        }
        public override async Task<WkExecutionPointer> FindAsync(Guid id, bool includeDetails = true, CancellationToken cancellationToken = default)
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Include(d => d.WkCandidates)
                .Include(d => d.ExtensionAttributes)
                .FirstAsync(d => d.Id == id, cancellationToken: cancellationToken);
        }
        public async Task UpdateDataAsync(Guid id, Dictionary<string, string> data)
        {
            var entity = await FindAsync(id);
            foreach (var item in data)
            {
                entity.ExtensionAttributes.RemoveAll(d => d.AttributeKey == item.Key);
                var persistedAttr = new WkExtensionAttribute(item.Key, item.Value);
                await entity.SetExtensionAttributes(persistedAttr);
            }
            await UpdateAsync(entity);
        }
    }
}
