using Hx.Workflow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Hx.Workflow.EntityFrameworkCore.DbMigrations
{
    public class WkDbMigrationContextFactory : IDesignTimeDbContextFactory<WkDbMigrationsContext>
    {
        public WkDbMigrationsContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration();
            var builder =
                new DbContextOptionsBuilder<WkDbMigrationsContext>()
                .UseMySql(
                configuration.GetConnectionString(WkDbProperties.ConnectionStringName),
                new MySqlServerVersion(new Version(5, 7, 19)));
            return new WkDbMigrationsContext(builder.Options);
        }
        private static IConfigurationRoot BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}
