using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace Hx.Workflow.EntityFrameworkCore.DbMigrations
{
    public class WkDbMigrationsContext(
        DbContextOptions<WkDbMigrationsContext> options) : AbpDbContext<WkDbMigrationsContext>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HxWorkflowConfigration();
        }
    }
}
