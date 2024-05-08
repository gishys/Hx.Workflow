using Hx.Workflow.Domain.Shared;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities;

namespace Hx.Workflow.Domain
{
    public class ApplicationForm : Entity<Guid>
    {
        public Guid? WkNodeId { get; protected set; }
        public WkNode WkNode { get; protected set; }
        /// <summary>
        /// if null is root
        /// </summary>
        public Guid? ParentId { get; protected set; }
        /// <summary>
        /// example:001.001.001
        /// </summary>
        public string Code { get; protected set; }
        /// <summary>
        /// application name
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// application node type
        /// </summary>
        public string DispalyName { get; protected set; }
        /// <summary>
        /// application type
        /// </summary>
        public ApplicationType ApplicationType { get;protected set; }
        public ApplicationForm()
        { }
        public ApplicationForm(
            Guid? parentId,
            string code,
            string name,
            string dispalyName,
            ApplicationType applicationType)
        {
            ParentId = parentId;
            Code = code;
            Name = name;
            DispalyName = dispalyName;
            ApplicationType = applicationType;
        }
        public Task SetName(string name)
        {
            Name = name;
            return Task.CompletedTask;
        }
    }
}
