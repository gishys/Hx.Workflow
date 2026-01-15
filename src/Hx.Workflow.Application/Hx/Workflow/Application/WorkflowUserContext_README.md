# WorkflowUserContext 使用说明

## 概述

`WorkflowUserContext` 是一个辅助服务，用于在 WorkflowCore 的 StepBody 中获取当前登录用户信息。由于 WorkflowCore 引擎在后台线程中执行 StepBody，无法直接通过 ABP 的 `ICurrentUser` 获取当前用户信息，因此需要通过工作流数据传递用户信息。

## 解决方案架构

### 1. 用户信息传递机制

工作流启动和活动执行时，当前用户信息会被自动添加到工作流数据中：

- **启动工作流时** (`WorkflowAppService.StartAsync`)：

  - `StartUserId`: 启动工作流的用户 ID
  - `StartUserName`: 启动工作流的用户名
  - `CurrentUserId`: 当前操作用户 ID（启动时与启动用户相同）
  - `CurrentUserName`: 当前操作用户名（启动时与启动用户相同）

- **启动活动时** (`WorkflowAppService.StartActivityAsync`)：
  - `CurrentUserId`: 当前操作用户 ID
  - `CurrentUserName`: 当前操作用户名

### 2. 用户信息获取优先级

`WorkflowUserContext` 按以下优先级获取用户信息：

1. **工作流数据中的 `CurrentUserId`**（最高优先级）
2. **工作流数据中的 `StartUserId`**（后备方案）
3. **执行指针的 `RecipientId`**（从数据库获取）
4. **工作流实例的 `CreatorId`**（从数据库获取，最低优先级）

## 使用方法

### 在 StepBody 中获取用户信息

```csharp
public class YourStepBody(
    WorkflowUserContext workflowUserContext,
    // ... 其他依赖
) : StepBodyAsync, ITransientDependency
{
    private readonly WorkflowUserContext _workflowUserContext = workflowUserContext;

    public async override Task<ExecutionResult> RunAsync(IStepExecutionContext context)
    {
        // 方法1：获取当前用户ID（推荐）
        var currentUserId = await _workflowUserContext.GetCurrentUserIdAsync(context);
        if (currentUserId.HasValue)
        {
            // 使用用户ID
            var userId = currentUserId.Value;
        }

        // 方法2：获取当前用户名
        var currentUserName = _workflowUserContext.GetCurrentUserName(context);
        if (!string.IsNullOrEmpty(currentUserName))
        {
            // 使用用户名
        }

        // 方法3：直接从工作流数据获取
        var workflowData = context.Workflow.Data as IDictionary<string, object>;
        var userId = _workflowUserContext.GetCurrentUserIdFromWorkflowData(workflowData);
        var userName = _workflowUserContext.GetCurrentUserNameFromWorkflowData(workflowData);

        // 方法4：检查是否有用户信息
        var hasUserInfo = await _workflowUserContext.HasUserInfoAsync(context);
        if (hasUserInfo)
        {
            // 执行需要用户信息的操作
        }

        // ... 其他逻辑
        return ExecutionResult.Next();
    }
}
```

### 在 Application Service 中启动工作流

用户信息会自动添加到工作流数据中，无需手动处理：

```csharp
public class YourAppService : ApplicationService
{
    public async Task<string> StartWorkflowAsync(StartWorkflowInput input)
    {
        // CurrentUser.Id 和 CurrentUser.UserName 会自动添加到 input.Inputs
        return await _workflowAppService.StartAsync(input);
    }
}
```

### 在 Application Service 中启动活动

用户信息会自动添加到活动数据中：

```csharp
public class YourAppService : ApplicationService
{
    public async Task StartActivityAsync(string actName, string workflowId, Dictionary<string, object>? data = null)
    {
        // CurrentUser.Id 和 CurrentUser.UserName 会自动添加到 data
        await _workflowAppService.StartActivityAsync(actName, workflowId, data);
    }
}
```

## API 参考

### WorkflowUserContext 方法

#### GetCurrentUserIdAsync(IStepExecutionContext context)

综合获取当前用户 ID，按优先级从多个来源获取。

**参数：**

- `context`: 工作流执行上下文

**返回：**

- `Task<Guid?>`: 用户 ID，如果不存在则返回 null

#### GetCurrentUserId(IStepExecutionContext context)

从工作流数据中获取当前用户 ID（同步方法）。

**参数：**

- `context`: 工作流执行上下文

**返回：**

- `Guid?`: 用户 ID，如果不存在则返回 null

#### GetCurrentUserName(IStepExecutionContext context)

从工作流数据中获取当前用户名（同步方法）。

**参数：**

- `context`: 工作流执行上下文

**返回：**

- `string?`: 用户名，如果不存在则返回 null

#### GetCurrentUserIdFromWorkflowData(IDictionary<string, object>? workflowData)

从工作流数据字典中获取当前用户 ID。

**参数：**

- `workflowData`: 工作流数据字典

**返回：**

- `Guid?`: 用户 ID，如果不存在则返回 null

#### GetCurrentUserNameFromWorkflowData(IDictionary<string, object>? workflowData)

从工作流数据字典中获取当前用户名。

**参数：**

- `workflowData`: 工作流数据字典

**返回：**

- `string?`: 用户名，如果不存在则返回 null

#### GetCreatorIdFromInstanceAsync(string workflowId)

从工作流实例中获取创建者 ID（后备方案）。

**参数：**

- `workflowId`: 工作流 ID

**返回：**

- `Task<Guid?>`: 创建者 ID，如果不存在则返回 null

#### GetRecipientIdFromExecutionPointerAsync(string workflowId, string executionPointerId)

从执行指针中获取接收者 ID（后备方案）。

**参数：**

- `workflowId`: 工作流 ID
- `executionPointerId`: 执行指针 ID

**返回：**

- `Task<Guid?>`: 接收者 ID，如果不存在则返回 null

#### HasUserInfoAsync(IStepExecutionContext context)

验证是否能够获取到用户信息。

**参数：**

- `context`: 工作流执行上下文

**返回：**

- `Task<bool>`: 如果能够获取到用户信息则返回 true

## 最佳实践

1. **优先使用异步方法**：`GetCurrentUserIdAsync` 提供了最完整的用户信息获取逻辑，包括后备方案。

2. **验证用户信息存在**：在使用用户信息之前，先检查是否为 null：

   ```csharp
   var userId = await _workflowUserContext.GetCurrentUserIdAsync(context);
   if (!userId.HasValue)
   {
       throw new UserFriendlyException("无法获取当前用户信息");
   }
   ```

3. **处理用户信息缺失的情况**：根据业务需求，决定是抛出异常还是使用默认值。

4. **日志记录**：在关键操作中记录用户信息，便于审计和调试：
   ```csharp
   var userId = await _workflowUserContext.GetCurrentUserIdAsync(context);
   Logger.LogInformation("用户 {UserId} 执行了操作", userId);
   ```

## 注意事项

1. **工作流数据键名**：系统使用以下键名存储用户信息：

   - `StartUserId`: 启动用户 ID
   - `StartUserName`: 启动用户名
   - `CurrentUserId`: 当前操作用户 ID
   - `CurrentUserName`: 当前操作用户名

2. **性能考虑**：`GetCurrentUserIdAsync` 方法可能会查询数据库（作为后备方案），如果性能敏感，可以考虑直接使用 `GetCurrentUserId` 方法。

3. **多租户支持**：该服务支持多租户场景，会自动处理租户隔离。

4. **向后兼容**：该方案不会破坏现有代码，如果工作流数据中没有用户信息，会尝试从数据库获取。

## 示例：在 GeneralAuditingStepBody 中的使用

```csharp
// 在 RunAsync 方法开始时获取用户信息
var currentUserId = await _workflowUserContext.GetCurrentUserIdAsync(context);
var currentUserName = _workflowUserContext.GetCurrentUserName(context);

// 在审核时使用用户信息
if (currentUserId.HasValue)
{
    Logger.LogInformation("用户 {UserId} ({UserName}) 执行了审核操作",
        currentUserId.Value, currentUserName);
}
```
