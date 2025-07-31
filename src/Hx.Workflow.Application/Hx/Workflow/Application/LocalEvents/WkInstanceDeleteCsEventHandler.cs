using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;

namespace Hx.Workflow.Application.LocalEvents
{
    [LocalEventHandlerOrder(10)]
    public class WkInstanceDeleteCsEventHandler(IWorkflowAppService appService) : ILocalEventHandler<WkInstanceDeleteEventData>, ITransientDependency
    {
        public IWorkflowAppService AppService = appService;
#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        public async Task HandleEventAsync(WkInstanceDeleteEventData eventData)
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        {
            string processType = eventData.ProcessType;
        }
    }
}
