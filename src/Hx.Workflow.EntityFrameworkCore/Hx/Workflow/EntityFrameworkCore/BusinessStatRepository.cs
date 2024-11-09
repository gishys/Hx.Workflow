using Hx.Workflow.Domain.Stats;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    public class BusinessStatRepository
        : EfCoreRepository<WkDbContext, BusinessStat, Guid>,
        IBusinessStatRepository
    {
        public BusinessStatRepository(IDbContextProvider<WkDbContext> options)
            : base(options)
        { }
        public virtual async Task<BusinessStat> GetAsync(
            Guid owner,
            string statType,
            string primaryC = null,
            string secondaryC = null,
            string threeLevelC = null)
        {
            return await (await GetDbSetAsync())
                .Where(d => d.Owner == owner && d.StatType == statType)

                .FirstOrDefaultAsync();
        }
        public virtual async Task<List<BusinessStat>> GetListAsync(string statType)
        {
            return await (await GetDbSetAsync()).Where(d => d.StatType == statType).ToListAsync();
        }
    }
}
