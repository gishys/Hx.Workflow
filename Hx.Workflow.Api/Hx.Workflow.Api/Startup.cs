using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Hx.Workflow.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplication<HxWorkflowApiModule>();
        }
        public void Configure(IApplicationBuilder app)
        {
            app.InitializeApplication();
        }
    }
}
