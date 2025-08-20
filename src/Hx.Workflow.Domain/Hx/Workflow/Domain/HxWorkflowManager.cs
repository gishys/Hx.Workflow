using Hx.Workflow.Domain.JsonDefinition;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using Microsoft.Extensions.Logging;
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
    public class HxWorkflowManager(
        IWorkflowRegistry registry,
        IWkStepBodyRespository wkStepBodyRespository,
        IDefinitionLoader definitionLoader,
        IWorkflowController workflowService,
        IUnitOfWorkManager unitOfWorkManager,
        IWkDefinitionRespository wkDefinitionRespository,
        IWorkflowHost workflowHost,
        IWkInstanceRepository instanceRepository,
        ILogger<HxWorkflowManager> logger) : DomainService
    {
        private readonly IWorkflowRegistry _registry = registry;
        private readonly IWkStepBodyRespository _wkStepBodyRespository = wkStepBodyRespository;
        private readonly IDefinitionLoader _definitionLoader = definitionLoader;
        protected readonly IWorkflowController _workflowService = workflowService;
        private readonly IUnitOfWorkManager _unitOfWorkManager = unitOfWorkManager;
        private readonly IWkDefinitionRespository _wkDefinitionRespository = wkDefinitionRespository;
        protected readonly IWorkflowHost _workflowHost = workflowHost;
        private readonly ILogger<HxWorkflowManager> _logger = logger;
        private List<WkNode>? _WkNodes;
        public IWkInstanceRepository WkInstanceRepository { get; set; } = instanceRepository;

        // 定义支持的操作符常量
        private static readonly string[] ValidStringOperators = ["==", "!="];
        private static readonly string[] ValidNumericOperators = ["==", "!=", ">", "<", ">=", "<="];

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
        public virtual async Task<WkDefinition?> GetDefinitionAsync(string name)
        {
            return await _wkDefinitionRespository.GetDefinitionAsync(name);
        }
        /// <summary>
        ///  initialize process registration
        /// </summary>
        public async virtual Task Initialize()
        {
            using var uow = _unitOfWorkManager.Begin(
                requiresNew: true, isTransactional: false
            );
            var workflows = await _wkDefinitionRespository.GetListAsync(includeDetails: true);
            foreach (var workflow in workflows)
            {
                LoadDefinitionByJson(workflow);
            }
            await uow.CompleteAsync();
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
        public virtual Task UpdateAsync(WkDefinition entity)
        {
            // 对于新版本，直接注册到工作流引擎
            if (_registry.IsRegistered(entity.Id.ToString(), entity.Version))
            {
                _registry.DeregisterWorkflow(entity.Id.ToString(), entity.Version);
            }
            LoadDefinitionByJson(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 更新现有版本
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task UpdateExistingVersionAsync(WkDefinition entity)
        {
            var wkDefinitionSource = await _wkDefinitionRespository.UpdateAsync(entity);
            if (_registry.IsRegistered(entity.Id.ToString(), entity.Version))
            {
                _registry.DeregisterWorkflow(entity.Id.ToString(), entity.Version);
            }
            LoadDefinitionByJson(wkDefinitionSource);
        }
        /// <summary>
        /// 启动工作流
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<string> StartWorkflowAsync(string id, int version, Dictionary<string, object> inputs)
        {
            if (!_registry.IsRegistered(id, version))
            {
                throw new UserFriendlyException(message: "the workflow  has not been defined!");
            }
            return await _workflowHost.StartWorkflow(id, version, inputs?.Count > 0 ? inputs : null);
        }
        public virtual async Task StartActivityAsync(
            string actName, string workflowId, object? data = null)
        {

            var activity = await _workflowHost.GetPendingActivity(actName, workflowId);
            if (activity != null)
            {
                Console.WriteLine(activity.Parameters);
                await _workflowHost.SubmitActivitySuccess(activity.Token, data);
            }
            else
            {
                throw new UserFriendlyException(message: $"{actName} was not found.");
            }
        }
        internal virtual WorkflowDefinition LoadDefinitionByJson(WkDefinition input)
        {
            if (_registry.IsRegistered(input.Id.ToString(), input.Version))
            {
                throw new AbpException($"the workflow {input.Id} has ben defined!");
            }
            var definitionSource = new JDefinitionSource
            {
                Id = input.Id.ToString(),
                Version = input.Version,
                Description = input.Title,
                DataType = $"{typeof(Dictionary<string, object>).FullName}, {typeof(Dictionary<string, object>).Assembly.FullName}"
            };
            BuildWorkflowStep(input.Nodes, definitionSource);
            string json = System.Text.Json.JsonSerializer.Serialize(definitionSource);
            return _definitionLoader.LoadDefinition(json, Deserializers.Json);
        }

        internal virtual void BuildWorkflowStep(
            IEnumerable<WkNode> allNodes,
            JDefinitionSource source)
        {
            _WkNodes = [];
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
            _logger.LogDebug("开始构建分支逻辑 - 步骤: {StepName} (ID: {StepId})", step.Name, step.Id);

            var stepSource = new JStepSource
            {
                Id = step.Id.ToString(),
                Name = step.Name
            };
            var stepBody = step.StepBody;
            stepBody ??= new WkStepBody(
                    "",
                    "",
                    null,
                    [],
                    typeof(NullStepBody).FullName ?? "",
                    typeof(NullStepBody).Assembly.FullName ?? ""
                    );
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
#pragma warning disable CS8602 // 解引用可能出现空引用。
            _WkNodes.Add(step);
#pragma warning restore CS8602 // 解引用可能出现空引用。
            source.Steps.Add(stepSource);

            if (step.NextNodes?.Count > 0)
            {
                _logger.LogDebug("处理步骤 {StepName} 的 {NextNodeCount} 个后续节点", step.Name, step.NextNodes.Count);

                foreach (var nextName in step.NextNodes)
                {
                    try
                    {
                        ProcessNextNode(allNodes, source, step, stepSource, nextName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "处理后续节点时发生错误 - 步骤: {StepName}, 后续节点: {NextNodeName}",
                            step.Name, nextName.NextNodeName);
                        throw;
                    }
                }
            }
            else
            {
                _logger.LogDebug("步骤 {StepName} 没有后续节点", step.Name);
            }
        }

        /// <summary>
        /// 处理单个后续节点
        /// </summary>
        private void ProcessNextNode(
            IEnumerable<WkNode> allNodes,
            JDefinitionSource source,
            WkNode step,
            JStepSource stepSource,
            WkNodeRelation nextName)
        {
            _logger.LogDebug("处理后续节点: {NextNodeName} -> {StepName}", nextName.NextNodeName, step.Name);

            var subStepNode = allNodes.FirstOrDefault(d => d.Name == nextName.NextNodeName);
            if (subStepNode == null)
            {
                _logger.LogWarning("未找到后续节点: {NextNodeName} (步骤: {StepName})", nextName.NextNodeName, step.Name);
                return;
            }

            // 设置默认条件表达式
            stepSource.SelectNextStep[subStepNode.Id.ToString()] = "1==1";

            // 处理规则条件
            if (nextName.Rules?.Count > 0)
            {
                try
                {
                    var expression = BuildConditionExpression(nextName.Rules, step.Name, nextName.NextNodeName);
                    stepSource.SelectNextStep[subStepNode.Id.ToString()] = expression;

                    _logger.LogDebug("为节点 {StepName} -> {NextNodeName} 设置条件表达式: {Expression}",
                        step.Name, nextName.NextNodeName, expression);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "构建条件表达式失败 - 步骤: {StepName}, 后续节点: {NextNodeName}",
                        step.Name, nextName.NextNodeName);
                    throw;
                }
            }
            else
            {
                _logger.LogDebug("节点 {StepName} -> {NextNodeName} 没有规则条件，使用默认表达式",
                    step.Name, nextName.NextNodeName);
            }

            // 递归处理后续节点（避免循环引用）
            if (_WkNodes != null && _WkNodes.Exists(d => d == subStepNode))
            {
                _logger.LogDebug("检测到循环引用，跳过节点: {NextNodeName}", nextName.NextNodeName);
                return;
            }

            BuildBranching(allNodes, source, subStepNode);
        }

        /// <summary>
        /// 构建条件表达式
        /// </summary>
        private string BuildConditionExpression(ICollection<WkNodeRelationRule> rules, string stepName, string nextNodeName)
        {
            if (rules == null || rules.Count == 0)
            {
                return "1==1";
            }

            _logger.LogDebug("开始构建条件表达式 - 规则数量: {RuleCount}, 步骤: {StepName} -> {NextNodeName}",
                rules.Count, stepName, nextNodeName);

            var expressions = new List<string>();
            var ruleIndex = 0;

            foreach (var rule in rules)
            {
                ruleIndex++;
                try
                {
                    ValidateRule(rule, stepName, nextNodeName, ruleIndex);
                    var expression = BuildSingleRuleExpression(rule, stepName, nextNodeName, ruleIndex);
                    expressions.Add(expression);

                    _logger.LogDebug("规则 {RuleIndex}: 字段={Field}, 操作符={Operator}, 值={Value} -> 表达式={Expression}",
                        ruleIndex, rule.Field, rule.Operator, rule.Value, expression);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理规则 {RuleIndex} 时发生错误 - 字段: {Field}, 操作符: {Operator}, 值: {Value}",
                        ruleIndex, rule.Field, rule.Operator, rule.Value);
                    throw;
                }
            }

            var finalExpression = string.Join(" && ", expressions);
            _logger.LogDebug("最终条件表达式: {FinalExpression}", finalExpression);

            return finalExpression;
        }

        /// <summary>
        /// 验证单个规则
        /// </summary>
        private void ValidateRule(WkNodeRelationRule rule, string stepName, string nextNodeName, int ruleIndex)
        {
            // 验证字段名
            if (string.IsNullOrWhiteSpace(rule.Field))
            {
                throw new AbpException($"规则 {ruleIndex} 的字段名不能为空 - 步骤: {stepName} -> {nextNodeName}");
            }

            // 验证操作符
            if (string.IsNullOrWhiteSpace(rule.Operator))
            {
                throw new AbpException($"规则 {ruleIndex} 的操作符不能为空 - 字段: {rule.Field}, 步骤: {stepName} -> {nextNodeName}");
            }

            // 验证值
            if (rule.Value == null)
            {
                throw new AbpException($"规则 {ruleIndex} 的值不能为null - 字段: {rule.Field}, 步骤: {stepName} -> {nextNodeName}");
            }

            _logger.LogDebug("规则 {RuleIndex} 验证通过 - 字段: {Field}, 操作符: {Operator}, 值: {Value}",
                ruleIndex, rule.Field, rule.Operator, rule.Value);
        }

        /// <summary>
        /// 构建单个规则的表达式
        /// </summary>
        private static string BuildSingleRuleExpression(WkNodeRelationRule rule, string stepName, string nextNodeName, int ruleIndex)
        {
            // 尝试解析为数值类型
            if (decimal.TryParse(rule.Value, out decimal numericValue))
            {
                return BuildNumericExpression(rule, numericValue, stepName, nextNodeName, ruleIndex);
            }
            else
            {
                return BuildStringExpression(rule, stepName, nextNodeName, ruleIndex);
            }
        }

        /// <summary>
        /// 构建数值类型表达式
        /// </summary>
        private static string BuildNumericExpression(WkNodeRelationRule rule, decimal numericValue, string stepName, string nextNodeName, int ruleIndex)
        {
            // 验证数值操作符
            if (!ValidNumericOperators.Contains(rule.Operator))
            {
                throw new AbpException($"规则 {ruleIndex}:数值字段 '{rule.Field}' 不支持操作符 '{rule.Operator}'。支持的操作符: {string.Join(", ", ValidNumericOperators)} - 步骤: {stepName} -> {nextNodeName}");
            }

            // 使用 Convert.ToDecimal 替代 decimal.Parse 和 ToString，避免 System.Linq.Dynamic.Core 的访问限制
            return $"Convert.ToDecimal(data[\"{rule.Field}\"]) {rule.Operator} {numericValue}";
        }

        /// <summary>
        /// 构建字符串类型表达式
        /// </summary>
        private static string BuildStringExpression(WkNodeRelationRule rule, string stepName, string nextNodeName, int ruleIndex)
        {
            // 验证字符串操作符
            if (!ValidStringOperators.Contains(rule.Operator))
            {
                throw new AbpException($"规则 {ruleIndex}:字符串字段 '{rule.Field}' 不支持操作符 '{rule.Operator}'。支持的操作符: {string.Join(", ", ValidStringOperators)} - 步骤: {stepName} -> {nextNodeName}");
            }

            // 转义字符串值中的引号，防止表达式解析错误
            var escapedValue = rule.Value.Replace("\"", "\\\"");

            // 使用 Convert.ToString 替代 ToString，避免 System.Linq.Dynamic.Core 的访问限制
            return $"Convert.ToString(data[\"{rule.Field}\"]) {rule.Operator} \"{escapedValue}\"";
        }
        private static void GetValue(WkStepBodyParam input, IDictionary<string, object> dics)
        {
            var value = input.Value;
            if (value != null)
            {
                IDictionary<object, object>? value2 = value.ToDictionaryObject();
                IDictionary<string, object>? value1 = value.ToDictionaryString();
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
