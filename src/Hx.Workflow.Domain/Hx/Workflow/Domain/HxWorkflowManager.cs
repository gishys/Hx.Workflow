using Hx.Workflow.Domain.JsonDefinition;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Services.DefinitionStorage;

namespace Hx.Workflow.Domain
{
    public class HxWorkflowManager : DomainService
    {
        private readonly IWorkflowRegistry _registry;
        private readonly IWkStepBodyRespository _wkStepBodyRespository;
        private readonly IDefinitionLoader _definitionLoader;
        protected readonly IWorkflowController _workflowService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IWkDefinitionRespository _wkDefinitionRespository;
        protected readonly IWorkflowHost _workflowHost;
        private List<WkNode> _WkNodes;
        public IWkInstanceRepository WkInstanceRepository { get; set; }
        public HxWorkflowManager(
            IWorkflowRegistry registry,
            IWkStepBodyRespository wkStepBodyRespository,
            IDefinitionLoader definitionLoader,
            IWorkflowController workflowService,
            IUnitOfWorkManager unitOfWorkManager,
            IWkDefinitionRespository wkDefinitionRespository,
            IWorkflowHost workflowHost,
            IWkInstanceRepository instanceRepository)
        {
            _registry = registry;
            _wkStepBodyRespository = wkStepBodyRespository;
            _definitionLoader = definitionLoader;
            _workflowService = workflowService;
            _unitOfWorkManager = unitOfWorkManager;
            _wkDefinitionRespository = wkDefinitionRespository;
            _workflowHost = workflowHost;
            WkInstanceRepository = instanceRepository;
        }
        /// <summary>
        /// terminate workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> TerminateWorkflowAsync(string workflowId)
        {
            return await _workflowService.TerminateWorkflow(workflowId);
        }
        /// <summary>
        /// suspend workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> SuspendWorkflowAsync(string workflowId)
        {
            return await _workflowService.SuspendWorkflow(workflowId);
        }
        /// <summary>
        /// resume workflow
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> ResumeWorkflowAsync(string workflowId)
        {
            return await _workflowService.ResumeWorkflow(workflowId);
        }
        /// <summary>
        /// by workflow id get definition
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinition> GetDefinitionAsync(string name)
        {
            return await _wkDefinitionRespository.GetDefinitionAsync(name);
        }
        /// <summary>
        ///  initialize process registration
        /// </summary>
        public async virtual Task Initialize()
        {
            using (var uow = _unitOfWorkManager.Begin(
                requiresNew: true, isTransactional: false
            ))
            {
                var workflows = await _wkDefinitionRespository.GetListAsync(includeDetails: true);
                foreach (var workflow in workflows)
                {
                    LoadDefinitionByJson(workflow);
                }
                await uow.CompleteAsync();
            }
        }
        public async virtual Task StartHostAsync()
        {
            await _workflowHost.StartAsync(new CancellationToken());
        }
        public async virtual Task StopAsync()
        {
            await _workflowHost.StopAsync(new CancellationToken());
        }
        public async virtual Task<IEnumerable<WkStepBody>> GetAllStepBodys()
        {
            return await _wkStepBodyRespository.GetListAsync();
        }
        public virtual async Task PublishEventAsync(string eventName, string eventKey, object eventData)
        {
            await _workflowHost.PublishEvent(eventName, eventKey, eventData);
        }
        /// <summary>
        /// 创建流程
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task CreateAsync(WkDefinition entity)
        {
            var rEntity = await _wkDefinitionRespository.InsertAsync(entity);
            LoadDefinitionByJson(rEntity);
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task UpdateAsync(WkDefinition entity)
        {
            var wkDefinitionSource = await _wkDefinitionRespository.FindAsync(entity.Id);

            if (_registry.IsRegistered(entity.Id.ToString(), entity.Version))
            {
                _registry.DeregisterWorkflow(entity.Id.ToString(), entity.Version);
            }
            LoadDefinitionByJson(wkDefinitionSource);
            await _wkDefinitionRespository.UpdateAsync(entity);
        }
        /// <summary>
        /// 启动工作流
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<string> StartWorlflowAsync(string id, int version, Dictionary<string, object> inputs)
        {
            if (!_registry.IsRegistered(id, version))
            {
                throw new UserFriendlyException("the workflow  has not been defined!");
            }
            return await _workflowHost.StartWorkflow(id, version, inputs?.Count > 0 ? inputs : null);
        }
        public virtual async Task StartActivityAsync(
            string actName, string workflowId, object data = null)
        {

            var activity = await _workflowHost.GetPendingActivity(actName, workflowId);
            if (activity != null)
            {
                Console.WriteLine(activity.Parameters);
                await _workflowHost.SubmitActivitySuccess(activity.Token, data);
            }
            else
            {
                throw new UserFriendlyException($"{actName} was not found.");
            }
        }
        internal virtual WorkflowDefinition LoadDefinitionByJson(WkDefinition input)
        {
            if (_registry.IsRegistered(input.Id.ToString(), input.Version))
            {
                throw new AbpException($"the workflow {input.Id} has ben defined!");
            }
            var definitionSource = new JDefinitionSource();
            definitionSource.Id = input.Id.ToString();
            definitionSource.Version = input.Version;
            definitionSource.Description = input.Title;
            definitionSource.DataType = $"{typeof(Dictionary<string, object>).FullName}, {typeof(Dictionary<string, object>).Assembly.FullName}";
            BuildWorkflowStep(input.Nodes, definitionSource);
            string json = System.Text.Json.JsonSerializer.Serialize(definitionSource);
            return _definitionLoader.LoadDefinition(json, Deserializers.Json);
        }

        internal virtual void BuildWorkflowStep(
            IEnumerable<WkNode> allNodes,
            JDefinitionSource source)
        {
            _WkNodes = new List<WkNode>();
            if (allNodes.Any(d => d.StepNodeType == StepNodeType.Start))
                BuildBranching(
                    allNodes,
                    source,
                    allNodes.First(d => d.StepNodeType == StepNodeType.Start));
        }
        public virtual void BuildBranching(
            IEnumerable<WkNode> allNodes,
            JDefinitionSource source,
            WkNode step)
        {
            var stepSource = new JStepSource();
            stepSource.Id = step.Id.ToString();
            stepSource.Name = step.Name;
            var stepBody = step.StepBody;
            if (stepBody == null)
            {
                stepBody = new WkStepBody(
                    "",
                    "",
                    "",
                    null,
                    typeof(NullStepBody).FullName,
                    typeof(NullStepBody).Assembly.FullName
                    );
            }
            stepSource.StepType = $"{stepBody.TypeFullName}, {stepBody.AssemblyFullName}";
            if (stepBody == null)
                throw new BusinessException(message: "Definition step body is null!");
            if (stepBody.Inputs?.Count > 0)
            {
                foreach (var input in stepBody.Inputs)
                {
                    if (input.StepBodyParaType == StepBodyParaType.Inputs)
                        GetValue(input, stepSource.Inputs);
                    else if (input.StepBodyParaType == StepBodyParaType.Outputs)
                        GetValue(input, stepSource.Outputs);
                }
            }
            _WkNodes.Add(step);
            source.Steps.Add(stepSource);
            if (step.NextNodes?.Count > 0)
            {
                foreach (var nextName in step.NextNodes)
                {
                    var subStepNode = allNodes.FirstOrDefault(d => d.Name == nextName.NextNodeName);
                    if (subStepNode != null)
                    {
                        stepSource.SelectNextStep[subStepNode.Id.ToString()] = "1==1";
                        if (nextName.WkConNodeConditions?.Count > 0)
                        {
                            List<string> exps = new List<string>();
                            foreach (var cond in nextName.WkConNodeConditions)
                            {
                                if ((!decimal.TryParse(cond.Value, out decimal tempValue)) && cond.Value is string)
                                {
                                    if (cond.Operator != "==" && cond.Operator != "!=")
                                    {
                                        throw new AbpException($" if {cond.Field} is type of 'String', the Operator must be \"==\" or \"!=\"");
                                    }
                                    exps.Add($"data[\"{cond.Field}\"].ToString() {cond.Operator} \"{cond.Value}\"");
                                    continue;
                                }
                                exps.Add($"decimal.Parse(data[\"{cond.Field}\"].ToString()) {cond.Operator} {cond.Value}");
                            }
                            stepSource.SelectNextStep[subStepNode.Id.ToString()] = string.Join(" && ", exps);
                        }
                        if (_WkNodes.Exists(d => d == subStepNode))
                            continue;
                        BuildBranching(allNodes, source, subStepNode);
                    }
                }
            }
        }
        private void GetValue(IHxKeyValueConvert input, IDictionary<string, object> dics)
        {
            var value = input.Value;
            if (value != null)
            {
                IDictionary<object, object> value2 = value.ToDictionaryObject();
                IDictionary<string, object> value1 = value.ToDictionaryString();
                if (value2 != null)
                {
                    dics.TryAdd(input.Key, value2);
                }
                else if (value1 != null)
                {
                    dics.TryAdd(input.Key, value1);
                }
                else if (value.Contains("data"))
                {
                    dics.TryAdd(input.Key, value);
                }
                else if (value.Contains("DateTime.Now"))
                {
                    dics.TryAdd(input.Key, value);
                }
                else if (value.Contains("step"))
                {
                    dics.TryAdd(input.Key, value);
                }
                else
                {
                    value = $"\"{value}\"";
                    dics.TryAdd(input.Key, value);
                }
            }
        }
    }
}