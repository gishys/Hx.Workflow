using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class ApplicationForm : Entity<Guid>, IHasExtraProperties
    {
        public Guid? GroupId { get; set; }
        /// <summary>
        /// application name
        /// </summary>
        public virtual string Name { get; protected set; }
        /// <summary>
        /// application node type
        /// </summary>
        public virtual string Title { get; protected set; }
        /// <summary>
        /// application type
        /// </summary>
        public virtual ApplicationType ApplicationType { get; protected set; }
        /// <summary>
        /// 应用数据
        /// </summary>
        public virtual string? Data { get; protected set; }
        /// <summary>
        /// 应用组件类型
        /// </summary>
        public virtual ApplicationComponentType ApplicationComponentType { get; protected set; }
        /// <summary>
        /// 扩展属性
        /// </summary>
        public virtual ExtraPropertyDictionary ExtraProperties { get; protected set; }
        /// <summary>
        /// 是否发布
        /// </summary>
        public virtual bool IsPublish { get; protected set; }
        /// <summary>
        /// 描述
        /// </summary>
        public virtual string? Description { get; protected set; }
        public virtual ICollection<WkParam> Params { get; protected set; } = [];
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public ApplicationForm()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        { }
        public ApplicationForm(
            string name,
            string title,
            ApplicationType applicationType,
            string? data,
            ApplicationComponentType applicationComponentType,
            Guid? groupId,
            bool isPublish,
            string? description
            )
        {
            Name = name;
            Title = title;
            ApplicationType = applicationType;
            ApplicationComponentType = applicationComponentType;
            Data = data;
            GroupId = groupId;
            IsPublish = isPublish;
            Description = description;
            ExtraProperties = [];
        }
        public Task SetName(string name)
        {
            Name = name;
            return Task.CompletedTask;
        }
        public Task SetTitle(string title)
        {
            Title = title;
            return Task.CompletedTask;
        }
        public Task SetApplicationType(ApplicationType applicationType)
        {
            ApplicationType = applicationType;
            return Task.CompletedTask;
        }
        public Task SetData(string? data)
        {
            Data = data;
            return Task.CompletedTask;
        }
        public Task SetGroupId(Guid groupId)
        {
            GroupId = groupId;
            return Task.CompletedTask;
        }
        public Task SetApplicationComponentType(ApplicationComponentType applicationComponentType)
        {
            ApplicationComponentType = applicationComponentType;
            return Task.CompletedTask;
        }
        public Task AddParam(WkParam param)
        {
            Params.Add(param);
            return Task.CompletedTask;
        }
        public Task SetIsPublish(bool isPublish)
        {
            IsPublish = isPublish;
            return Task.CompletedTask;
        }
        public Task SetDescription(string? description)
        {
            Description = description;
            return Task.CompletedTask;
        }
    }
}