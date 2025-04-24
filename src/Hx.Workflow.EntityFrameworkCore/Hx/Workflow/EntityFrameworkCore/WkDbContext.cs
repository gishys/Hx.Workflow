using Hx.Workflow.Domain;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.StepBodys;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore
{
    [ConnectionStringName(WkDbProperties.ConnectionStringName)]
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public class WkDbContext(DbContextOptions<WkDbContext> options) : AbpDbContext<WkDbContext>(options)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        public virtual ICollection<WkDefinition> WkDefinitions { get; set; }
        public virtual ICollection<WkStepBody> WkStepBodies { get; set; }
        public virtual ICollection<WkEvent> WkEvents { get; set; }
        public virtual ICollection<WkExecutionError> WkExecutionErrors { get; set; }
        public virtual ICollection<WkExtensionAttribute> WkExtensionAttributes { get; set; }
        public virtual ICollection<WkExecutionPointer> WkExecutionPointers { get; set; }
        public virtual ICollection<WkInstance> WkInstances { get; set; }
        public virtual ICollection<WkSubscription> WkSubscriptions { get; set; }
        public virtual ICollection<WkAuditor> WkAuditors { get; set; }
        public virtual ICollection<WkDefinitionGroup> WkDefinitionGroups { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HxWorkflowConfigration();
        }
    }
}