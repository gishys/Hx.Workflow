using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.Identity;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain.LocalEvents
{
    public class ExecutionPointerChangedEventHandler
    : ILocalEventHandler<EntityChangedEventData<WkExecutionPointer>>,
          ITransientDependency
    {
        private readonly IWkExecutionPointerRepository _wkExecutionPointer;
        private readonly IWkEventRepository _eventRepository;
        private readonly IServiceProvider _serviceProvider;
        public ExecutionPointerChangedEventHandler(
            IWkExecutionPointerRepository wkExecutionPointer,
            IWkEventRepository wkEventRepository,
            IServiceProvider serviceProvider
            )
        {
            _wkExecutionPointer = wkExecutionPointer;
            _eventRepository = wkEventRepository;
            _serviceProvider = serviceProvider;
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
        }
    }
}