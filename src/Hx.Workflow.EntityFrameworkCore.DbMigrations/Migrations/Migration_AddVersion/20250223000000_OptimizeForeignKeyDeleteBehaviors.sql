-- =============================================
-- 优化外键删除行为迁移脚本
-- 日期: 2025-02-23
-- 描述: 更新所有外键约束的删除行为，确保数据完整性和业务逻辑正确性
-- 
-- 注意: 本脚本会在创建外键约束前自动清理孤立数据
-- 孤立数据是指子表中的外键值在父表中不存在的记录
-- 清理操作会输出警告信息，显示清理的记录数量
-- =============================================

-- 1. WkInstance -> WkDefinition: 添加 Restrict 删除行为
-- 防止删除正在被实例使用的定义版本
DO $$
BEGIN
    -- 删除旧的外键约束
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_WKINSTANCES_WKDEFINITION_COMPOSITE'
        AND table_schema = current_schema()
    ) THEN
        ALTER TABLE "HXWKINSTANCES" 
        DROP CONSTRAINT "FK_WKINSTANCES_WKDEFINITION_COMPOSITE";
        RAISE NOTICE '已删除旧的外键约束 FK_WKINSTANCES_WKDEFINITION_COMPOSITE';
    END IF;
    
    -- 创建新的外键约束（Restrict）
    ALTER TABLE "HXWKINSTANCES"
    ADD CONSTRAINT "FK_WKINSTANCES_WKDEFINITION_COMPOSITE"
    FOREIGN KEY ("WKDIFINITIONID", "VERSION")
    REFERENCES "HXWKDEFINITIONS" ("ID", "VERSION")
    ON DELETE RESTRICT;
    
    RAISE NOTICE '已创建外键约束 FK_WKINSTANCES_WKDEFINITION_COMPOSITE (Restrict)';
END $$;

-- 2. WkAuditor -> WkInstance: 添加 Cascade 删除行为
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'Pk_WkAuditor_WkInstance'
        AND table_schema = current_schema()
    ) THEN
        ALTER TABLE "HXWKAUDITORS" 
        DROP CONSTRAINT "Pk_WkAuditor_WkInstance";
        RAISE NOTICE '已删除旧的外键约束 Pk_WkAuditor_WkInstance';
    END IF;
    
    -- 清理孤立数据：删除 WORKFLOWID 在 HXWKINSTANCES 中不存在的记录
    SELECT COUNT(*) INTO orphaned_count
    FROM "HXWKAUDITORS" a
    WHERE NOT EXISTS (
        SELECT 1 FROM "HXWKINSTANCES" i WHERE i."ID" = a."WORKFLOWID"
    );
    
    IF orphaned_count > 0 THEN
        DELETE FROM "HXWKAUDITORS"
        WHERE NOT EXISTS (
            SELECT 1 FROM "HXWKINSTANCES" i WHERE i."ID" = "HXWKAUDITORS"."WORKFLOWID"
        );
        RAISE WARNING '已清理 % 条孤立数据（WORKFLOWID 不存在）', orphaned_count;
    ELSE
        RAISE NOTICE '未发现孤立数据（WORKFLOWID）';
    END IF;
    
    ALTER TABLE "HXWKAUDITORS"
    ADD CONSTRAINT "Pk_WkAuditor_WkInstance"
    FOREIGN KEY ("WORKFLOWID")
    REFERENCES "HXWKINSTANCES" ("ID")
    ON DELETE CASCADE;
    
    RAISE NOTICE '已创建外键约束 Pk_WkAuditor_WkInstance (Cascade)';
END $$;

-- 3. WkAuditor -> WkExecutionPointer: 添加 Cascade 删除行为
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'Pk_WkAuditor_ExecPointer'
        AND table_schema = current_schema()
    ) THEN
        ALTER TABLE "HXWKAUDITORS" 
        DROP CONSTRAINT "Pk_WkAuditor_ExecPointer";
        RAISE NOTICE '已删除旧的外键约束 Pk_WkAuditor_ExecPointer';
    END IF;
    
    -- 清理孤立数据：删除 EXECUTIONPOINTERID 在 HXWKEXECUTIONPOINTER 中不存在的记录
    SELECT COUNT(*) INTO orphaned_count
    FROM "HXWKAUDITORS" a
    WHERE NOT EXISTS (
        SELECT 1 FROM "HXWKEXECUTIONPOINTER" p WHERE p."ID" = a."EXECUTIONPOINTERID"
    );
    
    IF orphaned_count > 0 THEN
        DELETE FROM "HXWKAUDITORS"
        WHERE NOT EXISTS (
            SELECT 1 FROM "HXWKEXECUTIONPOINTER" p WHERE p."ID" = "HXWKAUDITORS"."EXECUTIONPOINTERID"
        );
        RAISE WARNING '已清理 % 条孤立数据（EXECUTIONPOINTERID 不存在）', orphaned_count;
    ELSE
        RAISE NOTICE '未发现孤立数据（EXECUTIONPOINTERID）';
    END IF;
    
    ALTER TABLE "HXWKAUDITORS"
    ADD CONSTRAINT "Pk_WkAuditor_ExecPointer"
    FOREIGN KEY ("EXECUTIONPOINTERID")
    REFERENCES "HXWKEXECUTIONPOINTER" ("ID")
    ON DELETE CASCADE;
    
    RAISE NOTICE '已创建外键约束 Pk_WkAuditor_ExecPointer (Cascade)';
END $$;

-- 4. WkExecutionError -> WkInstance: 添加外键关系和 Cascade 删除行为
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    -- 删除旧的外键约束（如果存在）
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_WKEXECUTIONERRORS_WKINSTANCE'
        AND table_schema = current_schema()
    ) THEN
        ALTER TABLE "HXWKEXECUTIONERRORS" 
        DROP CONSTRAINT "FK_WKEXECUTIONERRORS_WKINSTANCE";
        RAISE NOTICE '已删除旧的外键约束 FK_WKEXECUTIONERRORS_WKINSTANCE';
    END IF;
    
    -- 清理孤立数据：删除 WKINSTANCEID 在 HXWKINSTANCES 中不存在的记录
    SELECT COUNT(*) INTO orphaned_count
    FROM "HXWKEXECUTIONERRORS" e
    WHERE NOT EXISTS (
        SELECT 1 FROM "HXWKINSTANCES" i WHERE i."ID" = e."WKINSTANCEID"
    );
    
    IF orphaned_count > 0 THEN
        DELETE FROM "HXWKEXECUTIONERRORS"
        WHERE NOT EXISTS (
            SELECT 1 FROM "HXWKINSTANCES" i WHERE i."ID" = "HXWKEXECUTIONERRORS"."WKINSTANCEID"
        );
        RAISE WARNING '已清理 % 条孤立数据（WKINSTANCEID 不存在）', orphaned_count;
    ELSE
        RAISE NOTICE '未发现孤立数据（WKINSTANCEID）';
    END IF;
    
    -- 创建新的外键约束
    ALTER TABLE "HXWKEXECUTIONERRORS"
    ADD CONSTRAINT "FK_WKEXECUTIONERRORS_WKINSTANCE"
    FOREIGN KEY ("WKINSTANCEID")
    REFERENCES "HXWKINSTANCES" ("ID")
    ON DELETE CASCADE;
    
    RAISE NOTICE '已创建外键约束 FK_WKEXECUTIONERRORS_WKINSTANCE (Cascade)';
END $$;

-- 5. WkExecutionError -> WkExecutionPointer: 添加外键关系和 Cascade 删除行为
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    -- 删除旧的外键约束（如果存在）
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_WKEXECUTIONERRORS_EXECUTIONPOINTER'
        AND table_schema = current_schema()
    ) THEN
        ALTER TABLE "HXWKEXECUTIONERRORS" 
        DROP CONSTRAINT "FK_WKEXECUTIONERRORS_EXECUTIONPOINTER";
        RAISE NOTICE '已删除旧的外键约束 FK_WKEXECUTIONERRORS_EXECUTIONPOINTER';
    END IF;
    
    -- 清理孤立数据：删除 WKEXECUTIONPOINTERID 在 HXWKEXECUTIONPOINTER 中不存在的记录
    SELECT COUNT(*) INTO orphaned_count
    FROM "HXWKEXECUTIONERRORS" e
    WHERE NOT EXISTS (
        SELECT 1 FROM "HXWKEXECUTIONPOINTER" p WHERE p."ID" = e."WKEXECUTIONPOINTERID"
    );
    
    IF orphaned_count > 0 THEN
        DELETE FROM "HXWKEXECUTIONERRORS"
        WHERE NOT EXISTS (
            SELECT 1 FROM "HXWKEXECUTIONPOINTER" p WHERE p."ID" = "HXWKEXECUTIONERRORS"."WKEXECUTIONPOINTERID"
        );
        RAISE WARNING '已清理 % 条孤立数据（WKEXECUTIONPOINTERID 不存在）', orphaned_count;
    ELSE
        RAISE NOTICE '未发现孤立数据（WKEXECUTIONPOINTERID）';
    END IF;
    
    -- 创建新的外键约束
    ALTER TABLE "HXWKEXECUTIONERRORS"
    ADD CONSTRAINT "FK_WKEXECUTIONERRORS_EXECUTIONPOINTER"
    FOREIGN KEY ("WKEXECUTIONPOINTERID")
    REFERENCES "HXWKEXECUTIONPOINTER" ("ID")
    ON DELETE CASCADE;
    
    RAISE NOTICE '已创建外键约束 FK_WKEXECUTIONERRORS_EXECUTIONPOINTER (Cascade)';
END $$;

-- 6. WkSubscription -> WkExecutionPointer: 添加 Cascade 删除行为
DO $$
DECLARE
    orphaned_count INTEGER;
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_EXTENSIONATTRIBUTES_POINTERS'
        AND table_schema = current_schema()
    ) THEN
        ALTER TABLE "HXWKSUBSCRIPTIONS" 
        DROP CONSTRAINT "FK_EXTENSIONATTRIBUTES_POINTERS";
        RAISE NOTICE '已删除旧的外键约束 FK_EXTENSIONATTRIBUTES_POINTERS';
    END IF;
    
    -- 清理孤立数据：删除 EXECUTIONPOINTERID 在 HXWKEXECUTIONPOINTER 中不存在的记录
    SELECT COUNT(*) INTO orphaned_count
    FROM "HXWKSUBSCRIPTIONS" s
    WHERE NOT EXISTS (
        SELECT 1 FROM "HXWKEXECUTIONPOINTER" p WHERE p."ID" = s."EXECUTIONPOINTERID"
    );
    
    IF orphaned_count > 0 THEN
        DELETE FROM "HXWKSUBSCRIPTIONS"
        WHERE NOT EXISTS (
            SELECT 1 FROM "HXWKEXECUTIONPOINTER" p WHERE p."ID" = "HXWKSUBSCRIPTIONS"."EXECUTIONPOINTERID"
        );
        RAISE WARNING '已清理 % 条孤立数据（EXECUTIONPOINTERID 不存在）', orphaned_count;
    ELSE
        RAISE NOTICE '未发现孤立数据（EXECUTIONPOINTERID）';
    END IF;
    
    ALTER TABLE "HXWKSUBSCRIPTIONS"
    ADD CONSTRAINT "FK_EXTENSIONATTRIBUTES_POINTERS"
    FOREIGN KEY ("EXECUTIONPOINTERID")
    REFERENCES "HXWKEXECUTIONPOINTER" ("ID")
    ON DELETE CASCADE;
    
    RAISE NOTICE '已创建外键约束 FK_EXTENSIONATTRIBUTES_POINTERS (Cascade)';
END $$;

-- 7. WkNode_ApplicationForms -> ApplicationForm: 添加 Restrict 删除行为
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'APLLICATION_FKEY'
        AND table_schema = current_schema()
    ) THEN
        ALTER TABLE "HX_NODES_APPLICATION_FORMS" 
        DROP CONSTRAINT "APLLICATION_FKEY";
        RAISE NOTICE '已删除旧的外键约束 APLLICATION_FKEY';
    END IF;
    
    ALTER TABLE "HX_NODES_APPLICATION_FORMS"
    ADD CONSTRAINT "APLLICATION_FKEY"
    FOREIGN KEY ("APPLICATION_ID")
    REFERENCES "HXAPPLICATIONFORMS" ("ID")
    ON DELETE RESTRICT;
    
    RAISE NOTICE '已创建外键约束 APLLICATION_FKEY (Restrict)';
END $$;

-- 8. WkDefinitionGroup -> WkDefinition: 改为 SetNull 删除行为
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'QI_GROUPS_WKDEFINITION_ID'
        AND table_schema = current_schema()
    ) THEN
        ALTER TABLE "HXWKDEFINITIONS" 
        DROP CONSTRAINT "QI_GROUPS_WKDEFINITION_ID";
        RAISE NOTICE '已删除旧的外键约束 QI_GROUPS_WKDEFINITION_ID';
    END IF;
    
    -- 确保 GroupId 字段允许为 NULL（如果还没有设置）
    BEGIN
        ALTER TABLE "HXWKDEFINITIONS" 
        ALTER COLUMN "GROUPID" DROP NOT NULL;
        RAISE NOTICE '已确保 GROUPID 字段允许为 NULL';
    EXCEPTION
        WHEN OTHERS THEN
            RAISE NOTICE 'GROUPID 字段可能已经是可空的';
    END;
    
    ALTER TABLE "HXWKDEFINITIONS"
    ADD CONSTRAINT "QI_GROUPS_WKDEFINITION_ID"
    FOREIGN KEY ("GROUPID")
    REFERENCES "HXWKDEFINITION_GROUPS" ("ID")
    ON DELETE SET NULL;
    
    RAISE NOTICE '已创建外键约束 QI_GROUPS_WKDEFINITION_ID (SetNull)';
END $$;

-- 9. ApplicationFormGroup -> ApplicationForm: 改为 SetNull 删除行为
-- 同时将 GroupId 字段重命名为 GROUPID 以保持命名一致性
DO $$
DECLARE
    has_old_column BOOLEAN := FALSE;
    has_new_column BOOLEAN := FALSE;
    idx_name TEXT;
BEGIN
    -- 检查列是否存在
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'HXAPPLICATIONFORMS'
          AND column_name = 'GroupId'
    ) INTO has_old_column;
    
    SELECT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = current_schema()
          AND table_name = 'HXAPPLICATIONFORMS'
          AND column_name = 'GROUPID'
    ) INTO has_new_column;
    
    -- 删除旧的外键约束
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'AF_GROUPS_APPLICATIONFORM_ID'
        AND table_schema = current_schema()
    ) THEN
        ALTER TABLE "HXAPPLICATIONFORMS" 
        DROP CONSTRAINT "AF_GROUPS_APPLICATIONFORM_ID";
        RAISE NOTICE '已删除旧的外键约束 AF_GROUPS_APPLICATIONFORM_ID';
    END IF;
    
    -- 如果存在旧的 GroupId 列，需要重命名并处理索引
    IF has_old_column AND NOT has_new_column THEN
        -- 删除包含 GroupId 的索引
        FOR idx_name IN
            SELECT indexname
            FROM pg_indexes
            WHERE schemaname = current_schema()
              AND tablename = 'HXAPPLICATIONFORMS'
              AND (indexdef LIKE '%GroupId%' OR indexdef LIKE '%"GroupId"%')
        LOOP
            EXECUTE format('DROP INDEX IF EXISTS %I.%I', current_schema(), idx_name);
            RAISE NOTICE '已删除索引: %', idx_name;
        END LOOP;
        
        -- 重命名列
        ALTER TABLE "HXAPPLICATIONFORMS" 
        RENAME COLUMN "GroupId" TO "GROUPID";
        RAISE NOTICE '已将 GroupId 字段重命名为 GROUPID';
        
        -- 重新创建唯一索引
        DROP INDEX IF EXISTS "IX_HXAPPLICATIONFORMS_GROUPID_TITLE";
        CREATE UNIQUE INDEX "IX_HXAPPLICATIONFORMS_GROUPID_TITLE"
        ON "HXAPPLICATIONFORMS" ("GROUPID", "TITLE");
        RAISE NOTICE '已重新创建唯一索引 IX_HXAPPLICATIONFORMS_GROUPID_TITLE';
    END IF;
    
    -- 确保 GROUPID 字段允许为 NULL（如果字段存在）
    IF has_new_column OR (has_old_column AND NOT has_new_column) THEN
        BEGIN
            ALTER TABLE "HXAPPLICATIONFORMS" 
            ALTER COLUMN "GROUPID" DROP NOT NULL;
            RAISE NOTICE '已确保 GROUPID 字段允许为 NULL';
        EXCEPTION
            WHEN OTHERS THEN
                -- 如果字段已经是可空的，忽略错误
                RAISE NOTICE 'GROUPID 字段可能已经是可空的';
        END;
    END IF;
    
    -- 创建新的外键约束（使用 GROUPID）
    IF has_new_column OR (has_old_column AND NOT has_new_column) THEN
        ALTER TABLE "HXAPPLICATIONFORMS"
        ADD CONSTRAINT "AF_GROUPS_APPLICATIONFORM_ID"
        FOREIGN KEY ("GROUPID")
        REFERENCES "HXAPPLICATIONFORM_GROUPS" ("ID")
        ON DELETE SET NULL;
        
        RAISE NOTICE '已创建外键约束 AF_GROUPS_APPLICATIONFORM_ID (SetNull)';
    END IF;
END $$;

-- 10. WkNode -> WkStepBody: 确保外键允许为 NULL（可选关系）
DO $$
BEGIN
    -- 确保 WkStepBodyId 字段允许为 NULL（如果还没有设置）
    BEGIN
        ALTER TABLE "HXWKNODES" 
        ALTER COLUMN "WKSTEPBODYID" DROP NOT NULL;
        RAISE NOTICE '已确保 WKSTEPBODYID 字段允许为 NULL';
    EXCEPTION
        WHEN OTHERS THEN
            -- 如果字段已经是可空的，忽略错误
            RAISE NOTICE 'WKSTEPBODYID 字段可能已经是可空的';
    END;
END $$;

-- 验证所有外键约束
DO $$
DECLARE
    constraint_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO constraint_count
    FROM information_schema.table_constraints
    WHERE constraint_type = 'FOREIGN KEY'
    AND table_schema = current_schema()
    AND constraint_name IN (
        'FK_WKINSTANCES_WKDEFINITION_COMPOSITE',
        'Pk_WkAuditor_WkInstance',
        'Pk_WkAuditor_ExecPointer',
        'FK_WKEXECUTIONERRORS_WKINSTANCE',
        'FK_WKEXECUTIONERRORS_EXECUTIONPOINTER',
        'FK_EXTENSIONATTRIBUTES_POINTERS',
        'APLLICATION_FKEY',
        'QI_GROUPS_WKDEFINITION_ID',
        'AF_GROUPS_APPLICATIONFORM_ID'
    );
    
    IF constraint_count = 9 THEN
        RAISE NOTICE '✅ 所有外键约束已成功创建 (共 % 个)', constraint_count;
    ELSE
        RAISE WARNING '⚠️ 外键约束数量不匹配: 期望 9 个，实际 % 个', constraint_count;
    END IF;
END $$;
