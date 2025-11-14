# EF Core 外键删除行为优化分析报告

## 📋 概述

本报告详细分析了 EF Core 配置中所有实体关系的删除行为优化，确保数据库操作的业务逻辑正确性和数据完整性。

## 🔍 修改详情

### 1. WkInstance -> WkDefinition

**位置**: `WorkflowModelBuilderConfigrationExtensions.cs` 第 478-484 行

**修改前**:

```csharp
t.HasOne(d => d.WkDefinition)
    .WithMany()
    .HasForeignKey(d => new { d.WkDifinitionId, d.Version })
    .HasConstraintName("FK_WKINSTANCES_WKDEFINITION_COMPOSITE");
```

**修改后**:

```csharp
// WkInstance 关联到特定版本的 WkDefinition
// 使用 Restrict 防止删除正在被实例使用的定义版本，保证数据完整性
t.HasOne(d => d.WkDefinition)
    .WithMany()
    .HasForeignKey(d => new { d.WkDifinitionId, d.Version })
    .HasConstraintName("FK_WKINSTANCES_WKDEFINITION_COMPOSITE")
    .OnDelete(DeleteBehavior.Restrict);
```

**业务原因**:

- 工作流实例必须关联到特定版本的定义
- 如果允许删除正在被实例使用的定义版本，会导致实例无法继续执行
- 应用层已实现检查逻辑（`CheckRunningInstancesAsync`），数据库层也应提供保护

**影响范围**:

- 删除 `WkDefinition` 时，如果有 `WkInstance` 引用该版本，数据库会阻止删除
- 需要先删除或完成所有相关实例，才能删除定义版本

---

### 2. WkAuditor -> WkInstance

**位置**: `WorkflowModelBuilderConfigrationExtensions.cs` 第 133-138 行

**修改前**:

```csharp
d.HasOne(d => d.Workflow)
    .WithMany(d => d.WkAuditors)
    .HasForeignKey(d => d.WorkflowId)
    .HasConstraintName("Pk_WkAuditor_WkInstance");
```

**修改后**:

```csharp
// WkAuditor 属于 WkInstance，删除实例时应该删除审核记录
d.HasOne(d => d.Workflow)
    .WithMany(d => d.WkAuditors)
    .HasForeignKey(d => d.WorkflowId)
    .HasConstraintName("Pk_WkAuditor_WkInstance")
    .OnDelete(DeleteBehavior.Cascade);
```

**业务原因**:

- `WkAuditor` 是工作流实例的审核记录，属于运行时数据
- 删除实例时，相关的审核记录应该一并删除，避免产生孤立数据

**影响范围**:

- 删除 `WkInstance` 时，会自动删除所有相关的 `WkAuditor` 记录

---

### 3. WkAuditor -> WkExecutionPointer

**位置**: `WorkflowModelBuilderConfigrationExtensions.cs` 第 140-145 行

**修改前**:

```csharp
d.HasOne(d => d.ExecutionPointer)
    .WithMany()
    .HasForeignKey(d => d.ExecutionPointerId)
    .HasConstraintName("Pk_WkAuditor_ExecPointer");
```

**修改后**:

```csharp
// WkAuditor 属于 WkExecutionPointer，删除执行指针时应该删除审核记录
d.HasOne(d => d.ExecutionPointer)
    .WithMany()
    .HasForeignKey(d => d.ExecutionPointerId)
    .HasConstraintName("Pk_WkAuditor_ExecPointer")
    .OnDelete(DeleteBehavior.Cascade);
```

**业务原因**:

- `WkAuditor` 关联到特定的执行指针
- 删除执行指针时，相关的审核记录应该一并删除

**影响范围**:

- 删除 `WkExecutionPointer` 时，会自动删除所有相关的 `WkAuditor` 记录

---

### 4. WkExecutionError -> WkInstance & WkExecutionPointer

**位置**: `WorkflowModelBuilderConfigrationExtensions.cs` 第 360-383 行

**修改前**:

```csharp
builder.Entity<WkExecutionError>(t =>
{
    // ... 属性配置 ...
    // 缺少外键关系配置
});
```

**修改后**:

```csharp
builder.Entity<WkExecutionError>(t =>
{
    // ... 属性配置 ...

    // WkExecutionError 属于 WkInstance，删除实例时应该删除错误记录
    t.HasOne<WkInstance>()
        .WithMany()
        .HasForeignKey(d => d.WkInstanceId)
        .HasConstraintName("FK_WKEXECUTIONERRORS_WKINSTANCE")
        .OnDelete(DeleteBehavior.Cascade);

    // WkExecutionError 属于 WkExecutionPointer，删除执行指针时应该删除错误记录
    t.HasOne<WkExecutionPointer>()
        .WithMany()
        .HasForeignKey(d => d.WkExecutionPointerId)
        .HasConstraintName("FK_WKEXECUTIONERRORS_EXECUTIONPOINTER")
        .OnDelete(DeleteBehavior.Cascade);
});
```

**业务原因**:

- `WkExecutionError` 是执行过程中的错误记录，属于运行时数据
- 删除实例或执行指针时，相关的错误记录应该一并删除
- 之前缺少外键关系配置，可能导致数据不一致

**影响范围**:

- 删除 `WkInstance` 时，会自动删除所有相关的 `WkExecutionError` 记录
- 删除 `WkExecutionPointer` 时，会自动删除所有相关的 `WkExecutionError` 记录
- 新增了两个外键约束，保证数据完整性

---

### 5. WkSubscription -> WkExecutionPointer

**位置**: `WorkflowModelBuilderConfigrationExtensions.cs` 第 435-440 行

**修改前**:

```csharp
t.HasMany(d => d.WkSubscriptions)
    .WithOne()
    .HasForeignKey(d => d.ExecutionPointerId)
    .HasConstraintName("FK_EXTENSIONATTRIBUTES_POINTERS");
```

**修改后**:

```csharp
// WkSubscription 属于 WkExecutionPointer，删除执行指针时应该删除订阅
t.HasMany(d => d.WkSubscriptions)
    .WithOne()
    .HasForeignKey(d => d.ExecutionPointerId)
    .HasConstraintName("FK_EXTENSIONATTRIBUTES_POINTERS")
    .OnDelete(DeleteBehavior.Cascade);
```

**业务原因**:

- `WkSubscription` 是执行指针的事件订阅，属于运行时数据
- 删除执行指针时，相关的订阅应该一并删除

**影响范围**:

- 删除 `WkExecutionPointer` 时，会自动删除所有相关的 `WkSubscription` 记录

---

### 6. WkNode_ApplicationForms -> ApplicationForm

**位置**: `WorkflowModelBuilderConfigrationExtensions.cs` 第 212-219 行

**修改前**:

```csharp
d.HasOne(d => d.ApplicationForm).WithMany().HasForeignKey(d => d.ApplicationId).HasConstraintName("APLLICATION_FKEY");
```

**修改后**:

```csharp
// ApplicationForm 是可重用的组件，多个节点可以共享同一个表单
// 使用 Restrict 防止删除正在被使用的表单，避免级联删除导致数据丢失
d.HasOne(d => d.ApplicationForm).WithMany().HasForeignKey(d => d.ApplicationId).HasConstraintName("APLLICATION_FKEY")
    .OnDelete(DeleteBehavior.Restrict);
```

**业务原因**:

- `ApplicationForm` 是可重用的组件，多个节点可以共享同一个表单
- 删除表单时，如果有节点正在使用该表单，应该阻止删除
- 避免级联删除导致节点数据丢失

**影响范围**:

- 删除 `ApplicationForm` 时，如果有 `WkNode_ApplicationForms` 引用该表单，数据库会阻止删除
- 需要先解除所有节点的表单关联，才能删除表单

---

### 7. WkDefinitionGroup -> WkDefinition

**位置**: `WorkflowModelBuilderConfigrationExtensions.cs` 第 36-43 行

**修改前**:

```csharp
t.HasMany(t => t.Items)
    .WithOne()
    .HasForeignKey(d => d.GroupId)
    .HasConstraintName("QI_GROUPS_WKDEFINITION_ID")
    .OnDelete(DeleteBehavior.Cascade);
```

**修改后**:

```csharp
// WkDefinition 属于 WkDefinitionGroup，但删除组时不应该删除定义
// 使用 Restrict 防止删除组时删除定义，或者使用 SetNull 将定义的 GroupId 设置为 null
// 由于 GroupId 是可空的，使用 SetNull 更合理
t.HasMany(t => t.Items)
    .WithOne()
    .HasForeignKey(d => d.GroupId)
    .HasConstraintName("QI_GROUPS_WKDEFINITION_ID")
    .OnDelete(DeleteBehavior.SetNull);
```

**业务原因**:

- `WkDefinition` 是核心业务数据，不应该因为删除组而被删除
- `GroupId` 是可空字段，删除组时可以将定义的 `GroupId` 设置为 `null`
- 这样既保留了定义数据，又解除了分组关系

**影响范围**:

- 删除 `WkDefinitionGroup` 时，不会删除 `WkDefinition`，而是将 `GroupId` 设置为 `null`
- 定义数据得以保留，但不再属于任何组

---

### 8. ApplicationFormGroup -> ApplicationForm

**位置**: `WorkflowModelBuilderConfigrationExtensions.cs` 第 260-267 行

**修改前**:

```csharp
t.HasMany(t => t.Items)
    .WithOne()
    .HasForeignKey(d => d.GroupId)
    .HasConstraintName("AF_GROUPS_APPLICATIONFORM_ID")
    .OnDelete(DeleteBehavior.Cascade);
```

**修改后**:

```csharp
// ApplicationForm 属于 ApplicationFormGroup，但删除组时不应该删除表单
// 使用 Restrict 防止删除组时删除表单，或者使用 SetNull 将表单的 GroupId 设置为 null
// 由于 GroupId 是可空的，使用 SetNull 更合理
t.HasMany(t => t.Items)
    .WithOne()
    .HasForeignKey(d => d.GroupId)
    .HasConstraintName("AF_GROUPS_APPLICATIONFORM_ID")
    .OnDelete(DeleteBehavior.SetNull);
```

**业务原因**:

- `ApplicationForm` 是核心业务数据，不应该因为删除组而被删除
- `GroupId` 是可空字段，删除组时可以将表单的 `GroupId` 设置为 `null`
- 这样既保留了表单数据，又解除了分组关系

**影响范围**:

- 删除 `ApplicationFormGroup` 时，不会删除 `ApplicationForm`，而是将 `GroupId` 设置为 `null`
- 表单数据得以保留，但不再属于任何组

---

### 9. WkNode -> WkStepBody

**位置**: `WorkflowModelBuilderConfigrationExtensions.cs` 第 166-173 行

**修改前**:

```csharp
t.HasOne(d => d.StepBody).WithMany()
    .HasForeignKey(d => d.WkStepBodyId)
    .HasConstraintName("Pk_WkNode_WkStepBody")
    .OnDelete(DeleteBehavior.Restrict);
```

**修改后**:

```csharp
// WkStepBody 是一个可重用的组件，多个 WkNode 可以共享同一个 WkStepBody
// 使用 Restrict 防止删除正在被使用的 WkStepBody，避免级联删除导致数据丢失
// 配置为可选关系（IsRequired(false)），确保即使 WkStepBody 不存在，WkNode 仍然会被加载
t.HasOne(d => d.StepBody).WithMany()
    .HasForeignKey(d => d.WkStepBodyId)
    .HasConstraintName("Pk_WkNode_WkStepBody")
    .IsRequired(false)  // 明确标记为可选关系，允许 StepBody 为 null
    .OnDelete(DeleteBehavior.Restrict);
```

**业务原因**:

- `WkStepBody` 是可重用的组件，多个节点可以共享
- 明确标记为可选关系，即使 `WkStepBody` 不存在，`WkNode` 仍然会被加载
- 保证数据的完整性，避免因关联实体缺失而导致节点被过滤掉

**影响范围**:

- 删除 `WkStepBody` 时，如果有 `WkNode` 引用该步骤体，数据库会阻止删除
- 查询 `WkNode` 时，即使 `WkStepBody` 不存在，节点仍然会被加载（`StepBody` 为 `null`）

---

## 📊 删除行为策略总结

| 关系                                       | 删除行为 | 策略类型         | 业务原因               |
| ------------------------------------------ | -------- | ---------------- | ---------------------- |
| WkInstance -> WkDefinition                 | Restrict | 保护核心数据     | 防止删除有实例的定义   |
| WkAuditor -> WkInstance                    | Cascade  | 清理运行时数据   | 审核记录属于实例       |
| WkAuditor -> WkExecutionPointer            | Cascade  | 清理运行时数据   | 审核记录属于执行指针   |
| WkExecutionError -> WkInstance             | Cascade  | 清理运行时数据   | 错误记录属于实例       |
| WkExecutionError -> WkExecutionPointer     | Cascade  | 清理运行时数据   | 错误记录属于执行指针   |
| WkSubscription -> WkExecutionPointer       | Cascade  | 清理运行时数据   | 订阅属于执行指针       |
| WkNode -> WkStepBody                       | Restrict | 保护可重用组件   | StepBody 可重用        |
| WkNode_ApplicationForms -> ApplicationForm | Restrict | 保护可重用组件   | ApplicationForm 可重用 |
| WkDefinitionGroup -> WkDefinition          | SetNull  | 保留数据解除关系 | 删除组时保留定义       |
| ApplicationFormGroup -> ApplicationForm    | SetNull  | 保留数据解除关系 | 删除组时保留表单       |

## 🎯 优化原则

### 1. 数据完整性保护

- **Restrict**: 用于保护核心业务数据和可重用组件
  - 防止删除正在被使用的实体
  - 保证数据完整性

### 2. 运行时数据清理

- **Cascade**: 用于清理运行时产生的数据
  - 删除父实体时自动清理子实体
  - 避免产生孤立数据

### 3. 分组关系处理

- **SetNull**: 用于处理分组关系
  - 删除组时保留实体数据
  - 将 `GroupId` 设置为 `null`，解除分组关系

### 4. 可选关系配置

- **IsRequired(false)**: 明确标记可选关系
  - 确保查询时不会过滤掉数据
  - 允许导航属性为 `null`

## ⚠️ 注意事项

1. **数据库迁移**: 需要执行迁移脚本更新外键约束
2. **应用层检查**: 数据库层的 `Restrict` 与应用层的检查逻辑（如 `CheckRunningInstancesAsync`）配合使用
3. **数据一致性**: `SetNull` 行为要求外键字段必须可空
4. **性能影响**: 级联删除可能影响性能，但保证了数据一致性

## 📝 迁移脚本

详见: `20250223000000_OptimizeForeignKeyDeleteBehaviors.sql`

## ✅ 验证清单

- [x] 所有外键关系都配置了删除行为
- [x] 核心业务数据使用 `Restrict` 保护
- [x] 运行时数据使用 `Cascade` 清理
- [x] 分组关系使用 `SetNull` 处理
- [x] 可重用组件使用 `Restrict` 保护
- [x] 可选关系明确标记 `IsRequired(false)`
- [x] 代码编译通过
- [x] 迁移脚本已生成

## 📅 修改日期

2025-02-23
