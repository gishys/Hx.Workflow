using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public HxPersistenceProvider(
            IWkSubscriptionRepository wkSubscriptionRepository,
            IUnitOfWorkManager unitOfWorkManager,
            IWkEventRepository wkEventRepository,
            IWkInstanceRepository wkInstanceRepository,
            IWkDefinitionRespository wkDefinitionRespository,
            IWkErrorRepository wkErrorRepository,
            IGuidGenerator guidGenerator,
            ICurrentUser currentUser)
        {
            _wkSubscriptionRepository = wkSubscriptionRepository;
            _unitOfWorkManager = unitOfWorkManager;
            _wkEventRepository = wkEventRepository;
            _wkInstanceRepository = wkInstanceRepository;
            _wkDefinitionRespository = wkDefinitionRespository;
            _wkErrorRepository = wkErrorRepository;
            _guidGenerator = guidGenerator;
            _currentUser = currentUser;
        }
        public virtual async Task ClearSubscriptionToken(string eventSubscriptionId, string token)
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
        public async Task<string> CreateEvent(Event newEvent)
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
        public async Task<string> CreateEventSubscription(EventSubscription subscription)
        {
            subscription.Id = _guidGenerator.Create().ToString();
            var persistable = subscription.ToPersistable();
            return (await _wkSubscriptionRepository.InsertAsync(persistable)).Id.ToString();
        }
        public async Task<string> CreateNewWorkflow(WorkflowInstance workflow)
        {
            workflow.Id = _guidGenerator.Create().ToString();
            var wkInstance = await workflow.ToPersistable();
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
        public async Task<Event> GetEvent(string id)
        {
            Guid uid = new Guid(id);
            var raw = await _wkEventRepository.FindAsync(uid);
            if (raw == null)
                return null;
            return raw.ToEvent();
        }
        public async Task<IEnumerable<string>> GetEvents(string eventName, string eventKey, DateTime asOf)
        {
            var raw = await _wkEventRepository
                .GetEventsAsync(eventName, eventKey, asOf);
            var result = new List<string>();
            foreach (var s in raw)
                result.Add(s.Id.ToString());
            return result;
        }
        public async Task<EventSubscription> GetFirstOpenSubscription(string eventName, string eventKey, DateTime asOf)
        {
            var raw = await _wkSubscriptionRepository
                .GetSubcriptionAsync(eventName, eventKey, asOf);
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
        public async Task<IEnumerable<string>> GetRunnableEvents(DateTime asAt)
        {
            return from p in await
                   _wkEventRepository.GetRunnableEventsAsync(asAt)
                   select p.ToString();
        }
        public async Task<IEnumerable<string>> GetRunnableInstances(DateTime asAt)
        {
            return from p in await
                   _wkInstanceRepository.GetRunnableInstancesAsync(asAt)
                   select p.ToString();
        }
        public async Task<EventSubscription> GetSubscription(string eventSubscriptionId)
        {
            var raw = await _wkSubscriptionRepository.FindAsync(new Guid(eventSubscriptionId));
            return raw?.ToEventSubscription();
        }
        public async Task<IEnumerable<EventSubscription>> GetSubscriptions(string eventName, string eventKey, DateTime asOf)
        {
            var subs = await _wkSubscriptionRepository.GetSubcriptionAsync(eventName, eventKey, asOf);
            return from x in subs select x.ToEventSubscription();
        }
        public async Task<WorkflowInstance> GetWorkflowInstance(string Id)
        {
            var entity = await _wkInstanceRepository.FindAsync(new Guid(Id));
            if (entity == null)
                return null;
            return entity.ToWorkflowInstance();
        }
        public async Task<IEnumerable<WorkflowInstance>> GetWorkflowInstances(WorkflowStatus? status, string type, DateTime? createdFrom, DateTime? createdTo, int skip, int take)
        {
            var query = await _wkInstanceRepository.GetDetails();
            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);
            if (!string.IsNullOrEmpty(type))
                query = query.Where(x => x.WkDifinitionId == new Guid(type));
            if (createdFrom.HasValue)
                query = query.Where(x => x.CreateTime >= createdFrom.Value);
            if (createdTo.HasValue)
                query = query.Where(x => x.CreateTime <= createdTo.Value);

            var rawResult = query.Skip(skip).Take(take).ToList();
            if (rawResult == null)
                return null;
            List<WorkflowInstance> result = new List<WorkflowInstance>();
            foreach (var item in rawResult)
                result.Add(item.ToWorkflowInstance());
            return result;
        }
        public async Task<IEnumerable<WorkflowInstance>> GetWorkflowInstances(IEnumerable<string> ids)
        {
            if (ids == null)
            {
                return new List<WorkflowInstance>();
            }
            var uids = ids.Select(i => new Guid(i)).ToList();
            var raws = await _wkInstanceRepository.GetDetails(uids);
            return raws.Select(i => i.ToWorkflowInstance());
        }
        public async Task MarkEventProcessed(string id)
        {
            var uid = new Guid(id);
            var existingEntity = await _wkEventRepository.FindAsync(uid);
            await existingEntity.SetProcessed(true);
            await _wkEventRepository.UpdateAsync(existingEntity);
        }
        public async Task MarkEventUnprocessed(string id)
        {
            var uid = new Guid(id);
            var existingEntity = await _wkEventRepository.FindAsync(uid);
            await existingEntity.SetProcessed(false);
            await _wkEventRepository.UpdateAsync(existingEntity);
        }
        public async Task PersistErrors(IEnumerable<ExecutionError> errors)
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
        public async Task PersistWorkflow(WorkflowInstance workflow)
        {
            using (var uow = _unitOfWorkManager.Begin())
            {
                var uid = new Guid(workflow.Id);
                var existingEntity = await _wkInstanceRepository.FindAsync(uid);
                if (existingEntity == null)
                    return;
                var persistable = await workflow.ToPersistable(existingEntity);
                await _wkInstanceRepository.UpdateAsync(persistable);
                await uow.CompleteAsync();
            }
        }
        public async Task<bool> SetSubscriptionToken(string eventSubscriptionId, string token, string workerId, DateTime expiry)
        {
            var uid = new Guid(eventSubscriptionId);
            var existingEntity = await _wkSubscriptionRepository.FindAsync(uid);

            await existingEntity.SetExternalToken(token);
            await existingEntity.SetExternalWorkerId(workerId);
            if (expiry > new DateTime(9999, 12, 31))
                expiry = new DateTime(9999, 12, 31);
            await existingEntity.SetExternalTokenExpiry(expiry);
            await _wkSubscriptionRepository.UpdateAsync(existingEntity);
            return true;
        }
        public async Task TerminateSubscription(string eventSubscriptionId)
        {
            var uid = new Guid(eventSubscriptionId);
            await _wkSubscriptionRepository.DeleteAsync(uid);
        }
    }
}
