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
        /// <summary>
        /// application name
        /// </summary>
        public virtual string Name { get; protected set; }
        /// <summary>
        /// application node type
        /// </summary>
        public virtual string DisplayName { get; protected set; }
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
        public virtual int SequenceNumber { get; protected set; }
        public virtual ICollection<WkParam> Params { get; protected set; } = new List<WkParam>();
        public ApplicationForm()
        { }
        public ApplicationForm(
            string name,
            string displayName,
            ApplicationType applicationType,
            string data,
            ApplicationComponentType applicationComponentType,
            int sequenceNumber)
        {
            Name = name;
            DisplayName = displayName;
            ApplicationType = applicationType;
            SequenceNumber = sequenceNumber;
            ApplicationComponentType = applicationComponentType;
            Data = data;
        }
        public Task SetName(string name)
        {
            Name = name;
            return Task.CompletedTask;
        }
        public Task AddParam(WkParam param)
        {
            Params.Add(param);
            return Task.CompletedTask;
        }
    }
}