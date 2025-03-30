using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
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
    }
}
