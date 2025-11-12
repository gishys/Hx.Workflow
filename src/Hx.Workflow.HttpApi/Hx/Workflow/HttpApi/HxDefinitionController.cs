using Hx.Workflow.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Hx.Workflow.HttpApi
{
    [ApiController]
    [Route("/hxworkflow/hxdefinition")]
    public class HxDefinitionController(IWkDefinitionAppService appService) : AbpController
    {
        private readonly IWkDefinitionAppService _appService = appService;

        [HttpPost]
        public Task CreateAsync(WkDefinitionCreateDto input)
        {
            return _appService.CreateAsync(input);
        }
        [HttpPut]
        public Task<WkDefinitionDto> UpdateAsync(WkDefinitionUpdateDto input)
        {
            return _appService.UpdateAsync(input);
        }
        [HttpPut]
        [Route("nodes")]
        public Task<List<WkNodeDto>> UpdateAsync(DefinitionNodeUpdateDto input)
        {
            return _appService.UpdateAsync(input);
        }
        [HttpGet]
        public Task<WkDefinitionDto?> GetAsync(Guid id)
        {
            return _appService.GetAsync(id);
        }
        [HttpDelete]
        public Task DeleteAsync(Guid id)
        {
            return _appService.DeleteAsync(id);
        }
        
        /// <summary>
        /// 删除指定版本
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("version")]
        public Task DeleteVersionAsync(Guid id, int version)
        {
            return _appService.DeleteVersionAsync(id, version);
        }
        
        /// <summary>
        /// 删除所有版本
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("all-versions")]
        public Task DeleteAllVersionsAsync(Guid id)
        {
            return _appService.DeleteAllVersionsAsync(id);
        }
        
        /// <summary>
        /// 删除旧版本
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="keepCount">保留的最新版本数量</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("old-versions")]
        public Task DeleteOldVersionsAsync(Guid id, int keepCount = 5)
        {
            return _appService.DeleteOldVersionsAsync(id, keepCount);
        }
        [HttpGet]
        [Route("details")]
        public Task<WkDefinitionDto> GetAsync(Guid id, int version = 1)
        {
            return _appService.GetDefinitionAsync(id, version);
        }
        
        /// <summary>
        /// 获取模板的所有版本
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("versions")]
        public Task<List<WkDefinitionDto>> GetVersionsAsync(Guid id)
        {
            return _appService.GetVersionsAsync(id);
        }
        
        /// <summary>
        /// 回滚到指定版本
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="targetVersion">目标版本</param>
        /// <returns></returns>
        [HttpPost]
        [Route("rollback")]
        public Task<WkDefinitionDto> RollbackToVersionAsync(Guid id, int targetVersion)
        {
            return _appService.RollbackToVersionAsync(id, targetVersion);
        }
        
        /// <summary>
        /// 比较两个版本的差异
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="version1">版本1</param>
        /// <param name="version2">版本2</param>
        /// <returns></returns>
        [HttpGet]
        [Route("compare")]
        public Task<WkDefinitionDiffDto> CompareVersionsAsync(Guid id, int version1, int version2)
        {
            return _appService.CompareVersionsAsync(id, version1, version2);
        }
        
        /// <summary>
        /// 获取版本历史
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("history")]
        public Task<List<WkDefinitionVersionHistoryDto>> GetVersionHistoryAsync(Guid id)
        {
            return _appService.GetVersionHistoryAsync(id);
        }
    }
}
