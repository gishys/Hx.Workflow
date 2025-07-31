using System;

namespace Hx.Workflow.Application.Contracts
{
    /// <summary>
    /// 模板版本历史DTO
    /// </summary>
    public class WkDefinitionVersionHistoryDto
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public int Version { get; set; }
        
        /// <summary>
        /// 模板标题
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        
        /// <summary>
        /// 创建人
        /// </summary>
        public string? CreatorName { get; set; }
        
        /// <summary>
        /// 版本描述
        /// </summary>
        public string? VersionDescription { get; set; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// 是否有正在运行的实例
        /// </summary>
        public bool HasRunningInstances { get; set; }
        
        /// <summary>
        /// 实例数量
        /// </summary>
        public int InstanceCount { get; set; }
        
        /// <summary>
        /// 变更摘要
        /// </summary>
        public string? ChangeSummary { get; set; }
    }
} 