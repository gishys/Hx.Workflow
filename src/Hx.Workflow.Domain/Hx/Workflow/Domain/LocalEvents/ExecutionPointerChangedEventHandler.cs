using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Hx.Workflow.Domain.Stats;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.Identity;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain.LocalEvents
{
    public class ExecutionPointerChangedEventHandler
    : ILocalEventHandler<EntityChangedEventData<WkExecutionPointer>>, ITransientDependency
    {
        private readonly IWkExecutionPointerRepository _wkExecutionPointer;
        private readonly IWkEventRepository _eventRepository;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<WorkflowInstanceHub> _workflowInstanceHub;
        private readonly IWkSubscriptionRepository _wkSubscriptionRepository;
        private readonly IBusinessStatRepository _businessStatRepository;
        public ExecutionPointerChangedEventHandler(
            IWkExecutionPointerRepository wkExecutionPointer,
            IWkEventRepository wkEventRepository,
            IServiceProvider serviceProvider,
            IHubContext<WorkflowInstanceHub> workflowInstanceHub,
            IWkSubscriptionRepository wkSubscriptionRepository,
            IBusinessStatRepository businessStatRepository
            )
        {
            _wkExecutionPointer = wkExecutionPointer;
            _eventRepository = wkEventRepository;
            _serviceProvider = serviceProvider;
            _workflowInstanceHub = workflowInstanceHub;
            _wkSubscriptionRepository = wkSubscriptionRepository;
            _businessStatRepository = businessStatRepository;
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
            if (eventData.Entity.Status != PointerStatus.Complete
                && eventData.Entity.StepId == 0)
            {
                var subscriptions = await _wkSubscriptionRepository.GetSubscriptionsByExecutionPointerAsync(eventData.Entity.Id);
                if (subscriptions.Any(d => d.ExternalToken == null))
                {
                    if (eventData.Entity.WkInstance.CreatorId.HasValue)
                    {
                        await _workflowInstanceHub.Clients.User(
                            eventData.Entity.WkInstance.CreatorId.Value.ToString()).SendAsync("WorkflowInitCompleted", eventData.Entity.WkInstanceId);
                    }
                }
            }
            //统计业务
            await UpdateTransactorStatAsync(eventData.Entity);
        }
        private async Task UpdateTransactorStatAsync(WkExecutionPointer pointer)
        {
            if (!pointer.CreatorId.HasValue)
            {
                return;
            }
            BusinessStat statEntity = null;
            if (pointer.Status == PointerStatus.WaitingForEvent &&
                pointer.WkCandidates.Any(c => c.CandidateId == pointer.CreatorId &&
                c.ParentState == ExeCandidateState.WaitingReceipt))
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.WaitingReceipt.ToString());
            }
            else if (pointer.Status == PointerStatus.WaitingForEvent &&
                pointer.WkCandidates.Any(c => c.CandidateId == pointer.CreatorId &&
                c.ParentState == ExeCandidateState.Pending))
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.Pending.ToString());
            }
            else if (pointer.Status == PointerStatus.WaitingForEvent)
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.Participation.ToString());
            }
            else if (pointer.Status == PointerStatus.WaitingForEvent &&
                pointer.WkCandidates.Any(c => c.CandidateId == pointer.CreatorId &&
                c.ExeOperateType == ExePersonnelOperateType.Entrusted))
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.Entrusted.ToString());
            }
            else if (pointer.Status == PointerStatus.WaitingForEvent &&
                pointer.StepId == 0 &&
                pointer.WkCandidates.Any(c => c.CandidateId == pointer.CreatorId))
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.Handled.ToString());
            }
            else if (pointer.Status == PointerStatus.WaitingForEvent &&
                pointer.WkCandidates.Any(c => c.CandidateId == pointer.CreatorId &&
                c.Follow == true))
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.Follow.ToString());
            }
            else if (pointer.Status == PointerStatus.WaitingForEvent &&
                pointer.WkCandidates.Any(c => c.CandidateId == pointer.CreatorId &&
                c.ExeOperateType == ExePersonnelOperateType.Countersign))
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.Countersign.ToString());
            }
            else if (pointer.Status == PointerStatus.WaitingForEvent &&
                pointer.WkCandidates.Any(c => c.CandidateId == pointer.CreatorId &&
                c.ExeOperateType == ExePersonnelOperateType.CarbonCopy))
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.CarbonCopy.ToString());
            }
            else if (pointer.Status == PointerStatus.Failed &&
                pointer.WkCandidates.Any(c => c.CandidateId == pointer.CreatorId))
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.Abnormal.ToString());
            }
            else if (pointer.WkInstance.Status == WorkflowStatus.Suspended &&
                pointer.WkCandidates.Any(c => c.CandidateId == pointer.CreatorId))
            {
                statEntity = await _businessStatRepository.GetAsync(
                    pointer.CreatorId.Value,
                    "TransactorStat",
                    MyWorkState.Suspended.ToString());
            }
            if (statEntity != null)
            {
                await statEntity.SetStatistics(statEntity.Statistics + 1);
                await _wkExecutionPointer.UpdateAsync(pointer);
            }
        }
    }
}