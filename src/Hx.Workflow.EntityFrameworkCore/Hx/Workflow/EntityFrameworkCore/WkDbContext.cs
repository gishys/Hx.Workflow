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
        public virtual DbSet<WkDefinition> WkDefinitions { get; set; }
        public virtual DbSet<WkStepBody> WkStepBodies { get; set; }
        public virtual DbSet<WkEvent> WkEvents { get; set; }
        public virtual DbSet<WkExecutionError> WkExecutionErrors { get; set; }
        public virtual DbSet<WkExtensionAttribute> WkExtensionAttributes { get; set; }
        public virtual DbSet<WkExecutionPointer> WkExecutionPointers { get; set; }
        public virtual DbSet<WkInstance> WkInstances { get; set; }
        public virtual DbSet<WkSubscription> WkSubscriptions { get; set; }
        public virtual DbSet<WkAuditor> WkAuditors { get; set; }
        public virtual DbSet<WkDefinitionGroup> WkDefinitionGroups { get; set; }
        public virtual DbSet<ApplicationFormGroup> ApplicationFormGroups { get; set; }
        public virtual DbSet<ApplicationForm> ApplicationForms { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HxWorkflowConfigration();
        }
    }
}