using Hx.Workflow.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using WorkflowCore.Interface;

namespace Hx.Workflow.Application
{
    /// <summary>
    /// 工作流用户上下文服务，用于在 StepBody 中获取用户信息
    /// </summary>
    public class WorkflowUserContext(IWkInstanceRepository wkInstanceRepository) : ITransientDependency
    {
        private readonly IWkInstanceRepository _wkInstanceRepository = wkInstanceRepository;

        /// <summary>
        /// 从工作流数据中获取当前用户ID
        /// </summary>
        /// <param name="workflowData">工作流数据字典</param>
        /// <returns>用户ID，如果不存在则返回null</returns>
        public static Guid? GetCurrentUserIdFromWorkflowData(IDictionary<string, object>? workflowData)
        {
            if (workflowData == null)
                return null;

            // 优先从工作流数据中获取当前操作用户
            if (workflowData.TryGetValue("CurrentUserId", out var currentUserIdObj))
            {
                if (currentUserIdObj is Guid guid)
                    return guid;
                if (Guid.TryParse(currentUserIdObj?.ToString(), out var parsedGuid))
                    return parsedGuid;
            }

            // 如果没有当前用户，尝试获取启动用户
            if (workflowData.TryGetValue("StartUserId", out var startUserIdObj))
            {
                if (startUserIdObj is Guid guid)
                    return guid;
                if (Guid.TryParse(startUserIdObj?.ToString(), out var parsedGuid))
                    return parsedGuid;
            }

            return null;
        }

        /// <summary>
        /// 从工作流数据中获取当前用户名
        /// </summary>
        /// <param name="workflowData">工作流数据字典</param>
        /// <returns>用户名，如果不存在则返回null</returns>
        public static string? GetCurrentUserNameFromWorkflowData(IDictionary<string, object>? workflowData)
        {
            if (workflowData == null)
                return null;

            // 优先从工作流数据中获取当前操作用户名
            if (workflowData.TryGetValue("CurrentUserName", out var currentUserNameObj))
            {
                return currentUserNameObj?.ToString();
            }

            // 如果没有当前用户名，尝试获取启动用户名
            if (workflowData.TryGetValue("StartUserName", out var startUserNameObj))
            {
                return startUserNameObj?.ToString();
            }

            return null;
        }

        /// <summary>
        /// 从工作流执行上下文中获取当前用户ID
        /// </summary>
        /// <param name="context">工作流执行上下文</param>
        /// <returns>用户ID，如果不存在则返回null</returns>
        public static Guid? GetCurrentUserId(IStepExecutionContext context)
        {
            var workflowData = context.Workflow.Data as IDictionary<string, object>;
            return GetCurrentUserIdFromWorkflowData(workflowData);
        }

        /// <summary>
        /// 从工作流执行上下文中获取当前用户名
        /// </summary>
        /// <param name="context">工作流执行上下文</param>
        /// <returns>用户名，如果不存在则返回null</returns>
        public static string? GetCurrentUserName(IStepExecutionContext context)
        {
            var workflowData = context.Workflow.Data as IDictionary<string, object>;
            return GetCurrentUserNameFromWorkflowData(workflowData);
        }

        /// <summary>
        /// 从工作流实例中获取创建者ID（作为后备方案）
        /// </summary>
        /// <param name="workflowId">工作流ID</param>
        /// <returns>创建者ID，如果不存在则返回null</returns>
        public async Task<Guid?> GetCreatorIdFromInstanceAsync(string workflowId)
        {
            try
            {
                var instance = await _wkInstanceRepository.FindAsync(new Guid(workflowId));
                return instance?.CreatorId;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从执行指针中获取接收者ID（作为后备方案）
        /// </summary>
        /// <param name="workflowId">工作流ID</param>
        /// <param name="executionPointerId">执行指针ID</param>
        /// <returns>接收者ID，如果不存在则返回null</returns>
        public async Task<Guid?> GetRecipientIdFromExecutionPointerAsync(string workflowId, string executionPointerId)
        {
            try
            {
                var instance = await _wkInstanceRepository.FindAsync(new Guid(workflowId));
                if (instance == null)
                    return null;

                var executionPointer = instance.ExecutionPointers.FirstOrDefault(
                    ep => ep.Id.ToString() == executionPointerId);
                return executionPointer?.RecipientId;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 综合获取当前用户ID（优先从工作流数据，其次从实例和执行指针）
        /// </summary>
        /// <param name="context">工作流执行上下文</param>
        /// <returns>用户ID，如果不存在则返回null</returns>
        public async Task<Guid?> GetCurrentUserIdAsync(IStepExecutionContext context)
        {
            // 优先从工作流数据中获取
            var userId = GetCurrentUserId(context);
            if (userId.HasValue)
                return userId;

            // 其次从执行指针中获取接收者
            userId = await GetRecipientIdFromExecutionPointerAsync(
                context.Workflow.Id, 
                context.ExecutionPointer.Id);
            if (userId.HasValue)
                return userId;

            // 最后从实例中获取创建者
            userId = await GetCreatorIdFromInstanceAsync(context.Workflow.Id);
            return userId;
        }

        /// <summary>
        /// 验证是否能够获取到用户信息
        /// </summary>
        /// <param name="context">工作流执行上下文</param>
        /// <returns>如果能够获取到用户信息则返回true</returns>
        public async Task<bool> HasUserInfoAsync(IStepExecutionContext context)
        {
            var userId = await GetCurrentUserIdAsync(context);
            return userId.HasValue;
        }
    }
}
