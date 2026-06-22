using Hx.Workflow.Domain.StepBodys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Stats
{
    public interface IBusinessStatRepository : IBasicRepository<BusinessStat, Guid>
    {
        Task<BusinessStat> GetAsync(
            Guid owner,
            string statType,
            string primaryC = null,
            string secondaryC = null,
            string threeLevelC = null);
        Task<List<BusinessStat>> GetListAsync(string statType);
    }
}
