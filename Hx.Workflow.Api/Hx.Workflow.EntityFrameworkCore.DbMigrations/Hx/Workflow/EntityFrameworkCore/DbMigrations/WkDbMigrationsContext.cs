using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore.DbMigrations
{
    public class WkDbMigrationsContext : AbpDbContext<WkDbMigrationsContext>
    {
        public WkDbMigrationsContext(
            DbContextOptions<WkDbMigrationsContext> options)
            : base(options)
        { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HxWorkflowConfigration();
        }
    }
}
