using System;
using System.Collections.Generic;

namespace Hx.Workflow.Application.Contracts
{
    public class ApplicationFormGroupDto
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 分组标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 路径枚举
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 分组排序
        /// </summary>
        public double Order { get; set; }
        /// <summary>
        /// 父Id
        /// </summary>
        public Guid? ParentId { get; set; }
        /// <summary>
        /// 分组描述
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Get multi tenant id
        /// </summary>
        public Guid? TenantId { get; set; }
        /// <summary>
        /// 子组
        /// </summary>
        public List<ApplicationFormGroupDto> Children { get; set; }
        /// <summary>
        /// 一组模板
        /// </summary>
        public List<ApplicationFormDto> Items { get; set; }
    }
}
