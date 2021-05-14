using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace Hx.Workflow.Domain.Obsolete
{
    public class WkInstance_Obsolete : FullAuditedEntity<Guid>
    {
        /// <summary>
        /// Get process instance title
        /// </summary>
        public string Title { get; protected set; }
        /// <summary>
        /// Get process instance version
        /// </summary>
        public int Version { get; protected set; }
        /// <summary>
        /// Get process instance description
        /// </summary>
        public string Description { get; protected set; }
        /// <summary>
        /// Get process instance priority
        /// </summary>
        public int Priority { get; protected set; }
        /// <summary>
        /// Get process instance limit time
        /// </summary>
        public int LimitTime { get; protected set; }
        public Guid WkDefinitionId { get; protected set; }
        public WkDefinition WkDefinition { get; protected set; }
        /// <summary>
        /// Get process instance is it complete
        /// </summary>
        public bool Completed { get; protected set; }
        /// <summary>
        /// Set process instance icon
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// Set process instance color
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// Get process instance excution pointers
        /// </summary>
        public ICollection<WkExecutionPointer> WkExecutionPointers { get; protected set; }
        public ICollection<WkNodeInstance_Obsolete> StepInstances { get; protected set; }
        /// <summary>
        /// 过程实例状态
        /// </summary>
        public WkInstanceState WkInstanceState { get; protected set; }
        public WkInstance_Obsolete()
        { }
        public WkInstance_Obsolete(
            string title,
            WkDefinition wkDefination,
            int priority,
            int limitTime)
        {
            Title = title;
            WkDefinition = wkDefination;
            Priority = priority;
            LimitTime = limitTime;
        }
        public Task SetName(string title)
        {
            Title = title;
            return Task.CompletedTask;
        }
        public Task SetPriority(int priority)
        {
            Priority = priority;
            return Task.CompletedTask;
        }
        public Task SetCompleted()
        {
            Completed = true;
            return Task.CompletedTask;
        }
        public Task SetWkDefinition(WkDefinition definition)
        {
            WkDefinition = definition;
            return Task.CompletedTask;
        }
    }
}
