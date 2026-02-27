using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Stats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkErrorRepository : IBasicRepository<WkExecutionError, Guid>
    {
        Task<List<WkExecutionError>> GetListByIdAsync(Guid? workflowId, Guid? pointerId);
        /// <summary>错误汇总（总数、涉及实例数）</summary>
        Task<ErrorSummaryStat> GetErrorSummaryAsync(DateTime? startTime, DateTime? endTime, Guid? definitionId, Guid? tenantId);
        /// <summary>错误统计按定义分组</summary>
        Task<List<ErrorStat>> GetErrorStatByDefinitionAsync(DateTime? startTime, DateTime? endTime, Guid? definitionId, Guid? tenantId);
        /// <summary>错误统计按节点分组</summary>
        Task<List<ErrorByStepStat>> GetErrorByStepStatListAsync(DateTime? startTime, DateTime? endTime, Guid? definitionId, Guid? tenantId);
    }
}
