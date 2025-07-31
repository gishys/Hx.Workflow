-- 检查 HXWKDEFINITIONS 表主键约束的依赖关系
-- 执行时间：2025-02-21 05:00:00
-- 用途：在删除主键约束前，检查会影响哪些外键约束

DO $$
DECLARE
    constraint_record RECORD;
    dependency_count INTEGER := 0;
BEGIN
    RAISE NOTICE '==========================================';
    RAISE NOTICE '检查 HXWKDEFINITIONS 表主键约束的依赖关系';
    RAISE NOTICE '==========================================';
    
    -- 1. 检查主键约束是否存在
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE table_name = 'HXWKDEFINITIONS' 
        AND constraint_name = 'PK_WKDEFINITION'
        AND constraint_type = 'PRIMARY KEY'
    ) THEN
        RAISE NOTICE '✓ 找到主键约束: PK_WKDEFINITION';
    ELSE
        RAISE NOTICE '✗ 未找到主键约束: PK_WKDEFINITION';
        RETURN;
    END IF;
    
    -- 2. 查找所有依赖 HXWKDEFINITIONS.ID 的外键约束
    RAISE NOTICE '';
    RAISE NOTICE '依赖 HXWKDEFINITIONS.ID 的外键约束列表:';
    RAISE NOTICE '----------------------------------------';
    
    FOR constraint_record IN
        SELECT 
            tc.table_name,
            tc.constraint_name,
            kcu.column_name,
            ccu.table_name AS referenced_table_name,
            ccu.column_name AS referenced_column_name
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu 
            ON tc.constraint_name = kcu.constraint_name
        JOIN information_schema.constraint_column_usage ccu 
            ON tc.constraint_name = ccu.constraint_name
        WHERE tc.constraint_type = 'FOREIGN KEY'
        AND ccu.table_name = 'HXWKDEFINITIONS'
        AND ccu.column_name = 'ID'
        ORDER BY tc.table_name, tc.constraint_name
    LOOP
        RAISE NOTICE '表: %, 约束: %, 外键列: %, 引用: %.%',
            constraint_record.table_name,
            constraint_record.constraint_name,
            constraint_record.column_name,
            constraint_record.referenced_table_name,
            constraint_record.referenced_column_name;
        dependency_count := dependency_count + 1;
    END LOOP;
    
    -- 3. 显示统计信息
    RAISE NOTICE '';
    RAISE NOTICE '统计信息:';
    RAISE NOTICE '----------------------------------------';
    RAISE NOTICE '总共发现 % 个依赖的外键约束', dependency_count;
    
    -- 4. 检查是否会影响数据完整性
    RAISE NOTICE '';
    RAISE NOTICE '数据完整性检查:';
    RAISE NOTICE '----------------------------------------';
    
    -- 检查 HXWKNODES 表
    IF EXISTS (SELECT 1 FROM "HXWKNODES" WHERE "WKDIFINITIONID" IS NOT NULL) THEN
        RAISE NOTICE 'HXWKNODES 表有 % 条记录引用了 HXWKDEFINITIONS', 
            (SELECT COUNT(*) FROM "HXWKNODES" WHERE "WKDIFINITIONID" IS NOT NULL);
    END IF;
    
    -- 检查 HXDEFINITION_CANDIDATES 表
    IF EXISTS (SELECT 1 FROM "HXDEFINITION_CANDIDATES" WHERE "NODEID" IS NOT NULL) THEN
        RAISE NOTICE 'HXDEFINITION_CANDIDATES 表有 % 条记录引用了 HXWKDEFINITIONS', 
            (SELECT COUNT(*) FROM "HXDEFINITION_CANDIDATES" WHERE "NODEID" IS NOT NULL);
    END IF;
    
    -- 检查 HXWKINSTANCES 表
    IF EXISTS (SELECT 1 FROM "HXWKINSTANCES" WHERE "WKDIFINITIONID" IS NOT NULL) THEN
        RAISE NOTICE 'HXWKINSTANCES 表有 % 条记录引用了 HXWKDEFINITIONS', 
            (SELECT COUNT(*) FROM "HXWKINSTANCES" WHERE "WKDIFINITIONID" IS NOT NULL);
    END IF;
    
    -- 5. 提供建议
    RAISE NOTICE '';
    RAISE NOTICE '建议:';
    RAISE NOTICE '----------------------------------------';
    IF dependency_count > 0 THEN
        RAISE NOTICE '⚠️  删除主键约束将同时删除 % 个外键约束', dependency_count;
        RAISE NOTICE '⚠️  建议在执行前备份相关表的数据';
        RAISE NOTICE '⚠️  删除后需要重新创建复合外键约束';
    ELSE
        RAISE NOTICE '✓ 没有发现依赖的外键约束，可以安全删除主键约束';
    END IF;
    
    RAISE NOTICE '';
    RAISE NOTICE '==========================================';
    RAISE NOTICE '检查完成';
    RAISE NOTICE '==========================================';
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION '检查过程中发生错误: %', SQLERRM;
END $$; 