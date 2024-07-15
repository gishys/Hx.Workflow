using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
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
        private readonly IIdentityUserRepository _identityUserRepository;
        private readonly IWkEventRepository _eventRepository;
        public ExecutionPointerChangedEventHandler(
            IWkExecutionPointerRepository wkExecutionPointer,
            IIdentityUserRepository identityUserRepository,
            IWkEventRepository wkEventRepository
            )
        {
            _wkExecutionPointer = wkExecutionPointer;
            _identityUserRepository = identityUserRepository;
            _eventRepository = wkEventRepository;
        }

        public async Task HandleEventAsync(
            EntityChangedEventData<WkExecutionPointer> eventData)
        {
            if (eventData.Entity.Status == PointerStatus.Complete)
            {
                var wkEvent = await _eventRepository.GetByEventKeyAsync($"{eventData.Entity.Id}");
                if (wkEvent != null && wkEvent.CreatorId.HasValue)
                {
                    var creator = await _identityUserRepository.FindAsync(wkEvent.CreatorId.Value, false);
                    await eventData.Entity.SetSubmitterInfo(creator.Name, creator.Id);
                    await _wkExecutionPointer.UpdateAsync(eventData.Entity);
                }
            }
        }
    }
}