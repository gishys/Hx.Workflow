using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Hx.Workflow.Domain.Repositories
{
    public interface IWkDefinitionRespository : IBasicRepository<WkDefinition, Guid>
    {
        /// <summary>
        /// Get entity by id an version
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        Task<WkDefinition?> GetDefinitionAsync(Guid id, int version, CancellationToken cancellation = default);
        Task<WkDefinition> GetDefinitionAsync(string name, bool includeDetails = true, CancellationToken cancellationToken = default);
        Task<WkDefinition> UpdateCandidatesAsync(
            Guid wkDefinitionId,
            Guid userId,
            ICollection<DefinitionCandidate> wkCandidates);
        Task<int> GetMaxSortNumberAsync();
        Task<List<WkDefinition>> GetListHasPermissionAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
