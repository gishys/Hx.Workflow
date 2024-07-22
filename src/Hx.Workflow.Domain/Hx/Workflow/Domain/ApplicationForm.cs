using Hx.Workflow.Domain.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class ApplicationForm : Entity<Guid>
    {
        public virtual Guid? WkNodeId { get; protected set; }
        public virtual WkNode WkNode { get; protected set; }
        /// <summary>
        /// if null is root
        /// </summary>
        public virtual Guid? ParentId { get; protected set; }
        /// <summary>
        /// example:001.001.001
        /// </summary>
        public virtual string Code { get; protected set; }
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
        public virtual int SequenceNumber { get; protected set; }
        public virtual ICollection<WkParam> Params { get; protected set; } = new List<WkParam>();
        public ApplicationForm()
        { }
        public ApplicationForm(
            Guid? parentId,
            string code,
            string name,
            string displayName,
            ApplicationType applicationType,
            int sequenceNumber)
        {
            ParentId = parentId;
            Code = code;
            Name = name;
            DisplayName = displayName;
            ApplicationType = applicationType;
            SequenceNumber = sequenceNumber;

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