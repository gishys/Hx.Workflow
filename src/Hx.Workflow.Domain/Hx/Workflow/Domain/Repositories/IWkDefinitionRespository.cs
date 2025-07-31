using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkDefinitionRespository : IBasicRepository<WkDefinition>
    {
        /// <summary>
        /// Get entity by id an version
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<WkDefinition?> GetDefinitionAsync(Guid id, int version, CancellationToken cancellation = default);
        Task<WkDefinition?> GetDefinitionAsync(string name, bool includeDetails = true, CancellationToken cancellationToken = default);
        Task<WkDefinition?> UpdateCandidatesAsync(
            Guid wkDefinitionId,
            Guid userId,
            ICollection<DefinitionCandidate> wkCandidates);
        Task<int> GetMaxSortNumberAsync();
        Task<List<WkDefinition>> GetListHasPermissionAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<WkNodeCandidate>> GetCandidatesAsync(Guid id, string stepName, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 获取指定模板的最大版本号
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>最大版本号，如果没有版本则返回0</returns>
        Task<int> GetMaxVersionAsync(Guid definitionId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 获取指定模板的所有版本
        /// </summary>
        /// <param name="definitionId">模板ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>所有版本列表</returns>
        Task<List<WkDefinition>> GetAllVersionsAsync(Guid definitionId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// 检查指定ID和版本是否存在
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="version">版本号</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(Guid id, int version, CancellationToken cancellationToken = default);
        Task<WkDefinition?> FindAsync(
            Guid id, bool includeDetails = true, CancellationToken cancellation = default);
    }
}
