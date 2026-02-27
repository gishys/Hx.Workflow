using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Stats;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkExecutionPointerRepository : IBasicRepository<WkExecutionPointer, Guid>
    {
        /// <summary>
        /// 标记初始化物料
        /// </summary>
        /// <param name="wkinstanceId"></param>
        /// <param name="executionPointerId"></param>
        /// <returns></returns>
        Task InitMaterialsAsync(Guid executionPointerId);
        Task UpdateDataAsync(Guid id, Dictionary<string, string> data);
        Task RetryAsync(Guid id);
        Task<List<WkExecutionPointer>> GetListAsync(Guid wkInstanceId, CancellationToken cancellationToken = default);
        /// <summary>节点/步骤处理时长统计</summary>
        Task<List<StepDurationStat>> GetStepDurationStatListAsync(Guid definitionId, int version, DateTime? startTime, DateTime? endTime, Guid? tenantId);
    }
}
