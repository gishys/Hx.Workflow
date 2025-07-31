# 工作流模板版本控制迁移脚本

## 概述

本文件夹包含将工作流模板从单一主键改为复合主键 `(Id, Version)` 的所有迁移脚本，以支持版本控制功能。

## 迁移脚本列表

### 核心迁移脚本

1. **`20250221050000_ExecuteVersionControlMigration.sql`** - 主执行脚本（推荐使用）
   - 按正确顺序执行所有迁移步骤
   - 一键完成所有迁移操作

2. **`20250221050000_FixDataBeforeMigration.sql`** - 数据修复脚本
   - 修复现有数据中 `VERSION=0` 或 `NULL` 的记录
   - 将所有相关表的 `VERSION` 设置为 1

3. **`20250221050000_AddVersionColumnsToRelatedTables.sql`** - 添加版本字段脚本
   - 为需要版本控制的表添加 `VERSION` 字段
   - 设置默认值为 1
   - 更新现有数据

4. **`20250221050000_ModifyWkDefinitionToCompositeKey.sql`** - 修改主键结构脚本
   - 删除 `HXWKDEFINITIONS` 的单一主键
   - 创建复合主键 `(ID, VERSION)`
   - 添加约束和索引

5. **`20250221050000_ManageForeignKeysForVersionControl.sql`** - 外键约束管理脚本
   - 删除需要修改的外键约束
   - 确保 `HXWKNODES` 有复合主键
   - 重建复合外键约束

### 检查和分析脚本

6. **`20250221050000_CheckForeignKeyDependencies.sql`** - 外键依赖检查脚本
   - 检查 `HXWKDEFINITIONS` 表主键约束的依赖关系
   - 在删除主键约束前了解影响范围

7. **`20250221050000_DetailedForeignKeyAnalysis.sql`** - 详细外键分析脚本
   - 全面分析数据库中的外键依赖关系
   - 提供详细的迁移建议

### 回滚脚本

8. **`20250221050000_RollbackWkDefinitionCompositeKey.sql`** - 回滚脚本
   - 如果迁移失败，可以使用此脚本回滚
   - 删除所有新创建的约束和索引
   - 恢复原始的主键结构

### 文档

9. **`README_CompositeKeyMigration.md`** - 详细迁移指南
   - 包含完整的迁移说明和故障排除

## 执行方式

### 快速执行（推荐）
```sql
-- 在 PostgreSQL 中执行
\i Migration_AddVersion/20250221050000_ExecuteVersionControlMigration.sql
```

### 分步执行（用于调试）
```sql
-- 步骤1: 检查外键依赖
\i Migration_AddVersion/20250221050000_CheckForeignKeyDependencies.sql

-- 步骤2: 数据修复
\i Migration_AddVersion/20250221050000_FixDataBeforeMigration.sql

-- 步骤3: 添加版本字段
\i Migration_AddVersion/20250221050000_AddVersionColumnsToRelatedTables.sql

-- 步骤4: 修改主键结构
\i Migration_AddVersion/20250221050000_ModifyWkDefinitionToCompositeKey.sql

-- 步骤5: 管理外键约束
\i Migration_AddVersion/20250221050000_ManageForeignKeysForVersionControl.sql
```

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
- `HXPOINTER_CANDIDATES` - 指针候选人（运行时数据）

## 备份建议

执行迁移前请备份数据库：
```bash
pg_dump -h localhost -U username -d database_name > backup_before_version_control.sql
```

## 验证步骤

执行迁移后，请验证以下内容：

1. **检查主键结构**
```sql
SELECT table_name, constraint_name, constraint_type 
FROM information_schema.table_constraints 
WHERE table_name IN ('HXWKDEFINITIONS', 'HXWKNODES') 
AND constraint_type = 'PRIMARY KEY';
```

2. **检查外键约束**
```sql
SELECT tc.table_name, tc.constraint_name, kcu.column_name
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu 
ON tc.constraint_name = kcu.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
AND tc.table_name IN ('HXWKNODES', 'HXDEFINITION_CANDIDATES', 'HXNODE_CANDIDATES', 'HX_NODES_APPLICATION_FORMS', 'HXWKINSTANCES');
```

3. **检查数据完整性**
```sql
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

## 注意事项

1. **执行前备份**：务必在执行前备份数据库
2. **测试环境**：建议先在测试环境执行迁移
3. **数据一致性**：确保所有相关表的 `VERSION` 字段都有正确的值
4. **外键约束**：迁移会删除并重建外键约束，请确保了解影响范围
5. **回滚准备**：如果迁移失败，可以使用回滚脚本恢复

## 故障排除

如果遇到问题，请参考 `README_CompositeKeyMigration.md` 中的故障排除部分。

## 联系信息

如有问题，请联系开发团队。 