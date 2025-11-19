using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hx.Workflow.Application.Contracts
{
    public interface IWkDefinitionAppService
    {
        Task CreateAsync(WkDefinitionCreateDto input);
        /// <summary>
        /// 更新模板
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<WkDefinitionDto> UpdateAsync(WkDefinitionUpdateDto input);
        /// <summary>
        /// 更新模板
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<List<WkNodeDto>> UpdateAsync(DefinitionNodeUpdateDto input);
        /// <summary>
        /// 通过Id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<WkDefinitionDto?> GetAsync(Guid id);
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid id);
        Task<WkDefinitionDto> GetDefinitionAsync(Guid id, int version = 1);
        
        /// <summary>
        /// 获取模板的所有版本
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns></returns>
        Task<List<WkDefinitionDto>> GetVersionsAsync(Guid id);
        
        /// <summary>
        /// 回滚到指定版本
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="targetVersion">目标版本</param>
        /// <returns></returns>
        Task<WkDefinitionDto> RollbackToVersionAsync(Guid id, int targetVersion);
        
        /// <summary>
        /// 比较两个版本的差异
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="version1">版本1</param>
        /// <param name="version2">版本2</param>
        /// <returns></returns>
        Task<WkDefinitionDiffDto> CompareVersionsAsync(Guid id, int version1, int version2);
        
        /// <summary>
        /// 获取版本历史
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns></returns>
        Task<List<WkDefinitionVersionHistoryDto>> GetVersionHistoryAsync(Guid id);
    }
}
