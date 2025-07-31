# 工作流模板版本控制数据库迁移指南

## 概述

本迁移将工作流模板从单一主键改为复合主键 `(Id, Version)`，以支持版本控制功能。

## 涉及的表

### 需要版本控制的表：
- `HXWKDEFINITIONS` - 工作流定义（主表）
- `HXDEFINITION_CANDIDATES` - 定义候选人
- `HXNODE_CANDIDATES` - 节点候选人  
- `HXWKNODES` - 工作流节点
- `HXWKINSTANCES` - 工作流实例
- `HX_NODES_APPLICATION_FORMS` - 节点表单关联

### 不需要版本控制的表：
- `HXWKEXECUTIONPOINTER` - 执行指针（运行时数据）
- `HXPOINTER_CANDIDATES` - 指针候选人（运行时数据，但保留与 WkExecutionPointer 的关系）

## 架构设计说明

### ExePointerCandidate 的设计
- 继承自 `CandidateWithoutVersion` 基类（不需要版本控制）
- 使用单一主键 `(NodeId, CandidateId)`
- 保留与 `WkExecutionPointer` 的关系，但不参与版本控制
- 这是运行时数据，不需要版本控制

### Candidate 基类层次结构
- `Candidate` - 需要版本控制的基类，包含 `Version` 字段
- `CandidateWithoutVersion` - 不需要版本控制的基类，不包含 `Version` 字段
- `ExePointerCandidate` 继承自 `CandidateWithoutVersion`

## 迁移执行

### 快速执行（推荐）
```sql
-- 执行完整的迁移脚本
\i 20250221050000_ExecuteVersionControlMigration.sql
```

### 分步执行（用于调试）
```sql
-- 步骤1: 数据修复
\i 20250221050000_FixDataBeforeMigration.sql

-- 步骤2: 添加版本字段
\i 20250221050000_AddVersionColumnsToRelatedTables.sql

-- 步骤3: 修改主键结构
\i 20250221050000_ModifyWkDefinitionToCompositeKey.sql

-- 步骤4: 管理外键约束
\i 20250221050000_ManageForeignKeysForVersionControl.sql
```

## 迁移脚本说明

### 20250221050000_ExecuteVersionControlMigration.sql
- **主执行脚本**：按正确顺序执行所有迁移步骤
- **推荐使用**：一键完成所有迁移操作

### 20250221050000_FixDataBeforeMigration.sql
- 修复现有数据中 `VERSION=0` 或 `NULL` 的记录
- 将所有相关表的 `VERSION` 设置为 1
- **注意**：不处理 `HXPOINTER_CANDIDATES`，因为 `ExePointerCandidate` 不需要版本控制

### 20250221050000_AddVersionColumnsToRelatedTables.sql
- 为需要版本控制的表添加 `VERSION` 字段
- 设置默认值为 1
- 更新现有数据
- **注意**：不添加 `HXPOINTER_CANDIDATES` 的 `VERSION` 字段

### 20250221050000_ModifyWkDefinitionToCompositeKey.sql
- 删除 `HXWKDEFINITIONS` 的单一主键
- 创建复合主键 `(ID, VERSION)`
- 添加约束和索引

### 20250221050000_ManageForeignKeysForVersionControl.sql
- 删除需要修改的外键约束
- 确保 `HXWKNODES` 有复合主键
- 重建复合外键约束
- **注意**：不处理 `HXPOINTER_CANDIDATES` 的外键约束

## 外键约束变更

### 新增的复合外键：
- `HXWKNODES.WKDIFINITIONID, VERSION` → `HXWKDEFINITIONS.ID, VERSION`
- `HXDEFINITION_CANDIDATES.NODEID, VERSION` → `HXWKDEFINITIONS.ID, VERSION`
- `HXNODE_CANDIDATES.NODEID, VERSION` → `HXWKNODES.ID, VERSION`
- `HX_NODES_APPLICATION_FORMS.NODE_ID, VERSION` → `HXWKNODES.ID, VERSION`
- `HXWKINSTANCES.WKDIFINITIONID, VERSION` → `HXWKDEFINITIONS.ID, VERSION`

### 保持单一外键：
- `HXPOINTER_CANDIDATES.NODEID` → `HXWKEXECUTIONPOINTER.ID`（不参与版本控制）

## 备份建议

执行迁移前请备份数据库：
```bash
pg_dump -h localhost -U username -d database_name > backup_before_version_control.sql
```

## 验证步骤

### 1. 检查主键结构
```sql
SELECT table_name, constraint_name, constraint_type 
FROM information_schema.table_constraints 
WHERE table_name IN ('HXWKDEFINITIONS', 'HXWKNODES') 
AND constraint_type = 'PRIMARY KEY';
```

### 2. 检查外键约束
```sql
SELECT tc.table_name, tc.constraint_name, kcu.column_name
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu 
ON tc.constraint_name = kcu.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
AND tc.table_name IN ('HXWKNODES', 'HXDEFINITION_CANDIDATES', 'HXNODE_CANDIDATES', 'HX_NODES_APPLICATION_FORMS', 'HXWKINSTANCES');
```

### 3. 检查数据完整性
```sql
-- 检查是否有 VERSION=0 或 NULL 的记录
SELECT 'HXWKDEFINITIONS' as table_name, COUNT(*) as invalid_count
FROM "HXWKDEFINITIONS" WHERE "VERSION" IS NULL OR "VERSION" <= 0
UNION ALL
SELECT 'HXDEFINITION_CANDIDATES', COUNT(*)
FROM "HXDEFINITION_CANDIDATES" WHERE "VERSION" IS NULL OR "VERSION" <= 0
UNION ALL
SELECT 'HXNODE_CANDIDATES', COUNT(*)
FROM "HXNODE_CANDIDATES" WHERE "VERSION" IS NULL OR "VERSION" <= 0
UNION ALL
SELECT 'HXWKNODES', COUNT(*)
FROM "HXWKNODES" WHERE "VERSION" IS NULL OR "VERSION" <= 0
UNION ALL
SELECT 'HXWKINSTANCES', COUNT(*)
FROM "HXWKINSTANCES" WHERE "VERSION" IS NULL OR "VERSION" <= 0
UNION ALL
SELECT 'HX_NODES_APPLICATION_FORMS', COUNT(*)
FROM "HX_NODES_APPLICATION_FORMS" WHERE "VERSION" IS NULL OR "VERSION" <= 0;
```

## 回滚方案

如果迁移失败，可以使用回滚脚本：
```sql
\i 20250221050000_RollbackWkDefinitionCompositeKey.sql
```

## 注意事项

1. **ExePointerCandidate 不需要版本控制**：作为运行时数据，不需要版本控制，但保留与 WkExecutionPointer 的关系
2. **WkExecutionPointer 保留 WkCandidates 关系**：虽然 ExePointerCandidate 不需要版本控制，但 WkExecutionPointer 仍然需要管理候选人关系
3. **简化外键管理**：只处理需要版本控制的外键，避免不必要的复杂性
4. **数据一致性**：确保所有相关表的 `VERSION` 字段都有正确的值
5. **测试环境**：建议先在测试环境执行迁移，验证无误后再在生产环境执行

## 故障排除

### 常见错误及解决方案：

1. **外键约束错误**
   - 确保先执行数据修复脚本
   - 检查 `VERSION` 字段是否存在且不为空

2. **主键冲突**
   - 确保 `HXWKDEFINITIONS` 表没有重复的 `(ID, VERSION)` 组合

3. **约束名称冲突**
   - 检查是否有同名的约束，如有则先删除

4. **数据不一致**
   - 使用验证脚本检查数据完整性
   - 手动修复异常数据 