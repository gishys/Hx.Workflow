using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;

namespace Hx.Workflow.Domain.LocalEvents
{
    [Authorize]
    [HubRoute("/api/app/workflowinstance")]
    public class WorkflowInstanceHub : AbpHub, ITransientDependency
    {
        public WorkflowInstanceHub()
        {
        }
        [UnitOfWork]
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
