using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace Hx.Workflow.Domain.LocalEvents
{
    [Authorize]
    [HubRoute("/api/app/receivealarmmessage")]
    public class WorkflowInstanceHub : AbpHub, ITransientDependency
    {
        public WorkflowInstanceHub()
        {
        }
        [UnitOfWork]
        public override async Task OnConnectedAsync()
        {
            if (CurrentUser.Id.HasValue)
            {
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
