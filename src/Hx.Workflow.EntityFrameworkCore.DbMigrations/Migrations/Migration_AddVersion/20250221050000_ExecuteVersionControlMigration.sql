-- 工作流模板版本控制迁移执行脚本
-- 执行时间：2025-02-21 05:00:00
-- 注意：请在执行前备份数据库

DO $$
BEGIN
    RAISE NOTICE '开始执行工作流模板版本控制迁移...';
    RAISE NOTICE '==========================================';
END $$;

-- 步骤0: 检查外键依赖关系（可选，用于了解影响范围）
\i 20250221050000_CheckForeignKeyDependencies.sql

-- 步骤1: 数据修复（修复现有数据中的 VERSION=0 或 NULL 记录）
\i 20250221050000_FixDataBeforeMigration.sql

-- 步骤2: 添加 VERSION 字段到需要版本控制的表
\i 20250221050000_AddVersionColumnsToRelatedTables.sql

-- 步骤3: 修改 HXWKDEFINITIONS 表为主键结构
\i 20250221050000_ModifyWkDefinitionToCompositeKey.sql

-- 步骤4: 管理外键约束（删除旧约束，重建复合外键）
\i 20250221050000_ManageForeignKeysForVersionControl.sql

DO $$
BEGIN
    RAISE NOTICE '==========================================';
    RAISE NOTICE '工作流模板版本控制迁移执行完成！';
    RAISE NOTICE '请检查以下内容：';
    RAISE NOTICE '1. 所有需要版本控制的表都有 VERSION 字段';
    RAISE NOTICE '2. HXWKDEFINITIONS 表有复合主键 (ID, VERSION)';
    RAISE NOTICE '3. 所有复合外键约束已正确创建';
    RAISE NOTICE '4. 数据完整性验证通过';
    RAISE NOTICE '==========================================';
END $$; 