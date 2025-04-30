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
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Volo.Abp.Users;
using WorkflowCore.Models;

namespace Hx.Workflow.Domain
{
    public class HxPersistenceProvider(
        IWkSubscriptionRepository wkSubscriptionRepository,
        IUnitOfWorkManager unitOfWorkManager,
        IWkEventRepository wkEventRepository,
        IWkInstanceRepository wkInstanceRepository,
        IWkDefinitionRespository wkDefinitionRespository,
        IWkErrorRepository wkErrorRepository,
        IGuidGenerator guidGenerator,
        ICurrentUser currentUser,
        ReferenceManager referenceManager) : IHxPersistenceProvider, ISingletonDependency
    {
        private readonly IWkSubscriptionRepository _wkSubscriptionRepository = wkSubscriptionRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager = unitOfWorkManager;
        private readonly IWkEventRepository _wkEventRepository = wkEventRepository;
        private readonly IWkInstanceRepository _wkInstanceRepository = wkInstanceRepository;
        private readonly IWkDefinitionRespository _wkDefinitionRespository = wkDefinitionRespository;
        private readonly IWkErrorRepository _wkErrorRepository = wkErrorRepository;
        private readonly IGuidGenerator _guidGenerator = guidGenerator;
        private readonly ICurrentUser _currentUser = currentUser;
        private readonly ReferenceManager ReferenceManager = referenceManager;

        public bool SupportsScheduledCommands { get; }

        public virtual async Task ClearSubscriptionToken(string eventSubscriptionId, string token, CancellationToken cancellationToken = default)
        {
            var uid = new Guid(eventSubscriptionId);
            var existingEntity = await _wkSubscriptionRepository.FindAsync(uid, true, cancellationToken);
            if (existingEntity?.ExternalToken != token)
                throw new InvalidOperationException();
            await existingEntity.SetExternalToken(null);
            await existingEntity.SetExternalWorkerId(null);
            await existingEntity.SetExternalTokenExpiry(null);
            await _wkSubscriptionRepository.UpdateAsync(existingEntity, false, cancellationToken);
        }
        public async Task<string> CreateEvent(Event newEvent, CancellationToken cancellationToken = default)
        {
            using var uow = _unitOfWorkManager.Begin();
            newEvent.Id = _guidGenerator.Create().ToString();
            var persistable = newEvent.ToPersistable();
            var entity = await _wkEventRepository.InsertAsync(persistable, false, cancellationToken);
            await uow.SaveChangesAsync(cancellationToken);
            return entity.Id.ToString();
        }
        public async Task<string> CreateEventSubscription(EventSubscription subscription, CancellationToken cancellationToken = default)
        {
            subscription.Id = _guidGenerator.Create().ToString();
            var persistable = subscription.ToPersistable();
            return (await _wkSubscriptionRepository.InsertAsync(persistable, false, cancellationToken)).Id.ToString();
        }
        public async Task<string> CreateNewWorkflow(WorkflowInstance workflow, CancellationToken cancellationToken = default)
        {
            workflow.Id = _guidGenerator.Create().ToString();
            workflow.Reference = await ReferenceManager.GetMaxNumber();
            var wkInstance = await workflow.ToPersistable();
            if (wkInstance.ExecutionPointers.Count > 0)
            {
                wkInstance = await CreateExePointerMaterials(wkInstance, new Guid(workflow.WorkflowDefinitionId), workflow.Version, workflow.Reference);
                if (_currentUser.Id.HasValue && _currentUser.UserName != null)
                {
                    foreach (var executionPointer in wkInstance.ExecutionPointers)
                    {
                        await executionPointer.SetRecipientInfo(_currentUser.UserName, _currentUser.Id.Value);
                    }
                }
            }
            return (await _wkInstanceRepository.InsertAsync(wkInstance, false, cancellationToken)).Id.ToString();
        }
        private static WkExecutionPointerMaterials CreateExePointerMaterials(WkNodeMaterials materials, string reference)
        {
            var em = new WkExecutionPointerMaterials(
                                reference,
                                materials.AttachReceiveType,
                                materials.ReferenceType,
                                materials.CatalogueName,
                                materials.SequenceNumber,
                                materials.IsRequired,
                                materials.IsStatic,
                                materials.IsVerification,
                                materials.VerificationPassed);
            List<WkExecutionPointerMaterials> ms = [];
            if (materials.Children != null && materials.Children.Count > 0)
            {
                foreach (var m in materials.Children)
                {
                    WkExecutionPointerMaterials nm = CreateExePointerMaterials(m, reference);
                    ms.Add(nm);
                }
                ms.ForEach(em.AddChild);
            }
            return em;
        }
        private async Task<WkInstance> CreateExePointerMaterials(WkInstance wkInstance, Guid wkDefinitionId, int version, string reference)
        {
            WkDefinition definition = await _wkDefinitionRespository.GetDefinitionAsync(wkDefinitionId, version) ?? throw new UserFriendlyException(message: $"[{wkDefinitionId}]流程模板不存在！");
            foreach (var exePointer in wkInstance.ExecutionPointers.Where(d => d.Status != PointerStatus.Complete))
            {
                WkNode? node = definition.Nodes.FirstOrDefault(d => d.Name == exePointer.StepName);
                if (node != null)
                {
                    foreach (var m in node.Materials.OrderBy(d => d.SequenceNumber))
                    {
                        if (!exePointer.Materials.Any(em =>
                        em.Reference == reference &&
                        em.ReferenceType == m.ReferenceType &&
                        em.CatalogueName == m.CatalogueName))
                            await exePointer.AddMaterails(CreateExePointerMaterials(m, reference));
                    }
                }
            }
            return wkInstance;
        }
        public void EnsureStoreExists()
        {
            //throw new NotImplementedException();
        }
        //public async Task<IEnumerable<WkInstance>> GetAllRunnablePersistedWorkflow(string definitionId, int version)
        //{
        //    return await _wkInstanceRepository.GetInstancesAsync(definitionId, version);
        //}
        public async Task<Event> GetEvent(string id, CancellationToken cancellationToken = default)
        {
            Guid uid = new(id);
            var raw = await _wkEventRepository.FindAsync(uid, true, cancellationToken) ?? throw new UserFriendlyException(message: $"[{uid}]流程事件不存在！");
            return raw.ToEvent();
        }
        public async Task<IEnumerable<string>> GetEvents(string eventName, string eventKey, DateTime asOf, CancellationToken cancellationToken = default)
        {
            var raw = await _wkEventRepository
                .GetEventsAsync(eventName, eventKey, asOf);
            var result = new List<string>();
            foreach (var s in raw)
                result.Add(s.Id.ToString());
            return result;
        }
        public async Task<EventSubscription> GetFirstOpenSubscription(string eventName, string eventKey, DateTime asOf, CancellationToken cancellationToken = default)
        {
            var raw = await _wkSubscriptionRepository.GetSubscriptionAsync(eventName, eventKey, asOf);
            if (raw.Count <= 0)
                throw new UserFriendlyException(message: $"符合条件：[eventName:{eventName},eventKey:{eventKey},asOf:{asOf}]的流程描述不存在！");
            return raw.First().ToEventSubscription();
        }
        //public async Task<WkExecutionPointer> GetPersistedExecutionPointer(string id)
        //{
        //    return await _wkInstanceRepository.GetPointerAsync(new Guid(id));
        //}
        //public async Task<WkInstance> GetPersistedWorkflow(Guid id)
        //{
        //    return await _wkInstanceRepository.FindAsync(id);
        //}
        //public async Task<WkDefinition> GetPersistedWorkflowDefinition(string id, int version)
        //{
        //    return await _wkDefinitionRespository.GetDefinitionAsync(new Guid(id), version);
        //}
        public async Task<IEnumerable<string>> GetRunnableEvents(DateTime asAt, CancellationToken cancellationToken = default)
        {
            DateTime asAtTime = asAt;
            return from p in await
                   _wkEventRepository.GetRunnableEventsAsync(asAtTime)
                   select p.ToString();
        }
        public async Task<IEnumerable<string>> GetRunnableInstances(DateTime asAt, CancellationToken cancellationToken = default)
        {
            DateTime asAtTime = asAt;
            return from p in await
                   _wkInstanceRepository.GetRunnableInstancesAsync(asAtTime)
                   select p.ToString();
        }
        public async Task<EventSubscription> GetSubscription(string eventSubscriptionId, CancellationToken cancellationToken = default)
        {
            var raw = await _wkSubscriptionRepository.FindAsync(new Guid(eventSubscriptionId), true, cancellationToken) ?? throw new UserFriendlyException(message: $"[{eventSubscriptionId}]流程描述不存在！");
            return raw.ToEventSubscription();
        }
        public async Task<IEnumerable<EventSubscription>> GetSubscriptions(string eventName, string eventKey, DateTime asOf, CancellationToken cancellationToken = default)
        {
            var subs = await _wkSubscriptionRepository.GetSubscriptionAsync(eventName, eventKey, asOf);
            return from x in subs select x.ToEventSubscription();
        }
        public async Task<WorkflowInstance> GetWorkflowInstance(string Id, CancellationToken cancellationToken = default)
        {
            var entity = await _wkInstanceRepository.FindAsync(new Guid(Id), true, cancellationToken) ?? throw new UserFriendlyException(message: $"[{Id}]流程实例不存在！");
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

            var rawResult = query.OrderBy(d => d.CreationTime).Skip(skip).Take(take).ToList();
            if (rawResult == null)
                return [];
            List<WorkflowInstance> result = [];
            foreach (var item in rawResult)
                result.Add(item.ToWorkflowInstance());
            return result;
        }
        public async Task<IEnumerable<WorkflowInstance>> GetWorkflowInstances(IEnumerable<string> ids, CancellationToken cancellationToken = default)
        {
            if (ids == null)
            {
                return [];
            }
            var uids = ids.Select(i => new Guid(i)).ToList();
            var raws = await _wkInstanceRepository.GetDetails(uids);
            return raws.Select(i => i.ToWorkflowInstance());
        }
        public async Task MarkEventProcessed(string id, CancellationToken cancellationToken = default)
        {
            var uid = new Guid(id);
            var existingEntity = await _wkEventRepository.GetAsync(uid, true, cancellationToken);
            await existingEntity.SetProcessed(true);
            await _wkEventRepository.UpdateAsync(existingEntity, false, cancellationToken);
        }
        public async Task MarkEventUnprocessed(string id, CancellationToken cancellationToken = default)
        {
            var uid = new Guid(id);
            var existingEntity = await _wkEventRepository.GetAsync(uid, true, cancellationToken);
            await existingEntity.SetProcessed(false);
            await _wkEventRepository.UpdateAsync(existingEntity, false, cancellationToken);
        }
        public async Task PersistErrors(IEnumerable<ExecutionError> errors, CancellationToken cancellationToken = default)
        {
            var executionErrors = errors as ExecutionError[] ?? errors.ToArray();
            if (executionErrors.Length != 0)
            {
                foreach (var error in executionErrors)
                {
                    await _wkErrorRepository.InsertAsync(error.ToPersistable(), false, cancellationToken);
                }
            }
        }
        public async Task PersistWorkflow(WorkflowInstance workflow, CancellationToken cancellationToken = default)
        {
            using var uow = _unitOfWorkManager.Begin();
            var uid = new Guid(workflow.Id);
            var existingEntity = await _wkInstanceRepository.FindAsync(uid, true, cancellationToken);
            if (existingEntity == null)
                return;
            var persistable = await workflow.ToPersistable(existingEntity);
            existingEntity = await CreateExePointerMaterials(existingEntity, new Guid(workflow.WorkflowDefinitionId), workflow.Version, workflow.Reference);
            await _wkInstanceRepository.UpdateAsync(persistable, false, cancellationToken);
            await uow.CompleteAsync(cancellationToken);
        }
        public async Task PersistWorkflow(WorkflowInstance workflow, List<EventSubscription> subscriptions, CancellationToken cancellationToken = default)
        {
            using var uow = _unitOfWorkManager.Begin();
            var uid = new Guid(workflow.Id);
            var existingEntity = await _wkInstanceRepository.FindAsync(uid, true, cancellationToken);
            if (existingEntity == null)
                return;
            var persistable = await workflow.ToPersistable(existingEntity);
            existingEntity = await CreateExePointerMaterials(existingEntity, new Guid(workflow.WorkflowDefinitionId), workflow.Version, workflow.Reference);
            await _wkInstanceRepository.UpdateAsync(persistable, false, cancellationToken);
            //需要确认
            foreach (var subscription in subscriptions)
            {
                if (!string.IsNullOrEmpty(subscription.Id))
                {
                    var subscriptionEntity = await _wkSubscriptionRepository.FindAsync(new Guid(subscription.Id), true, cancellationToken);
                    if (subscriptionEntity != null)
                    {
                        //var scription = subscription.ToPersistable();
                        //await _wkSubscriptionRepository.UpdateAsync(scription);
                    }
                }
                else
                {
                    subscription.Id = _guidGenerator.Create().ToString();
                    var scription = subscription.ToPersistable();
                    await _wkSubscriptionRepository.InsertAsync(scription, false, cancellationToken);
                }
            }
            await uow.CompleteAsync(cancellationToken);
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
            _ = await _wkInstanceRepository.FindAsync(new Guid(workerId), true, cancellationToken) ?? throw new UserFriendlyException(message: $"[{workerId}]流程实例不存在！");
            var uid = new Guid(eventSubscriptionId);
            var existingEntity = await _wkSubscriptionRepository.GetAsync(uid, true, cancellationToken);
            await existingEntity.SetExternalToken(token);
            await existingEntity.SetExternalWorkerId(workerId);
            if (expiry > new DateTime(9999, 12, 31))
                expiry = new DateTime(9999, 12, 31);
            await existingEntity.SetExternalTokenExpiry(expiry);
            await _wkSubscriptionRepository.UpdateAsync(existingEntity, false, cancellationToken);
            return true;
        }
        public async Task TerminateSubscription(string eventSubscriptionId, CancellationToken cancellationToken = default)
        {
            var uid = new Guid(eventSubscriptionId);
            await _wkSubscriptionRepository.DeleteAsync(uid, false, cancellationToken);
        }
    }
}