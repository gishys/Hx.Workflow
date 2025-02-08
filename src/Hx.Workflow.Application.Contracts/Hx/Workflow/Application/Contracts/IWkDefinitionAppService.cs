using System;
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
        Task UpdateAsync(WkDefinitionUpdateDto input);
        /// <summary>
        /// 通过Id获取实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<WkDefinitionDto> GetAsync(Guid id);
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid id);
    }
}
