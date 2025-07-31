-- 修复现有数据，确保 VERSION 字段有正确的值
-- 注意：ExePointerCandidate 不需要版本控制，所以不处理 HXPOINTER_CANDIDATES

DO $$
BEGIN
    RAISE NOTICE '开始修复现有数据的 VERSION 字段...';
    
    -- 修复 HXWKDEFINITIONS 表的 VERSION 数据
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'HXWKDEFINITIONS' AND column_name = 'VERSION') THEN
        UPDATE "HXWKDEFINITIONS" SET "VERSION" = 1 WHERE "VERSION" IS NULL OR "VERSION" = 0;
        RAISE NOTICE '已修复 HXWKDEFINITIONS 表的 VERSION 数据';
    END IF;
    
    -- 修复 HXDEFINITION_CANDIDATES 表的 VERSION 数据
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'HXDEFINITION_CANDIDATES' AND column_name = 'VERSION') THEN
        UPDATE "HXDEFINITION_CANDIDATES" SET "VERSION" = 1 WHERE "VERSION" IS NULL OR "VERSION" = 0;
        RAISE NOTICE '已修复 HXDEFINITION_CANDIDATES 表的 VERSION 数据';
    END IF;
    
    -- 修复 HXNODE_CANDIDATES 表的 VERSION 数据
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'HXNODE_CANDIDATES' AND column_name = 'VERSION') THEN
        UPDATE "HXNODE_CANDIDATES" SET "VERSION" = 1 WHERE "VERSION" IS NULL OR "VERSION" = 0;
        RAISE NOTICE '已修复 HXNODE_CANDIDATES 表的 VERSION 数据';
    END IF;
    
    -- 修复 HXWKNODES 表的 VERSION 数据
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'HXWKNODES' AND column_name = 'VERSION') THEN
        UPDATE "HXWKNODES" SET "VERSION" = 1 WHERE "VERSION" IS NULL OR "VERSION" = 0;
        RAISE NOTICE '已修复 HXWKNODES 表的 VERSION 数据';
    END IF;
    
    -- 修复 HXWKINSTANCES 表的 VERSION 数据
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'HXWKINSTANCES' AND column_name = 'VERSION') THEN
        UPDATE "HXWKINSTANCES" SET "VERSION" = 1 WHERE "VERSION" IS NULL OR "VERSION" = 0;
        RAISE NOTICE '已修复 HXWKINSTANCES 表的 VERSION 数据';
    END IF;
    
    -- 修复 HX_NODES_APPLICATION_FORMS 表的 VERSION 数据
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'HX_NODES_APPLICATION_FORMS' AND column_name = 'VERSION') THEN
        UPDATE "HX_NODES_APPLICATION_FORMS" SET "VERSION" = 1 WHERE "VERSION" IS NULL OR "VERSION" = 0;
        RAISE NOTICE '已修复 HX_NODES_APPLICATION_FORMS 表的 VERSION 数据';
    END IF;
    
    RAISE NOTICE '数据修复完成';
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION '数据修复过程中发生错误: %', SQLERRM;
END $$;

-- 3. 检查并删除所有可能的外键约束
DO $$
BEGIN
    -- 检查并删除HXDEFINITION_CANDIDATES表的外键约束
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_name = 'HXDEFINITION_CANDIDATES' 
        AND constraint_type = 'FOREIGN KEY'
    ) THEN
        RAISE NOTICE '发现HXDEFINITION_CANDIDATES表外键约束，将在主迁移脚本中删除';
    ELSE
        RAISE NOTICE 'HXDEFINITION_CANDIDATES表没有外键约束';
    END IF;
    
    -- 检查并删除HXWKNODES表的外键约束
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_name = 'HXWKNODES' 
        AND constraint_type = 'FOREIGN KEY'
    ) THEN
        RAISE NOTICE '发现HXWKNODES表外键约束，将在主迁移脚本中删除';
    ELSE
        RAISE NOTICE 'HXWKNODES表没有外键约束';
    END IF;
    
    -- 检查并删除HXWKINSTANCES表的外键约束
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_name = 'HXWKINSTANCES' 
        AND constraint_type = 'FOREIGN KEY'
    ) THEN
        RAISE NOTICE '发现HXWKINSTANCES表外键约束，将在主迁移脚本中删除';
    ELSE
        RAISE NOTICE 'HXWKINSTANCES表没有外键约束';
    END IF;
    
    -- 检查并删除HXNODE_CANDIDATES表的外键约束
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_name = 'HXNODE_CANDIDATES' 
        AND constraint_type = 'FOREIGN KEY'
    ) THEN
        RAISE NOTICE '发现HXNODE_CANDIDATES表外键约束，将在主迁移脚本中删除';
    ELSE
        RAISE NOTICE 'HXNODE_CANDIDATES表没有外键约束';
    END IF;
    
    -- 检查并删除HX_NODES_APPLICATION_FORMS表的外键约束
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_name = 'HX_NODES_APPLICATION_FORMS' 
        AND constraint_type = 'FOREIGN KEY'
    ) THEN
        RAISE NOTICE '发现HX_NODES_APPLICATION_FORMS表外键约束，将在主迁移脚本中删除';
    ELSE
        RAISE NOTICE 'HX_NODES_APPLICATION_FORMS表没有外键约束';
    END IF;
END $$;

-- 4. 验证数据完整性
DO $$
BEGIN
    -- 检查HXWKDEFINITIONS表
    IF EXISTS (
        SELECT 1 FROM "HXWKDEFINITIONS" 
        WHERE "VERSION" IS NULL OR "VERSION" <= 0
    ) THEN
        RAISE EXCEPTION 'HXWKDEFINITIONS表仍存在VERSION为NULL或小于等于0的记录';
    END IF;
    
    -- 检查相关表
    IF EXISTS (
        SELECT 1 FROM "HXDEFINITION_CANDIDATES" 
        WHERE "VERSION" IS NULL OR "VERSION" <= 0
    ) THEN
        RAISE EXCEPTION 'HXDEFINITION_CANDIDATES表仍存在VERSION为NULL或小于等于0的记录';
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM "HXNODE_CANDIDATES" 
        WHERE "VERSION" IS NULL OR "VERSION" <= 0
    ) THEN
        RAISE EXCEPTION 'HXNODE_CANDIDATES表仍存在VERSION为NULL或小于等于0的记录';
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM "HXWKNODES" 
        WHERE "VERSION" IS NULL OR "VERSION" <= 0
    ) THEN
        RAISE EXCEPTION 'HXWKNODES表仍存在VERSION为NULL或小于等于0的记录';
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM "HXWKINSTANCES" 
        WHERE "VERSION" IS NULL OR "VERSION" <= 0
    ) THEN
        RAISE EXCEPTION 'HXWKINSTANCES表仍存在VERSION为NULL或小于等于0的记录';
    END IF;
    
    IF EXISTS (
        SELECT 1 FROM "HX_NODES_APPLICATION_FORMS" 
        WHERE "VERSION" IS NULL OR "VERSION" <= 0
    ) THEN
        RAISE EXCEPTION 'HX_NODES_APPLICATION_FORMS表仍存在VERSION为NULL或小于等于0的记录';
    END IF;
    
    RAISE NOTICE '数据完整性验证通过';
END $$; 