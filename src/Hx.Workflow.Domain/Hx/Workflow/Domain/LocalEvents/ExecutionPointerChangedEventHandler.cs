using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Identity;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain.LocalEvents
{
    [LocalEventHandlerOrder(10)]
    public class ExecutionPointerChangedEventHandler
    : ILocalEventHandler<EntityChangedEventData<WkExecutionPointer>>, ITransientDependency
    {
        private readonly IWkExecutionPointerRepository _wkExecutionPointer;
        private readonly IWkEventRepository _eventRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<WorkflowInstanceHub> _workflowInstanceHub;
        private readonly IWkSubscriptionRepository _wkSubscriptionRepository;
        private readonly IWkDefinitionRespository _wkDefinitionRespository;
        public ExecutionPointerChangedEventHandler(
            IWkExecutionPointerRepository wkExecutionPointer,
            IWkEventRepository wkEventRepository,
            IServiceProvider serviceProvider,
            IHubContext<WorkflowInstanceHub> workflowInstanceHub,
            IWkSubscriptionRepository wkSubscriptionRepository,
            IWkDefinitionRespository wkDefinitionRespository
            )
        {
            _wkExecutionPointer = wkExecutionPointer;
            _eventRepository = wkEventRepository;
            _serviceProvider = serviceProvider;
            _workflowInstanceHub = workflowInstanceHub;
            _wkSubscriptionRepository = wkSubscriptionRepository;
            _wkDefinitionRespository = wkDefinitionRespository;
        }

        public async Task HandleEventAsync(
            EntityChangedEventData<WkExecutionPointer> eventData)
        {
            if (eventData.Entity.Status == PointerStatus.Complete)
            {
                var wkEvent = await _eventRepository.GetByEventKeyAsync($"{eventData.Entity.Id}");
                if (wkEvent != null && wkEvent.CreatorId.HasValue)
                {
                    var userRepository = _serviceProvider.GetService<IIdentityUserRepository>();
                    string userName = null;
                    if (userRepository != null)
                    {
                        var creator = await userRepository.FindAsync(wkEvent.CreatorId.Value, false);
                        userName = creator.UserName;
                    }
                    await eventData.Entity.SetSubmitterInfo(userName, wkEvent.CreatorId.Value);
                    await _wkExecutionPointer.UpdateAsync(eventData.Entity);
                }
            }
            //流程创建后通知客户端初始化完成
            if (eventData.Entity.WkInstance.CreatorId.HasValue)
            {
                var definition = await _wkDefinitionRespository.FindAsync(eventData.Entity.WkInstance.WkDifinitionId);
                var step = definition.Nodes.First(d => d.Name == eventData.Entity.StepName);
                await _workflowInstanceHub.Clients.User(
                                eventData.Entity.WkInstance.CreatorId.Value.ToString()).SendAsync("WorkflowInitCompleted",
                                new
                                {
                                    eventData.Entity.WkInstanceId,
                                    eventData.Entity.Status,
                                    StepTitle = step.DisplayName,
                                    step.StepNodeType,
                                    eventData.Entity.Active
                                });
            }
        }
    }
}