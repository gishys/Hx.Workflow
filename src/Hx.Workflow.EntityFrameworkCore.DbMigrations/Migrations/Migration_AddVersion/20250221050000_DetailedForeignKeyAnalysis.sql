-- 详细的外键依赖分析脚本
-- 执行时间：2025-02-21 05:00:00
-- 用途：全面分析数据库中的外键依赖关系

DO $$
DECLARE
    fk_record RECORD;
    pk_record RECORD;
    total_fk_count INTEGER := 0;
    affected_tables TEXT[] := ARRAY['HXWKDEFINITIONS', 'HXWKNODES', 'HXWKINSTANCES'];
    table_name TEXT;
BEGIN
    RAISE NOTICE '==========================================';
    RAISE NOTICE '详细的外键依赖分析';
    RAISE NOTICE '==========================================';
    
    -- 1. 检查所有主键约束
    RAISE NOTICE '';
    RAISE NOTICE '主键约束检查:';
    RAISE NOTICE '----------------------------------------';
    
    FOR pk_record IN
        SELECT 
            table_name,
            constraint_name,
            constraint_type
        FROM information_schema.table_constraints 
        WHERE table_name IN ('HXWKDEFINITIONS', 'HXWKNODES', 'HXWKINSTANCES')
        AND constraint_type = 'PRIMARY KEY'
        ORDER BY table_name
    LOOP
        RAISE NOTICE '表: %, 主键约束: %', pk_record.table_name, pk_record.constraint_name;
    END LOOP;
    
    -- 2. 检查所有外键约束
    RAISE NOTICE '';
    RAISE NOTICE '外键约束详细分析:';
    RAISE NOTICE '----------------------------------------';
    
    FOR fk_record IN
        SELECT 
            tc.table_name,
            tc.constraint_name,
            kcu.column_name,
            ccu.table_name AS referenced_table_name,
            ccu.column_name AS referenced_column_name,
            tc.constraint_type
        FROM information_schema.table_constraints tc
        JOIN information_schema.key_column_usage kcu 
            ON tc.constraint_name = kcu.constraint_name
        JOIN information_schema.constraint_column_usage ccu 
            ON tc.constraint_name = ccu.constraint_name
        WHERE tc.constraint_type = 'FOREIGN KEY'
        AND (tc.table_name = ANY(affected_tables) OR ccu.table_name = ANY(affected_tables))
        ORDER BY tc.table_name, tc.constraint_name
    LOOP
        RAISE NOTICE '表: %, 约束: %, 外键列: %, 引用: %.%',
            fk_record.table_name,
            fk_record.constraint_name,
            fk_record.column_name,
            fk_record.referenced_table_name,
            fk_record.referenced_column_name;
        total_fk_count := total_fk_count + 1;
    END LOOP;
    
    -- 3. 检查数据引用情况
    RAISE NOTICE '';
    RAISE NOTICE '数据引用情况检查:';
    RAISE NOTICE '----------------------------------------';
    
    -- 检查 HXWKNODES 引用 HXWKDEFINITIONS
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'HXWKNODES') THEN
        RAISE NOTICE 'HXWKNODES 表:';
        RAISE NOTICE '  - 总记录数: %', (SELECT COUNT(*) FROM "HXWKNODES");
        RAISE NOTICE '  - 引用 HXWKDEFINITIONS 的记录数: %', 
            (SELECT COUNT(*) FROM "HXWKNODES" WHERE "WKDIFINITIONID" IS NOT NULL);
        RAISE NOTICE '  - 空引用记录数: %', 
            (SELECT COUNT(*) FROM "HXWKNODES" WHERE "WKDIFINITIONID" IS NULL);
    END IF;
    
    -- 检查 HXDEFINITION_CANDIDATES 引用 HXWKDEFINITIONS
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'HXDEFINITION_CANDIDATES') THEN
        RAISE NOTICE 'HXDEFINITION_CANDIDATES 表:';
        RAISE NOTICE '  - 总记录数: %', (SELECT COUNT(*) FROM "HXDEFINITION_CANDIDATES");
        RAISE NOTICE '  - 引用 HXWKDEFINITIONS 的记录数: %', 
            (SELECT COUNT(*) FROM "HXDEFINITION_CANDIDATES" WHERE "NODEID" IS NOT NULL);
        RAISE NOTICE '  - 空引用记录数: %', 
            (SELECT COUNT(*) FROM "HXDEFINITION_CANDIDATES" WHERE "NODEID" IS NULL);
    END IF;
    
    -- 检查 HXWKINSTANCES 引用 HXWKDEFINITIONS
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'HXWKINSTANCES') THEN
        RAISE NOTICE 'HXWKINSTANCES 表:';
        RAISE NOTICE '  - 总记录数: %', (SELECT COUNT(*) FROM "HXWKINSTANCES");
        RAISE NOTICE '  - 引用 HXWKDEFINITIONS 的记录数: %', 
            (SELECT COUNT(*) FROM "HXWKINSTANCES" WHERE "WKDIFINITIONID" IS NOT NULL);
        RAISE NOTICE '  - 空引用记录数: %', 
            (SELECT COUNT(*) FROM "HXWKINSTANCES" WHERE "WKDIFINITIONID" IS NULL);
    END IF;
    
    -- 4. 检查索引情况
    RAISE NOTICE '';
    RAISE NOTICE '索引检查:';
    RAISE NOTICE '----------------------------------------';
    
    FOR table_name IN SELECT unnest(affected_tables)
    LOOP
        RAISE NOTICE '表 % 的索引:', table_name;
        FOR fk_record IN
            SELECT 
                indexname,
                indexdef
            FROM pg_indexes 
            WHERE tablename = table_name
            ORDER BY indexname
        LOOP
            RAISE NOTICE '  - %: %', fk_record.indexname, fk_record.indexdef;
        END LOOP;
    END LOOP;
    
    -- 5. 提供迁移建议
    RAISE NOTICE '';
    RAISE NOTICE '迁移建议:';
    RAISE NOTICE '----------------------------------------';
    RAISE NOTICE '1. 发现 % 个外键约束需要处理', total_fk_count;
    RAISE NOTICE '2. 建议按以下顺序执行迁移:';
    RAISE NOTICE '   a) 备份数据库';
    RAISE NOTICE '   b) 执行数据修复脚本';
    RAISE NOTICE '   c) 添加 VERSION 字段';
    RAISE NOTICE '   d) 删除主键约束（会级联删除外键约束）';
    RAISE NOTICE '   e) 创建复合主键';
    RAISE NOTICE '   f) 重新创建复合外键约束';
    RAISE NOTICE '3. 预计影响时间: 根据数据量，可能需要几分钟到几小时';
    
    RAISE NOTICE '';
    RAISE NOTICE '==========================================';
    RAISE NOTICE '分析完成';
    RAISE NOTICE '==========================================';
    
EXCEPTION
    WHEN OTHERS THEN
        RAISE EXCEPTION '分析过程中发生错误: %', SQLERRM;
END $$; 