-- =============================================
-- 添加 IsArchived 字段到 WkDefinition 表
-- 日期: 2025-02-24
-- 描述: 添加 IsArchived 字段用于版本归档管理
-- 
-- 业务逻辑：
-- 1. 已归档的版本不再用于模板管理（不能创建新实例）
-- 2. 已归档的版本仅用于服务已创建的实例
-- 3. 查询最新版本时会自动排除已归档的版本
-- =============================================

DO $$
BEGIN
    -- 检查列是否已存在
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = current_schema()
        AND table_name = 'HXWKDEFINITIONS'
        AND column_name = 'ISARCHIVED'
    ) THEN
        -- 添加 IsArchived 列，默认值为 false
        ALTER TABLE "HXWKDEFINITIONS"
        ADD COLUMN "ISARCHIVED" BOOLEAN NOT NULL DEFAULT false;
        
        -- 添加注释
        COMMENT ON COLUMN "HXWKDEFINITIONS"."ISARCHIVED" IS '是否已归档（已归档的版本不再用于模板管理，仅用于服务已创建的实例）';
        
        RAISE NOTICE '已添加 ISARCHIVED 列到 HXWKDEFINITIONS 表';
    ELSE
        RAISE NOTICE 'ISARCHIVED 列已存在，跳过添加';
    END IF;
    
    -- 确保现有记录的 IsArchived 值为 false（如果之前存在但为 NULL）
    UPDATE "HXWKDEFINITIONS"
    SET "ISARCHIVED" = false
    WHERE "ISARCHIVED" IS NULL;
    
    RAISE NOTICE '迁移完成：IsArchived 字段已添加到 WkDefinition 表';
END $$;

