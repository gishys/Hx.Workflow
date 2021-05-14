using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace Hx.Workflow.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(baseDirectory, "log/log.txt"))
                .CreateLogger();
            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args)
                .Build().Run();
                return;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(serverOptions =>
                    {

                    })
                    .UseStartup<Startup>();
                })
            .UseSerilog()
            .UseAutofac();  // 添加这一行
    }
}
