using Hx.Workflow.Domain.BusinessModule;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain
{
    public class HxPersistenceProvider : IHxPersistenceProvider, ISingletonDependency
    {
        private readonly IWkSubscriptionRepository _wkSubscriptionRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IWkEventRepository _wkEventRepository;
        private readonly IWkInstanceRepository _wkInstanceRepository;
        private readonly IWkDefinitionRespository _wkDefinitionRespository;
        private readonly IWkErrorRepository _wkErrorRepository;
        private readonly IGuidGenerator _guidGenerator;
        private readonly ICurrentUser _currentUser;
        private readonly BusinessNumberManager BusinessNumberManager;
        public HxPersistenceProvider(
            IWkSubscriptionRepository wkSubscriptionRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IWkEventRepository wkEventRepository,
            IWkInstanceRepository wkInstanceRepository,
            IWkDefinitionRespository wkDefinitionRespository,
            IWkErrorRepository wkErrorRepository,
            IGuidGenerator guidGenerator,
            ICurrentUser currentUser,
            BusinessNumberManager businessNumberManager)
        {
            _wkSubscriptionRepository = wkSubscriptionRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _wkEventRepository = wkEventRepository;
            _wkInstanceRepository = wkInstanceRepository;
            _wkDefinitionRespository = wkDefinitionRespository;
            _wkErrorRepository = wkErrorRepository;
            _guidGenerator = guidGenerator;
            _currentUser = currentUser;
            BusinessNumberManager = businessNumberManager;
        }

        public bool SupportsScheduledCommands { get; }

        public virtual async Task ClearSubscriptionToken(string eventSubscriptionId, string token, CancellationToken cancellationToken = default)
        {
            var uid = new Guid(eventSubscriptionId);
            var existingEntity = await _wkSubscriptionRepository.FindAsync(uid, true);
            if (existingEntity.ExternalToken != token)
                throw new InvalidOperationException();
            await existingEntity.SetExternalToken(null);
            await existingEntity.SetExternalWorkerId(null);
            await existingEntity.SetExternalTokenExpiry(null);
            await _wkSubscriptionRepository.UpdateAsync(existingEntity);
        }
        public async Task<string> CreateEvent(Event newEvent, CancellationToken cancellationToken = default)
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                newEvent.Id = _guidGenerator.Create().ToString();
                var persistable = newEvent.ToPersistable();
                var entity = await _wkEventRepository.InsertAsync(persistable);
                await uow.SaveChangesAsync();
                return entity.Id.ToString();
            }
        }
        public async Task<string> CreateEventSubscription(EventSubscription subscription, CancellationToken cancellationToken = default)
        {
            subscription.Id = _guidGenerator.Create().ToString();
            var persistable = subscription.ToPersistable();
            return (await _wkSubscriptionRepository.InsertAsync(persistable)).Id.ToString();
        }
        public async Task<string> CreateNewWorkflow(WorkflowInstance workflow, CancellationToken cancellationToken = default)
        {
            workflow.Id = _guidGenerator.Create().ToString();
            var businessNumber = await BusinessNumberManager.GetMaxNumber();
            var persistData = new WkInstancePersistData(businessNumber, _currentUser.UserName, _currentUser.Id.HasValue ? _currentUser.Id.Value : Guid.Empty);
            var definition = await _wkDefinitionRespository.FindAsync(new Guid(workflow.WorkflowDefinitionId));
            foreach (var pointer in workflow.ExecutionPointers)
            {
                var node = definition.Nodes.First(d => d.Name == pointer.StepName);
                if (node.LimitTime != null)
                {
                    var pointerData = new Dictionary<string, object>() { { "CommitmentDeadline", DateTime.Now.AddMinutes((double)node.LimitTime) } };
                    pointer.EventData = pointerData;
                }
            }
            var wkInstance = await workflow.ToPersistable(persistData);
            return (await _wkInstanceRepository.InsertAsync(wkInstance)).Id.ToString();
        }
        public void EnsureStoreExists()
        {
            //throw new NotImplementedException();
        }
        public async Task<IEnumerable<WkInstance>> GetAllRunnablePersistedWorkflow(string definitionId, int version)
        {
            return await _wkInstanceRepository.GetInstancesAsync(definitionId, version);
        }
        public async Task<Event> GetEvent(string id, CancellationToken cancellationToken = default)
        {
            Guid uid = new Guid(id);
            var raw = await _wkEventRepository.FindAsync(uid);
            if (raw == null)
                return null;
            return raw.ToEvent();
        }
        public async Task<IEnumerable<string>> GetEvents(string eventName, string eventKey, DateTime asOf, CancellationToken cancellationToken = default)
        {
            DateTime asOfTime = DateTime.SpecifyKind(asOf, DateTimeKind.Unspecified);
            var raw = await _wkEventRepository
                .GetEventsAsync(eventName, eventKey, asOfTime);
            var result = new List<string>();
            foreach (var s in raw)
                result.Add(s.Id.ToString());
            return result;
        }
        public async Task<EventSubscription> GetFirstOpenSubscription(string eventName, string eventKey, DateTime asOf, CancellationToken cancellationToken = default)
        {
            DateTime asOfTime = DateTime.SpecifyKind(asOf, DateTimeKind.Unspecified);
            var raw = await _wkSubscriptionRepository
                .GetSubcriptionAsync(eventName, eventKey, asOfTime);
            return raw?.FirstOrDefault()?.ToEventSubscription();
        }
        public async Task<WkExecutionPointer> GetPersistedExecutionPointer(string id)
        {
            return await _wkInstanceRepository.GetPointerAsync(new Guid(id));
        }
        public async Task<WkInstance> GetPersistedWorkflow(Guid id)
        {
            return await _wkInstanceRepository.FindAsync(id);
        }
        public async Task<WkDefinition> GetPersistedWorkflowDefinition(string id, int version)
        {
            return await _wkDefinitionRespository.GetDefinitionAsync(new Guid(id), version);
        }
        public async Task<IEnumerable<string>> GetRunnableEvents(DateTime asAt, CancellationToken cancellationToken = default)
        {
            DateTime asAtTime = DateTime.SpecifyKind(asAt, DateTimeKind.Unspecified);
            return from p in await
                   _wkEventRepository.GetRunnableEventsAsync(asAtTime)
                   select p.ToString();
        }
        public async Task<IEnumerable<string>> GetRunnableInstances(DateTime asAt, CancellationToken cancellationToken = default)
        {
            DateTime asAtTime = DateTime.SpecifyKind(asAt, DateTimeKind.Unspecified);
            return from p in await
                   _wkInstanceRepository.GetRunnableInstancesAsync(asAtTime)
                   select p.ToString();
        }
        public async Task<EventSubscription> GetSubscription(string eventSubscriptionId, CancellationToken cancellationToken = default)
        {
            var raw = await _wkSubscriptionRepository.FindAsync(new Guid(eventSubscriptionId));
            return raw?.ToEventSubscription();
        }
        public async Task<IEnumerable<EventSubscription>> GetSubscriptions(string eventName, string eventKey, DateTime asOf, CancellationToken cancellationToken = default)
        {
            DateTime asOfTime = DateTime.SpecifyKind(asOf, DateTimeKind.Unspecified);
            var subs = await _wkSubscriptionRepository.GetSubcriptionAsync(eventName, eventKey, asOfTime);
            return from x in subs select x.ToEventSubscription();
        }
        public async Task<WorkflowInstance> GetWorkflowInstance(string Id, CancellationToken cancellationToken = default)
        {
            var entity = await _wkInstanceRepository.FindAsync(new Guid(Id));
            if (entity == null)
                return null;
            return entity.ToWorkflowInstance();
        }
        public async Task<IEnumerable<WorkflowInstance>> GetWorkflowInstances(WorkflowStatus? status, string type, DateTime? createdFrom, DateTime? createdTo, int skip, int take)
        {
            DateTime? createdFromTime = createdFrom.HasValue ? DateTime.SpecifyKind(createdFrom.Value, DateTimeKind.Unspecified) : null;
            DateTime? createdToTime = createdFrom.HasValue ? DateTime.SpecifyKind(createdTo.Value, DateTimeKind.Unspecified) : null;
            var query = await _wkInstanceRepository.GetDetails();
            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);
            if (!string.IsNullOrEmpty(type))
                query = query.Where(x => x.WkDifinitionId == new Guid(type));
            if (createdFrom.HasValue)
                query = query.Where(x => x.CreateTime >= createdFromTime.Value);
            if (createdTo.HasValue)
                query = query.Where(x => x.CreateTime <= createdToTime.Value);

            var rawResult = query.Skip(skip).Take(take).ToList();
            if (rawResult == null)
                return null;
            List<WorkflowInstance> result = new List<WorkflowInstance>();
            foreach (var item in rawResult)
                result.Add(item.ToWorkflowInstance());
            return result;
        }
        public async Task<IEnumerable<WorkflowInstance>> GetWorkflowInstances(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null)
            {
                return new List<WorkflowInstance>();
            }
            var uids = ids.Select(i => new Guid(i)).ToList();
            var raws = await _wkInstanceRepository.GetDetails(uids);
            return raws.Select(i => i.ToWorkflowInstance());
        }
        public async Task MarkEventProcessed(string id, CancellationToken cancellationToken = default)
        {
            var uid = new Guid(id);
            var existingEntity = await _wkEventRepository.FindAsync(uid);
            await existingEntity.SetProcessed(true);
            await _wkEventRepository.UpdateAsync(existingEntity);
        }
        public async Task MarkEventUnprocessed(string id, CancellationToken cancellationToken = default)
        {
            var uid = new Guid(id);
            var existingEntity = await _wkEventRepository.FindAsync(uid);
            await existingEntity.SetProcessed(false);
            await _wkEventRepository.UpdateAsync(existingEntity);
        }
        public async Task PersistErrors(IEnumerable<ExecutionError> errors, CancellationToken cancellationToken = default)
        {
            var executionErrors = errors as ExecutionError[] ?? errors.ToArray();
            if (executionErrors.Any())
            {
                foreach (var error in executionErrors)
                {
                    await _wkErrorRepository.InsertAsync(error.ToPersistable());
                }
            }
        }
        public async Task PersistWorkflow(WorkflowInstance workflow, CancellationToken cancellationToken = default)
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                var uid = new Guid(workflow.Id);
                var existingEntity = await _wkInstanceRepository.FindAsync(uid);
                if (existingEntity == null)
                    return;
                var persistData = new WkInstancePersistData(existingEntity.BusinessNumber, _currentUser.UserName, _currentUser.Id.HasValue ? _currentUser.Id.Value : Guid.Empty);
                var persistable = await workflow.ToPersistable(persistData, existingEntity);
                await _wkInstanceRepository.UpdateAsync(persistable);
                await uow.CompleteAsync();
            }
        }
        public async Task PersistWorkflow(WorkflowInstance workflow, List<EventSubscription> subscriptions, CancellationToken cancellationToken = default)
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                var uid = new Guid(workflow.Id);
                var existingEntity = await _wkInstanceRepository.FindAsync(uid);
                if (existingEntity == null)
                    return;
                var persistData = new WkInstancePersistData(existingEntity.BusinessNumber, _currentUser.UserName, _currentUser.Id.HasValue ? _currentUser.Id.Value : Guid.Empty);
                var persistable = await workflow.ToPersistable(persistData, existingEntity);
                await _wkInstanceRepository.UpdateAsync(persistable);
                //需要确认
                foreach (var subscription in subscriptions)
                {
                    if (!string.IsNullOrEmpty(subscription.Id))
                    {
                        var subscriptionEntity = await _wkSubscriptionRepository.FindAsync(new Guid(subscription.Id));
                        if (subscriptionEntity != null)
                        {
                            //这部分需要测试
                        }
                    }
                    else
                    {
                        subscription.Id = _guidGenerator.Create().ToString();
                        var scription = subscription.ToPersistable();
                        await _wkSubscriptionRepository.InsertAsync(scription);
                    }
                }
                await uow.CompleteAsync();
            }
        }
        //未实现
        public Task ProcessCommands(DateTimeOffset asOf, Func<ScheduledCommand, Task> action, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
        //未实现
        public Task ScheduleCommand(ScheduledCommand command)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetSubscriptionToken(string eventSubscriptionId, string token, string workerId, DateTime expiry, CancellationToken cancellationToken = default)
        {
            var wkInstance = await _wkInstanceRepository.FindAsync(new Guid(workerId));
            if (wkInstance == null)
                throw new UserFriendlyException("流程实例不存在！");
            var uid = new Guid(eventSubscriptionId);
            var existingEntity = await _wkSubscriptionRepository.FindAsync(uid);

            await existingEntity.SetExternalToken(token);
            await existingEntity.SetExternalWorkerId(workerId);
            if (expiry > new DateTime(9999, 12, 31))
                expiry = new DateTime(9999, 12, 31);
            DateTime expiryTime = DateTime.SpecifyKind(expiry, DateTimeKind.Unspecified);
            await existingEntity.SetExternalTokenExpiry(expiryTime);
            await _wkSubscriptionRepository.UpdateAsync(existingEntity);
            return true;
        }
        public async Task TerminateSubscription(string eventSubscriptionId, CancellationToken cancellationToken = default)
        {
            var uid = new Guid(eventSubscriptionId);
            await _wkSubscriptionRepository.DeleteAsync(uid);
        }
    }
}