using System;
using System.Collections.Generic;

namespace Hx.Workflow.Domain
{
    /// <summary>
    /// Group 基础类型接口
    /// </summary>
    /// <typeparam name="ItemEntity"></typeparam>
    public interface IGroupBaseEntity<ItemEntity>
    {
        /// <summary>
        /// 分组标题
        /// </summary>
        public string Title { get;}
        /// <summary>
        /// 路径枚举
        /// </summary>
        public string Code { get; }
        /// <summary>
        /// 分组排序
        /// </summary>
        public double Order { get; }
        /// <summary>
        /// 父Id
        /// </summary>
        public Guid? ParentId { get;}
        /// <summary>
        /// 分组描述
        /// </summary>
        public string? Description { get; }
        /// <summary>
        /// Get multi tenant id
        /// </summary>
        public Guid? TenantId { get;}
        /// <summary>
        /// 子组
        /// </summary>
        public List<IGroupBaseEntity<ItemEntity>> Children { get;}
        /// <summary>
        /// 表单
        /// </summary>
        public List<ItemEntity> Items { get; }
    }
}
