using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hx.Workflow.Domain
{
    public class WkStepBody : FullAuditedEntity<Guid>, IHasExtraProperties
    {
        public virtual string Name { get; protected set; }
        public virtual string DisplayName { get; protected set; }
        public virtual ICollection<WkStepBodyParam> Inputs { get; protected set; }
        public virtual string TypeFullName { get; protected set; }
        public virtual string AssemblyFullName { get; protected set; }
        public virtual string? Data { get; protected set; }
        /// <summary>
        /// 扩展属性
        /// </summary>
        public virtual ExtraPropertyDictionary ExtraProperties { get; protected set; }
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        public WkStepBody()
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        { }
        public WkStepBody(
            string name,
            string displayName,
            string? data,
            ICollection<WkStepBodyParam> inputs,
            string typeFullName,
            string assemblyFullName)
        {
            Name = name;
            DisplayName = displayName;
            Data = data;
            Inputs = inputs;
            TypeFullName = typeFullName;
            AssemblyFullName = assemblyFullName;
            ExtraProperties = [];
        }
        public Task UpdateInputs(ICollection<WkStepBodyParam>? wkParams)
        {
            Inputs = wkParams ?? ([]);
            return Task.CompletedTask;
        }
        public Task SetName(string name)
        {
            Name = name;
            return Task.CompletedTask;
        }
        public Task SetDisplayName(string displayName)
        {
            DisplayName = displayName;
            return Task.CompletedTask;
        }
        public Task SetData(string? data)
        {
            Data = data;
            return Task.CompletedTask;
        }
        public Task SetTypeFullName(string typeFullName)
        {
            TypeFullName = typeFullName;
            return Task.CompletedTask;
        }
        public Task SetAssemblyFullName(string assemblyFullName)
        {
            AssemblyFullName = assemblyFullName;
            return Task.CompletedTask;
        }
    }
}
