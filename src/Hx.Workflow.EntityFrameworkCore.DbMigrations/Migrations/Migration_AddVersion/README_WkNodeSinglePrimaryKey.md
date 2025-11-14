# WkNode 主键修改说明

## 修改原因

`WkNode` 的 `Version` 字段实际上是 `WkDefinition` 的版本，用于外键关联到 `WkDefinition`。`WkNode` 不需要自己的版本控制，因为已经通过 `WkDefinition` 的版本进行了隔离。

因此，将 `WkNode` 的主键从复合主键 `(Id, Version)` 改为单一主键 `Id`，`Version` 字段保留但仅作为外键的一部分，不作为主键的一部分。

## 修改内容

### 1. 实体类修改

#### WkNode

- **主键**：从 `(Id, Version)` 改为 `Id`
- **Version 字段**：改名为 `WkDefinitionVersion`，保留但仅作为外键的一部分，不作为主键
- **GetKeys()**：返回 `[Id]` 而不是 `[Id, Version]`
- **构造函数**：移除了 `wkDefinitionVersion` 参数，因为版本信息应该从父实体 `WkDefinition` 获取
- **SetDefinition 方法**：新增方法，同时设置 `WkDefinitionId` 和 `WkDefinitionVersion`（必须同时设置，因为它们作为外键关联到 `WkDefinition`）

#### WkNodeCandidate

- **主键**：从 `(NodeId, CandidateId, Version)` 改为 `(NodeId, CandidateId)`
- **外键**：从 `(NodeId, Version)` 改为 `NodeId`
- **Version 字段**：已删除，不再需要（继承自 `CandidateBase`，不包含 Version 字段）

#### WkNodePara

- **主键**：保持为 `Id`（单一主键）
- **外键**：从 `(WkNodeId, Version)` 改为 `WkNodeId`
- **Version 字段**：已删除，不再需要

#### WkNode_ApplicationForms

- **主键**：从 `(NodeId, ApplicationId, Version)` 改为 `(NodeId, ApplicationId)`
- **外键**：从 `(NodeId, Version)` 改为 `NodeId`
- **Version 字段**：已删除，不再需要

### 2. EF Core 配置修改

#### WorkflowModelBuilderConfigrationExtensions.cs

**WkNode 配置**：

```csharp
// 修改前
t.HasKey(p => new { p.Id, p.Version }).HasName("PK_WKNODES");

// 修改后
t.HasKey(p => p.Id).HasName("PK_WKNODES");
```

**WkNode.WkCandidates 外键**：

```csharp
// 修改前
.HasForeignKey(d => new { d.NodeId, d.Version })

// 修改后
.HasForeignKey(d => d.NodeId)
```

**WkNode.OutcomeSteps 外键**：

```csharp
// 修改前
.HasForeignKey(d => new { d.WkNodeId, d.Version })

// 修改后
.HasForeignKey(d => d.WkNodeId)
```

**WkNodeCandidate 配置**：

```csharp
// 修改前
d.HasKey(d => new { d.NodeId, d.CandidateId, d.Version });

// 修改后
d.HasKey(d => new { d.NodeId, d.CandidateId });
```

**WkNode_ApplicationForms 配置**：

```csharp
// 修改前
d.HasKey(d => new { d.NodeId, d.ApplicationId, d.Version });
.HasForeignKey(d => new { d.NodeId, d.Version })

// 修改后
d.HasKey(d => new { d.NodeId, d.ApplicationId });
.HasForeignKey(d => d.NodeId)
```

### 3. 应用层修改

#### WkDefinition.AddWkNode 方法

- **修改**：自动设置节点的 `WkDefinitionId` 和 `WkDefinitionVersion`
- **实现**：调用 `node.SetDefinition(Id, Version)` 确保节点正确关联到父定义

```csharp
public async Task AddWkNode(WkNode node)
{
    // 自动设置节点的 WkDefinitionId 和 WkDefinitionVersion
    await node.SetDefinition(Id, Version);
    Nodes.Add(node);
}
```

#### WkNodeCreateDto

- **修改**：移除了 `WkDefinitionVersion` 字段
- **原因**：节点总是通过 `WkDefinition` 创建，版本信息应从父实体获取，无需在 DTO 中传递

#### WkNode 构造函数

- **修改前**：

```csharp
public WkNode(
    string name,
    string displayName,
    StepNodeType stepNodeType,
    int wkDefinitionVersion,  // 已移除
    int sortNumber,
    int? limitTime = null,
    Guid? id = null)
```

- **修改后**：

```csharp
public WkNode(
    string name,
    string displayName,
    StepNodeType stepNodeType,
    int sortNumber,
    int? limitTime = null,
    Guid? id = null)
```

#### SetDefinition 方法

- **新增**：合并了 `SetDefinitionId` 和 `SetWkDefinitionVersion` 两个方法
- **原因**：`WkDefinitionId` 和 `WkDefinitionVersion` 必须同时设置，因为它们作为外键关联到 `WkDefinition`

```csharp
/// <summary>
/// 设置节点的定义ID和版本（必须同时设置，因为它们作为外键关联到 WkDefinition）
/// </summary>
public Task SetDefinition(Guid definitionId, int version)
{
    WkDefinitionId = definitionId;
    WkDefinitionVersion = version;
    return Task.CompletedTask;
}
```

### 4. 数据库迁移脚本

**文件**：`20250222000000_ModifyWkNodeToSinglePrimaryKey.sql`

**主要操作**：

1. 删除 `WKNODES` 表的复合主键，创建单一主键
2. 删除 `NODE_CANDIDATES` 表的复合主键，创建新的复合主键（不包含 Version）
3. 更新 `NODE_CANDIDATES` 的外键约束（只关联 NodeId）
4. **删除 `NODE_CANDIDATES` 表的 `VERSION` 字段**（因为 `WkNodeCandidate` 不再需要 Version）
5. 更新 `WKNODEPARAS` 的外键约束（只关联 WkNodeId）
6. **删除 `WKNODEPARAS` 表的 `VERSION` 字段**（因为 `WkNodePara` 不再需要 Version）
7. 删除 `_NODES_APPLICATION_FORMS` 表的复合主键，创建新的复合主键（不包含 Version）
8. 更新 `_NODES_APPLICATION_FORMS` 的外键约束（只关联 NodeId）
9. **删除 `_NODES_APPLICATION_FORMS` 表的 `VERSION` 字段**（因为 `WkNode_ApplicationForms` 不再需要 Version）
10. 验证并修复数据完整性（确保 `WKNODES` 表的 `VERSION` 字段与 `WkDefinition` 的 `Version` 一致）

**注意**：

- `WKNODES` 表的 `VERSION` 字段保留（数据库列名保持为 `VERSION`，代码中已改名为 `WkDefinitionVersion`）
- `DEFINITION_CANDIDATES` 表的 `VERSION` 字段保留（作为主键的一部分）

## 影响范围

### 不受影响的操作

- 通过 `Id` 查询 `WkNode` 的操作（因为主键仍然是 `Id`）
- 通过 `WkDefinitionId` 和 `Version` 查询 `WkNode` 的操作（因为查询条件不变）
- 所有业务逻辑代码（因为不直接使用复合主键）

### 需要注意的操作

- 如果代码中直接使用 `WkNode` 的复合主键进行查询或更新，需要修改为只使用 `Id`
- 如果代码中直接使用 `WkNodeCandidate`、`WkNodePara`、`WkNode_ApplicationForms` 的复合主键，需要相应调整
- **创建节点时**：不再需要在构造函数中传入 `WkDefinitionVersion`，版本信息会通过 `AddWkNode` 方法自动设置
- **更新节点时**：`UpdateFrom` 方法不会更新 `WkDefinitionId` 和 `WkDefinitionVersion`，这些字段必须通过 `SetDefinition` 方法或 `AddWkNode` 方法设置
- **DTO 修改**：`WkNodeCreateDto` 中移除了 `WkDefinitionVersion` 字段，创建节点时无需传递版本信息

## 数据库迁移步骤

1. **备份数据库**

   ```sql
   pg_dump -U username -d database_name > backup.sql
   ```

2. **执行迁移脚本**

   ```sql
   \i 20250222000000_ModifyWkNodeToSinglePrimaryKey.sql
   ```

3. **验证迁移结果**
   - 检查主键约束是否正确创建
   - 检查外键约束是否正确创建
   - 检查数据完整性（Version 字段是否与 WkNode 的 Version 一致）

## 优势

1. **简化主键结构**：`WkNode` 使用单一主键，更符合常规设计
2. **明确版本控制范围**：版本控制只在 `WkDefinition` 层面，`WkNode` 不需要自己的版本控制
3. **提高查询性能**：单一主键的查询性能通常优于复合主键
4. **减少数据冗余**：`Version` 字段不再作为主键的一部分，减少索引大小

## 设计改进

### 1. 封装性更好

- 版本信息由父实体 `WkDefinition` 管理，节点无需关心版本号
- 节点创建时自动从父实体获取版本信息，避免手动传递错误

### 2. 代码更简洁

- 构造函数参数减少，调用更简单
- DTO 中移除了不必要的版本字段

### 3. 一致性更强

- 所有节点都通过 `AddWkNode` 方法添加，确保版本信息正确设置
- `SetDefinition` 方法强制同时设置 `WkDefinitionId` 和 `WkDefinitionVersion`，避免不一致

### 4. 维护性更好

- 版本逻辑集中在一处（`AddWkNode` 方法），便于维护
- 减少了代码重复和潜在的错误

## 注意事项

1. **WkDefinitionVersion 字段仍然保留**：虽然不作为主键，但仍然需要保留，用于外键关联到 `WkDefinition`（数据库列名保持为 `VERSION`，代码中已改名为 `WkDefinitionVersion`）
2. **Version 字段已删除**：`WkNodeCandidate`、`WkNodePara`、`WkNode_ApplicationForms` 的 `Version` 字段已从数据库和代码中删除
3. **数据完整性**：迁移脚本会自动验证并修复 Version 字段不匹配的情况
4. **向后兼容**：所有现有的查询和更新操作都应该继续正常工作
5. **节点创建**：创建节点时不再需要传递 `WkDefinitionVersion`，版本信息会通过 `AddWkNode` 方法自动设置
6. **节点更新**：`UpdateFrom` 方法不会更新 `WkDefinitionId` 和 `WkDefinitionVersion`，这些字段必须通过 `SetDefinition` 方法或 `AddWkNode` 方法设置
