using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.StepBodys;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkAuditorRespository : IBasicRepository<WkAuditor, Guid>
    {
        Task<bool> IsVerifyAsync(
            Guid executionPointerId,
            Guid userId,
            EnumAuditStatus status);
    }
}
