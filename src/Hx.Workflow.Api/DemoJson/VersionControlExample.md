# 工作流模板版本控制使用示例

## 版本控制原理

在当前的实现中，同一个模板的所有版本共享相同的ID，通过版本号来区分不同的版本。使用 ABP vNext 的复合主键 `WkDefinitionKey(Id, Version)` 来确保每个版本都是唯一的记录。

### 数据结构
```json
{
  "id": {
    "definitionId": "550e8400-e29b-41d4-a716-446655440000",  // 所有版本共享相同的ID
    "version": 1                                               // 版本号区分不同版本
  },
  "title": "请假申请流程",
  "description": "员工请假申请流程",
  "businessType": "LeaveApplication",
  "processType": "Approval",
  "isEnabled": true
}
```

## 数据库结构

### ABP vNext 复合主键设计
```sql
-- WkDefinition 表结构
CREATE TABLE WKDEFINITIONS (
    ID UNIQUEIDENTIFIER NOT NULL,           -- 模板ID（所有版本共享）
    VERSION INT NOT NULL,                   -- 版本号
    TITLE NVARCHAR(255) NOT NULL,          -- 标题
    DESCRIPTION NVARCHAR(500),              -- 描述
    BUSINESSTYPE NVARCHAR(255) NOT NULL,    -- 业务类型
    PROCESSTYPE NVARCHAR(255) NOT NULL,     -- 流程类型
    ISENABLED BIT NOT NULL,                 -- 是否启用
    CREATIONTIME DATETIME2 NOT NULL,        -- 创建时间
    CREATORID UNIQUEIDENTIFIER,             -- 创建人ID
    -- 其他字段...
    PRIMARY KEY (ID, VERSION)               -- 复合主键
);

-- 索引设计
CREATE UNIQUE INDEX IX_WKDEFINITIONS_ID_VERSION ON WKDEFINITIONS (ID, VERSION);
CREATE INDEX IX_WKDEFINITIONS_ID ON WKDEFINITIONS (ID);

-- 检查约束
ALTER TABLE WKDEFINITIONS ADD CONSTRAINT CK_WKDEFINITIONS_VERSION_POSITIVE CHECK (VERSION > 0);
```

### 版本识别机制
- **复合主键**：`WkDefinitionKey(DefinitionId, Version)` 确保每个版本都是唯一的数据库记录
- **共享ID**：所有版本共享相同的DefinitionId，便于识别同一个模板的不同版本
- **版本号**：通过版本号区分不同的版本
- **ABP vNext 标准**：使用 `AggregateRoot<TKey>` 的标准做法

## API 使用示例

### 1. 创建初始版本
```http
POST /hxworkflow/hxdefinition
Content-Type: application/json

{
  "title": "请假申请流程",
  "description": "员工请假申请流程",
  "businessType": "LeaveApplication",
  "processType": "Approval",
  "version": 1,
  "isEnabled": true
}
```

### 2. 更新模板（创建新版本）
```http
PUT /hxworkflow/hxdefinition
Content-Type: application/json

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "title": "请假申请流程（优化版）",
  "description": "员工请假申请流程 - 优化版本",
  "businessType": "LeaveApplication",
  "processType": "Approval",
  "version": 0,  // 0表示自动递增版本号
  "isEnabled": true
}
```

### 3. 获取所有版本
```http
GET /hxworkflow/hxdefinition/versions?id=550e8400-e29b-41d4-a716-446655440000
```

### 4. 获取特定版本
```http
GET /hxworkflow/hxdefinition/details?id=550e8400-e29b-41d4-a716-446655440000&version=1
```

### 5. 比较版本差异
```http
GET /hxworkflow/hxdefinition/compare?id=550e8400-e29b-41d4-a716-446655440000&version1=1&version2=2
```

### 6. 回滚到指定版本
```http
POST /hxworkflow/hxdefinition/rollback?id=550e8400-e29b-41d4-a716-446655440000&targetVersion=1
```

### 7. 获取版本历史
```http
GET /hxworkflow/hxdefinition/history?id=550e8400-e29b-41d4-a716-446655440000
```

### 8. 删除指定版本
```http
DELETE /hxworkflow/hxdefinition/version?id=550e8400-e29b-41d4-a716-446655440000&version=1
```

### 9. 删除所有版本
```http
DELETE /hxworkflow/hxdefinition/all-versions?id=550e8400-e29b-41d4-a716-446655440000
```

### 10. 删除旧版本（保留最新5个版本）
```http
DELETE /hxworkflow/hxdefinition/old-versions?id=550e8400-e29b-41d4-a716-446655440000&keepCount=5
```

### 11. 删除最新版本
```http
DELETE /hxworkflow/hxdefinition?id=550e8400-e29b-41d4-a716-446655440000
```

### 12. 更新节点（创建新版本）
```http
PUT /hxworkflow/hxdefinition/nodes
Content-Type: application/json

{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "nodes": [
    {
      "name": "审批节点",
      "displayName": "部门审批",
      "stepNodeType": 1,
      "version": 1,
      "wkStepBodyId": "step-body-id",
      "limitTime": 120,
      "nextNodes": [
        {
          "nextNodeName": "结束节点",
          "nodeType": 1,
          "rules": []
        }
      ],
      "wkCandidates": [
        {
          "candidateId": "user-id",
          "userName": "张三",
          "displayUserName": "张三",
          "executorType": 1,
          "defaultSelection": true
        }
      ]
    }
  ],
  "recreateNodes": false,
  "extraProperties": {}
}
```

### 13. 查询实例（支持版本控制）
```http
GET /hxworkflow/hxworkflow/workflow/mywkinstances/version?definitionIds=550e8400-e29b-41d4-a716-446655440000&definitionVersions=1,2&status=1&skipCount=0&maxResultCount=20
```

### 14. 获取指定模板版本的实例
```http
GET /hxworkflow/hxworkflow/workflow/instances/by-version?definitionId=550e8400-e29b-41d4-a716-446655440000&version=1
```

### 15. 获取运行中的实例（按模板版本）
```http
GET /hxworkflow/hxworkflow/workflow/instances/running-by-version?definitionId=550e8400-e29b-41d4-a716-446655440000&version=1
```

## 实例管理版本控制

### 1. 实例与模板版本关联
- **版本信息存储**：实例中存储了模板ID和版本号
- **版本查询支持**：支持按模板版本查询实例
- **版本统计支持**：支持按版本统计实例数量

### 2. 实例查询功能
- **按版本查询**：支持查询指定模板版本的实例
- **运行实例检查**：支持检查指定版本的运行实例
- **版本过滤**：支持在查询时过滤特定版本

### 3. 实例管理特性
- **版本兼容性**：确保实例与模板版本的兼容性
- **版本迁移**：支持实例在不同版本间的迁移
- **版本回滚保护**：防止版本回滚影响正在运行的实例

## 版本控制特性

### 1. 版本管理
- **自动版本递增**：每次更新自动创建新版本
- **版本历史保留**：保留所有历史版本
- **版本回滚**：支持回滚到任意历史版本
- **版本比较**：支持比较不同版本的差异

### 2. 数据安全
- **实例检查**：更新前检查是否有正在运行的实例
- **数据完整性**：确保数据结构的完整性
- **并发控制**：防止并发更新导致的数据冲突

### 3. 更新策略
- **创建新版本**：更新时创建新版本而不是修改原版本
- **节点克隆**：完整克隆节点及其所有相关属性
- **候选人复制**：复制所有候选人信息
- **参数继承**：继承原版本的所有参数和配置

### 4. 节点更新特性
- **完整节点克隆**：包括候选人、参数、材料、表单等
- **关系保持**：保持节点间的连接关系
- **验证机制**：验证节点限制时间和名称唯一性
- **扩展属性**：支持自定义扩展属性

### 1. 版本识别
- 所有版本共享相同的DefinitionId
- 通过版本号区分不同版本
- 支持自动版本号递增
- ABP vNext 复合主键确保数据库记录唯一性

### 2. 实例兼容性
- 更新前检查是否有正在运行的实例
- 防止影响正在执行的流程

### 3. 版本管理
- 支持版本历史查看
- 支持版本差异比较
- 支持版本回滚

### 4. 数据完整性
- 新版本继承原版本的所有节点和候选人
- 保持数据结构的完整性
- 复合主键确保数据一致性

### 5. 版本删除管理
- **删除最新版本**：删除模板的最新版本
- **删除指定版本**：删除指定的版本号
- **删除所有版本**：删除模板的所有版本
- **删除旧版本**：保留指定数量的最新版本，删除其他旧版本
- **实例检查**：删除前检查是否有正在运行的实例，确保数据安全

## 实现细节

### 1. ABP vNext 实体设计
```csharp
public class WkDefinition : FullAuditedAggregateRoot<WkDefinitionKey>, IMultiTenant
{
    public int Version => Id.Version;  // 从复合主键获取版本号
    public Guid DefinitionId => Id.DefinitionId;  // 从复合主键获取定义ID
    // ... 其他属性
}

public class WkDefinitionKey : IEquatable<WkDefinitionKey>
{
    public Guid DefinitionId { get; }
    public int Version { get; }
    // ... 实现
}
```

### 2. EF Core 配置
```csharp
builder.Entity<WkDefinition>(t =>
{
    // 使用复合主键支持版本控制
    t.HasKey(p => new { p.Id.DefinitionId, p.Id.Version }).HasName("PK_WKDEFINITION");
    t.Property(p => p.Id.DefinitionId).HasColumnName("ID").HasComment("主键");
    t.Property(p => p.Id.Version).HasColumnName("VERSION").HasComment("版本号");
    // ... 其他配置
});
```

### 3. 仓储接口
```csharp
public interface IWkDefinitionRespository : IBasicRepository<WkDefinition, WkDefinitionKey>
{
    // 检查版本是否存在
    Task<bool> ExistsAsync(Guid id, int version, CancellationToken cancellationToken = default);
    
    // 获取最大版本号
    Task<int> GetMaxVersionAsync(Guid definitionId, CancellationToken cancellationToken = default);
    
    // 获取所有版本
    Task<List<WkDefinition>> GetAllVersionsAsync(Guid definitionId, CancellationToken cancellationToken = default);
}
```

### 4. 应用服务使用
```csharp
// 获取最新版本
var latestVersion = await _definitionRespository.FindAsync(definitionId);

// 获取特定版本
var specificVersion = await _definitionRespository.FindAsync(new WkDefinitionKey(definitionId, version));

// 检查版本是否存在
var exists = await _definitionRespository.ExistsAsync(definitionId, version);

// 删除操作
await _appService.DeleteAsync(definitionId);                    // 删除最新版本
await _appService.DeleteVersionAsync(definitionId, version);    // 删除指定版本
await _appService.DeleteAllVersionsAsync(definitionId);         // 删除所有版本
await _appService.DeleteOldVersionsAsync(definitionId, 5);     // 删除旧版本，保留5个最新版本
```

## ABP vNext 特性

### 1. 标准聚合根
- 继承自 `FullAuditedAggregateRoot<WkDefinitionKey>`
- 支持审计功能（创建时间、修改时间等）
- 支持软删除

### 2. 复合主键支持
- 使用 `WkDefinitionKey` 作为复合主键
- 符合 ABP vNext 的设计模式
- 支持 EF Core 的复合主键映射

### 3. 仓储模式
- 使用 `IBasicRepository<WkDefinition, WkDefinitionKey>`
- 支持标准的 CRUD 操作
- 支持自定义查询方法

### 4. 依赖注入
- 自动注册到 DI 容器
- 支持生命周期管理
- 支持事务处理

## 注意事项

1. **版本号管理**：建议使用自动递增的版本号，避免手动指定版本号可能导致的冲突
2. **实例检查**：在更新模板前，系统会自动检查是否有正在运行的实例
3. **数据迁移**：如果需要修改现有数据，建议创建新版本而不是直接修改
4. **性能考虑**：版本历史会随着时间增长，建议定期清理过旧的版本
5. **复合主键**：确保每个版本都是唯一的数据库记录，避免主键冲突
6. **索引优化**：通过索引提高按ID和版本号查询的性能
7. **ABP vNext 标准**：遵循 ABP vNext 的设计模式和最佳实践
8. **检查约束**：数据库层面确保版本号大于0 