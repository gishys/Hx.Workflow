using Hx.Workflow.Application.BusinessModule;
using Hx.Workflow.Application.Contracts;
using Hx.Workflow.Domain;
using Hx.Workflow.Domain.BusinessModule;
using Hx.Workflow.Domain.Persistence;
using Hx.Workflow.Domain.Repositories;
using Hx.Workflow.Domain.Shared;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using WorkflowCore.Models;
using SharpYaml;
using Volo.Abp.ObjectMapping;
using System.Reflection;
using Hx.Workflow.Domain.Stats;
using Hx.Workflow.Domain.StepBodys;
using static Volo.Abp.Identity.Settings.IdentitySettingNames;

namespace Hx.Workflow.Application
{
    public class WorkflowAppService : HxWorkflowAppServiceBase, IWorkflowAppService
    {
        private readonly IWkStepBodyRespository _wkStepBody;
        private readonly HxWorkflowManager _hxWorkflowManager;
        private readonly IWkDefinitionRespository _wkDefinition;
        private readonly IWkInstanceRepository _wkInstanceRepository;
        private readonly IWkErrorRepository _errorRepository;
        private readonly IWkExecutionPointerRepository _wkExecutionPointerRepository;
        private readonly IWkAuditorRespository _wkAuditor;
        public WorkflowAppService(
            IWkStepBodyRespository wkStepBody,
            HxWorkflowManager hxWorkflowManager,
            IWkDefinitionRespository wkDefinition,
            IWkInstanceRepository wkInstanceRepository,
            IWkErrorRepository errorRepository,
            IWkExecutionPointerRepository wkExecutionPointerRepository,
            IWkAuditorRespository wkAuditor)
        {
            _wkStepBody = wkStepBody;
            _hxWorkflowManager = hxWorkflowManager;
            _wkDefinition = wkDefinition;
            _wkInstanceRepository = wkInstanceRepository;
            _errorRepository = errorRepository;
            _wkExecutionPointerRepository = wkExecutionPointerRepository;
            _wkAuditor = wkAuditor;
        }
        /// <summary>
        /// 创建流程模版
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public virtual async Task CreateAsync(WkDefinitionCreateDto input)
        {
            var sortNumber = await _wkDefinition.GetMaxSortNumberAsync();
            var entity = new WkDefinition(
                    input.Title,
                    input.Icon,
                    input.Color,
                    sortNumber,
                    input.Discription,
                    input.BusinessType,
                    input.ProcessType,
                    limitTime: input.LimitTime,
                    version: input.Version <= 0 ? 1 : input.Version);
            foreach (var candidate in input.WkCandidates)
            {
                entity.WkCandidates.Add(new DefinitionCandidate(
                    candidate.CandidateId,
                    candidate.UserName,
                    candidate.DisplayUserName,
                    candidate.ExecutorType,
                    candidate.DefaultSelection));
            }
            var nodeEntitys = input.Nodes.ToWkNodes();
            foreach (var node in nodeEntitys)
            {
                var wkStepBodyId = input.Nodes.FirstOrDefault(d => d.Name == node.Name).WkStepBodyId;
                if (wkStepBodyId?.Length > 0)
                {
                    Guid.TryParse(wkStepBodyId, out Guid guidStepBodyId);
                    var stepBodyEntity = await _wkStepBody.FindAsync(guidStepBodyId);
                    if (stepBodyEntity == null)
                        throw new BusinessException(message: "StepBody没有查询到");
                    await node.SetWkStepBody(stepBodyEntity);
                }
                await entity.AddWkNode(node);
            }
            await _hxWorkflowManager.CreateAsync(entity);
        }
        /// <summary>
        /// 获取流程模版
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> GetDefinitionByNameAsync(string name)
        {
            var entity = await _hxWorkflowManager.GetDefinitionAsync(name);
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
        }
        /// <summary>
        /// 获取流程模版
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> GetDefinitionAsync(Guid id, int version = 1)
        {
            var entity = await _wkDefinition.GetDefinitionAsync(id, version);
            var result = ObjectMapper.Map<WkDefinition, WkDefinitionDto>(entity);
            if (result != null && result.Nodes.Count > 0)
                result.Nodes = result.Nodes.OrderBy(n => n.SortNumber).ToList();
            return result;
        }
        /// <summary>
        /// 获取全部流程模版
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<WkDefinitionDto>> GetDefinitionsAsync()
        {
            var entitys = await _wkDefinition.GetListAsync(true);
            return ObjectMapper.Map<List<WkDefinition>, List<WkDefinitionDto>>(entitys);
        }
        /// <summary>
        /// 获取可创建的模板（赋予权限）
        /// </summary>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public virtual async Task<List<WkDefinitionDto>> GetDefinitionsCanCreateAsync()
        {
            if (CurrentUser.Id.HasValue)
            {
                var entitys = await _wkDefinition.GetListHasPermissionAsync(CurrentUser.Id.Value);
                return ObjectMapper.Map<List<WkDefinition>, List<WkDefinitionDto>>(entitys);
            }
            throw new UserFriendlyException("未获取到当前登录用户！");
        }
        /// <summary>
        /// 通过流程模版Id创建流程
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual async Task<string> StartAsync(StartWorkflowInput input)
        {
            try
            {
                if (!input.Inputs.Any(d => d.Key == "Candidates" && Guid.TryParse(d.Value.ToString(), out _)))
                {
                    throw new UserFriendlyException("请传入有效的接收人Id！");
                }
                var entity = await _wkDefinition.GetDefinitionAsync(new Guid(input.Id), input.Version);
                if (!entity.WkCandidates.Any(d => d.CandidateId == new Guid(input.Inputs["Candidates"].ToString())))
                {
                    throw new UserFriendlyException($"无权限，请在流程定义中配置Id为（{input.Inputs["Candidates"].ToString()}）的权限！");
                }
                return await _hxWorkflowManager.StartWorlflowAsync(input.Id, input.Version, input.Inputs);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
        /// <summary>
        /// 开始活动
        /// </summary>
        /// <param name="actName"></param>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task StartActivityAsync(string actName, string workflowId, Dictionary<string, object> data = null)
        {
            var eventPointerEventData = JsonSerializer.Deserialize<WkPointerEventData>(JsonSerializer.Serialize(data));
            if (string.IsNullOrEmpty(eventPointerEventData.DecideBranching))
                throw new UserFriendlyException("提交必须携带分支节点名称！");
            await _hxWorkflowManager.StartActivityAsync(actName, workflowId, data);
        }
        /// <summary>
        /// 查询我的办理件（在办、废弃、已完成、挂起），如果为空则查询所有
        /// </summary>
        /// <param name="status"></param>
        /// <param name="userIds"></param>
        /// <param name="skipCount"></param>
        /// <param name="maxResultCount"></param>
        /// <returns></returns>
        public virtual async Task<PagedResultDto<WkProcessInstanceDto>> GetMyWkInstanceAsync(
            MyWorkState? status = null,
            string reference = null,
            ICollection<Guid> userIds = null,
            int skipCount = 0,
            int maxResultCount = 20)
        {
            if ((userIds == null || userIds.Count == 0) && CurrentUser.Id.HasValue)
            {
                userIds = [CurrentUser.Id.Value];
            }
            //userIds = [new Guid("3a140076-3f3e-0ae6-56ad-2c3f1c06508f")];
            List<WkProcessInstanceDto> result = [];
            var instances = await _hxWorkflowManager.WkInstanceRepository.GetMyInstancesAsync(
                userIds,
                reference,
                status,
                skipCount,
                maxResultCount);
            var count = await _wkInstanceRepository.GetMyInstancesCountAsync(userIds, reference, status);
            foreach (var instance in instances)
            {
                var processInstance = instance.ToProcessInstanceDto(userIds);
                result.Add(processInstance);
            }
            return new PagedResultDto<WkProcessInstanceDto>(count, result);
        }
        /// <summary>
        /// 通过业务编号获得实例详细信息
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public virtual async Task<WkCurrentInstanceDetailsDto> GetWkInstanceAsync(string reference)
        {
            var instance = await _wkInstanceRepository.GetByReferenceAsync(reference);
            var businessData = JsonSerializer.Deserialize<WkInstanceEventData>(instance.Data);
            WkExecutionPointer pointer = instance.ExecutionPointers.First(d => d.Status != PointerStatus.Complete);
            var errors = await _errorRepository.GetListByIdAsync(instance.Id, pointer.Id);
            return instance.ToWkInstanceDetailsDto(ObjectMapper, businessData, pointer, CurrentUser.Id, errors);
        }
        /// <summary>
        /// 签收实例
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        /// <exception cref="UserFriendlyException"></exception>
        public virtual async Task RecipientInstanceAsync(Guid workflowId)
        {
            var userId = CurrentUser.Id;
            //Guid? userId = new Guid("3a140076-3f3e-0ae6-56ad-2c3f1c06508f");
            if (userId.HasValue)
            {
                await _wkInstanceRepository.RecipientExePointerAsync(workflowId, userId.Value);
            }
            else
            {
                throw new UserFriendlyException("未获取到当前登录用户！");
            }
        }
        /// <summary>
        /// 获得实例详细信息
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="pointerId"></param>
        /// <returns></returns>
        public virtual async Task<WkCurrentInstanceDetailsDto> GetInstanceAsync(Guid workflowId, Guid? pointerId)
        {
            var instance = await _wkInstanceRepository.FindAsync(workflowId);
            var businessData = JsonSerializer.Deserialize<WkInstanceEventData>(instance.Data);
            WkExecutionPointer pointer;
            if (pointerId.HasValue)
            {
                pointer = instance.ExecutionPointers.First(d => d.Id == pointerId.Value);
            }
            else
            {
                pointer = instance.ExecutionPointers.First(d => d.Status != PointerStatus.Complete);
            }
            var errors = await _errorRepository.GetListByIdAsync(workflowId, pointer.Id);
            return instance.ToWkInstanceDetailsDto(ObjectMapper, businessData, pointer, CurrentUser.Id, errors);
        }
        /// <summary>
        /// 获得实例节点（包含已完成及正在办理的节点）
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<List<WkNodeTreeDto>> GetInstanceNodesAsync(Guid workflowId)
        {
            var instance = await _wkInstanceRepository.FindAsync(workflowId);
            var result = new List<WkNodeTreeDto>();
            string preId = null;
            while (instance.ExecutionPointers.Any(d => d.PredecessorId == preId))
            {
                var node = instance.ExecutionPointers.First(d => d.PredecessorId == preId);
                result.Add(new WkNodeTreeDto()
                {
                    Key = node.Id,
                    Title = instance.WkDefinition.Nodes.First(d => d.Name == node.StepName).DisplayName,
                    Selected = node.Status != PointerStatus.Complete,
                    Name = node.StepName,
                    Receiver = node.Recipient,
                    CommitmentDeadline = node.CommitmentDeadline,
                    Status = (int)node.Status,
                    WkCandidates = ObjectMapper.Map<ICollection<ExePointerCandidate>, ICollection<WkPointerCandidateDto>>(node.WkCandidates)
                });
                preId = node.Id.ToString();
            }
            return result;
        }
        /// <summary>
        /// 获得实例节点（流程所有节点）
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<List<WkNodeTreeDto>> GetInstanceAllNodesAsync(Guid workflowId)
        {
            var instance = await _wkInstanceRepository.FindAsync(workflowId);
            var result = new List<WkNodeTreeDto>();
            string preId = null;
            WkExecutionPointer currentNode = null;
            while (instance.ExecutionPointers.Any(d => d.PredecessorId == preId))
            {
                var node = instance.ExecutionPointers.First(d => d.PredecessorId == preId);
                result.Add(new WkNodeTreeDto()
                {
                    Key = node.Id,
                    Title = instance.WkDefinition.Nodes.First(d => d.Name == node.StepName).DisplayName,
                    Selected = node.Status != PointerStatus.Complete,
                    Name = node.StepName,
                    Receiver = node.Recipient,
                    CommitmentDeadline = node.CommitmentDeadline,
                    Status = (int)node.Status,
                    WkCandidates = ObjectMapper.Map<ICollection<ExePointerCandidate>, ICollection<WkPointerCandidateDto>>(node.WkCandidates)
                });
                preId = node.Id.ToString();
                currentNode = node;
            }
            if (currentNode.Status != PointerStatus.Complete)
            {
                var entity = await _wkDefinition.GetDefinitionAsync(instance.WkDifinitionId, instance.Version);
                var node = entity.Nodes.First(d => d.Name == currentNode.StepName);
                do
                {
                    var nextNode = node.NextNodes.FirstOrDefault(n => n.NodeType == WkRoleNodeType.Forward);
                    if (nextNode != null)
                    {
                        node = entity.Nodes.First(d => d.Name == nextNode.NextNodeName);
                        result.Add(new WkNodeTreeDto()
                        {
                            Key = node.Id,
                            Title = node.DisplayName,
                            Selected = false,
                            Name = node.Name,
                            Receiver = null,
                            CommitmentDeadline = null,
                            Status = 20,
                            WkCandidates = ObjectMapper.Map<ICollection<WkNodeCandidate>, ICollection<WkPointerCandidateDto>>(node.WkCandidates)
                        });
                    }
                }
                while (node.NextNodes.Any(n => n.NodeType == WkRoleNodeType.Forward));
            }
            return result;
        }
        /// <summary>
        /// 终止工作流
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> TerminateWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.TerminateWorkflowAsync(workflowId);
        }
        /// <summary>
        /// 挂起工作流
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> SuspendWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.SuspendWorkflowAsync(workflowId);
        }
        /// <summary>
        /// 恢复工作流
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public virtual async Task<bool> ResumeWorkflowAsync(string workflowId)
        {
            return await _hxWorkflowManager.ResumeWorkflowAsync(workflowId);
        }
        /// <summary>
        /// 更新模板定义候选人
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<WkDefinitionDto> UpdateDefinitionAsync(WkDefinitionUpdateDto entity)
        {
            var wkCandidates = entity.WkCandidates.ToWkCandidate();
            var resultEntity = await _wkDefinition.UpdateCandidatesAsync(entity.Id, entity.UserId, wkCandidates as ICollection<DefinitionCandidate>);
            return ObjectMapper.Map<WkDefinition, WkDefinitionDto>(resultEntity);
        }
        /// <summary>
        /// 更新实例候选人（委托、抄送、会签）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual async Task<WkInstancesDto> UpdateInstanceCandidatesAsync(WkInstanceUpdateDto entity)
        {
            var instance = await _wkInstanceRepository.FindAsync(entity.WkInstanceId);
            WkExecutionPointer pointer = instance.ExecutionPointers.First(d => d.Id == entity.WkExecutionPointerId);
            var step = instance.WkDefinition.Nodes.First(d => d.Name == pointer.StepName);
            if (pointer.Status != PointerStatus.WaitingForEvent)
            {
                throw new UserFriendlyException("当前流程环节不是活动节点！");
            }
            if (!entity.WkCandidates.All(id => step.WkCandidates.Any(d => d.CandidateId == id)))
            {
                throw new UserFriendlyException("选择的用户没有办理此节点的权限！");
            }
            var wkCandidates = step.WkCandidates.Where(d => entity.WkCandidates.Any(f => f == d.CandidateId)).Select(d =>
                new ExePointerCandidate(
                    d.CandidateId,
                    d.UserName,
                    d.DisplayUserName,
                    entity.ExeOperateType,
                    ExeCandidateState.WaitingReceipt,
                    d.ExecutorType)).ToList();
            var resultEntity = await _wkInstanceRepository.UpdateCandidateAsync(
                entity.WkInstanceId,
                entity.WkExecutionPointerId,
                wkCandidates,
                entity.ExeOperateType);
            var result = ObjectMapper.Map<WkInstance, WkInstancesDto>(resultEntity);
            result.CurrentStepName = step.DisplayName;
            result.StepStartTime = pointer.StartTime;
            return result;
        }
        /// <summary>
        /// 通过实例Id获取可选择的人员
        /// </summary>
        /// <param name="wkInstanceId"></param>
        /// <returns></returns>
        public virtual async Task<ICollection<WkCandidateDto>> GetCandidatesAsync(Guid wkInstanceId)
        {
            var entitys = await _wkInstanceRepository.GetCandidatesAsync(wkInstanceId);
            return ObjectMapper.Map<ICollection<ExePointerCandidate>, ICollection<WkCandidateDto>>(entitys);
        }
        /// <summary>
        /// 流程实例添加业务数据
        /// </summary>
        /// <param name="workflowId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task UpdateInstanceBusinessDataAsync(InstanceBusinessDataInput input)
        {
            var data = new Dictionary<string, object>() {
                { "ProcessName", input.ProcessName },
                { "Located", input.Located }
                };
            await _wkInstanceRepository.UpdateDataAsync(input.WorkflowId, data);
        }
        /// <summary>
        /// 关注实例（取消关注）
        /// </summary>
        /// <param name="pointerId"></param>
        /// <param name="follow"></param>
        /// <returns></returns>
        public virtual async Task FollowAsync(Guid pointerId, bool follow)
        {
            if (CurrentUser.Id.HasValue)
            {
                var pointer = await _wkExecutionPointerRepository.FindAsync(pointerId);
                var exeCandidate = pointer.WkCandidates.Where(d => d.CandidateId == CurrentUser.Id.Value).FirstOrDefault();
                if (exeCandidate != null)
                {
                    await exeCandidate.SetFollow(follow);
                    await _wkExecutionPointerRepository.UpdateAsync(pointer);
                }
            }
        }
        /// <summary>
        /// 标记初始化物料
        /// </summary>
        /// <param name="wkinstanceId"></param>
        /// <param name="executionPointerId"></param>
        /// <returns></returns>
        public virtual async Task InitMaterialsAsync(Guid executionPointerId)
        {
            await _wkExecutionPointerRepository.InitMaterialsAsync(executionPointerId);
        }
        /// <summary>
        /// 计算我的工作状态数量
        /// </summary>
        /// <param name="transactorId"></param>
        /// <returns></returns>
        public virtual async Task<List<ProcessingStatusStat>> GetProcessingStatusStatListAsync(Guid? transactorId)
        {
            if (CurrentUser.Id.HasValue)
            {
                transactorId = CurrentUser.Id.Value;
            }
            return await _wkInstanceRepository.GetProcessingStatusStatListAsync(transactorId.Value);
        }
        public virtual Task<List<ProcessTypeStat>> GetBusinessTypeListAsync()
        {
            return _wkInstanceRepository.GetBusinessTypeListAsync();
        }
        public virtual async Task<List<ProcessTypeStat>> GetProcessTypeStatListAsync()
        {
            var result = await _wkInstanceRepository.GetProcessTypeStatListAsync();
            // 创建一个包含所有月份的列表
            var allMonths = Enumerable.Range(1, 12).Select(m => m.ToString().PadLeft(2, '0')).ToList();

            // 创建一个字典来存储每个ProcessType的全年数据
            var fullYearData = new Dictionary<string, List<ProcessTypeStat>>();

            foreach (var item in result)
            {
                if (!fullYearData.ContainsKey(item.PClassification))
                {
                    fullYearData[item.PClassification] = new List<ProcessTypeStat>();
                }
                fullYearData[item.PClassification].Add(item);
            }

            // 确保每个ProcessType都包含全年的每个月份
            var finalResult = new List<ProcessTypeStat>();
            foreach (var processType in fullYearData.Keys)
            {
                foreach (var month in allMonths)
                {
                    var existingStat = fullYearData[processType].FirstOrDefault(s => s.SClassification == month);
                    if (existingStat != null)
                    {
                        finalResult.Add(existingStat);
                    }
                    else
                    {
                        // 如果数据库中没有该月份的数据，则添加计数为0的条目
                        finalResult.Add(new ProcessTypeStat
                        {
                            PClassification = processType,
                            SClassification = month,
                            Count = 0
                        });
                    }
                }
            }
            return finalResult;
        }
        public virtual async Task AuditAsync(Guid wkInstanceId, Guid executionPointerId, string remark)
        {
            if (CurrentUser.Id.HasValue)
            {
                var entity = await _wkAuditor.GetAuditorAsync(executionPointerId, CurrentUser.Id.Value);
                if (entity != null)
                {
                    await entity.Audit(DateTime.Now, remark);
                    await _wkAuditor.UpdateAsync(entity);
                }
                else
                {
                    var auditorInstance = new WkAuditor(
                    wkInstanceId,
                    executionPointerId,
                    CurrentUser.UserName,
                    userId: CurrentUser.Id.Value,
                    status: EnumAuditStatus.Unapprove);
                    await _wkAuditor.InsertAsync(auditorInstance);
                }
            }
        }
    }
}