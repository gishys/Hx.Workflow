using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    /// <summary>
    /// 模板版本差异DTO
    /// </summary>
    public class WkDefinitionDiffDto
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// 版本1
        /// </summary>
        public int Version1 { get; set; }
        
        /// <summary>
        /// 版本2
        /// </summary>
        public int Version2 { get; set; }
        
        /// <summary>
        /// 差异项列表
        /// </summary>
        public List<WkDefinitionDiffItemDto> DiffItems { get; set; } = new();
        
        /// <summary>
        /// 是否有差异
        /// </summary>
        public bool HasDifferences => DiffItems.Count > 0;
    }
    
    /// <summary>
    /// 差异项DTO
    /// </summary>
    public class WkDefinitionDiffItemDto
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName { get; set; } = string.Empty;
        
        /// <summary>
        /// 字段显示名称
        /// </summary>
        public string FieldDisplayName { get; set; } = string.Empty;
        
        /// <summary>
        /// 版本1的值
        /// </summary>
        public string? Value1 { get; set; }
        
        /// <summary>
        /// 版本2的值
        /// </summary>
        public string? Value2 { get; set; }
        
        /// <summary>
        /// 变更类型
        /// </summary>
        public DiffChangeType ChangeType { get; set; }
    }
    
    /// <summary>
    /// 变更类型枚举
    /// </summary>
    public enum DiffChangeType
    {
        /// <summary>
        /// 无变更
        /// </summary>
        None,
        
        /// <summary>
        /// 新增
        /// </summary>
        Added,
        
        /// <summary>
        /// 删除
        /// </summary>
        Removed,
        
        /// <summary>
        /// 修改
        /// </summary>
        Modified
    }
} 