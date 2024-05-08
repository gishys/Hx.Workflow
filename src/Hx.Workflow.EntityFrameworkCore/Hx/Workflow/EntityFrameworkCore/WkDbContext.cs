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
    public class WkDbContext : AbpDbContext<WkDbContext>
    {
        public WkDbContext(DbContextOptions<WkDbContext> options)
            : base(options)
        { }
        public virtual ICollection<WkDefinition> WkDefinitions { get; set; }
        public virtual ICollection<WkStepBody> WkStepBodies { get; set; }
        public virtual ICollection<WkEvent> WkEvents { get; set; }
        public virtual ICollection<WkExecutionError> WkExecutionErrors { get; set; }
        public virtual ICollection<WkExtensionAttribute> WkExtensionAttributes { get; set; }
        public virtual ICollection<WkExecutionPointer> WkExecutionPointers { get; set; }
        public virtual ICollection<WkInstance> WkInstances { get; set; }
        public virtual ICollection<WkSubscription> WkSubscriptions { get; set; }
        public virtual ICollection<WkAuditor> WkAuditors { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HxWorkflowConfigration();
        }
    }
}