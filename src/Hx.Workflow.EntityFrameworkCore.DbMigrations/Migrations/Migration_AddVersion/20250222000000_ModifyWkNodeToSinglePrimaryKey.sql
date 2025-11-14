-- 修改 WKNODES 及相关表从复合主键改为单一主键，并删除不需要的 Version 字段的 PostgreSQL 迁移脚本
-- 迁移时间：2025-02-22 00:00:00
--
-- 说明：
-- 1. WkNode 的 Version 字段实际上是 WkDefinition 的版本，用于外键关联
-- 2. WkNode 不需要自己的版本控制，因为已经通过 WkDefinition 的版本进行了隔离
-- 3. 因此将 WkNode 的主键从 (Id, Version) 改为 Id
-- 4. 相关的子实体（WkNodeCandidate、WkNodePara、WkNode_ApplicationForms）也需要相应调整
-- 5. WkNodeCandidate、WkNodePara、WkNode_ApplicationForms 不需要 Version 字段（已删除）
-- 6. DefinitionCandidate 需要保留 Version 字段（作为主键的一部分）

-- ============================================
-- 第一部分：修改 WKNODES 表的主键
-- ============================================

-- 1. 删除 WKNODES 表的所有主键约束（先查找并删除所有可能的主键约束）
DO $$
DECLARE
    pk_constraint_name TEXT;
BEGIN
    -- 查找表的主键约束名称
    SELECT constraint_name INTO pk_constraint_name
    FROM information_schema.table_constraints
    WHERE table_schema = current_schema()
      AND table_name = 'HXWKNODES'
      AND constraint_type = 'PRIMARY KEY'
    LIMIT 1;
    
    -- 如果找到主键约束，删除它
    IF pk_constraint_name IS NOT NULL THEN
        EXECUTE format('ALTER TABLE "HXWKNODES" DROP CONSTRAINT IF EXISTS %I CASCADE', pk_constraint_name);
        RAISE NOTICE '已删除 HXWKNODES 表的主键约束: %', pk_constraint_name;
    ELSE
        RAISE NOTICE 'HXWKNODES 表没有找到主键约束';
    END IF;
END $$;

-- 2. 创建单一主键
ALTER TABLE "HXWKNODES" ADD CONSTRAINT "PK_WKNODES" PRIMARY KEY ("ID");

-- 3. 创建索引以提高查询性能
CREATE INDEX IF NOT EXISTS "IX_HXWKNODES_WKDIFINITIONID_VERSION" ON "HXWKNODES" ("WKDIFINITIONID", "VERSION");
CREATE INDEX IF NOT EXISTS "IX_HXWKNODES_VERSION" ON "HXWKNODES" ("VERSION");

-- ============================================
-- 第二部分：修改 NODE_CANDIDATES 表的主键和外键，并删除 VERSION 字段
-- ============================================

-- 1. 删除旧的外键约束（如果存在，必须在删除主键之前删除，因为外键可能依赖主键）
ALTER TABLE "HXNODE_CANDIDATES" DROP CONSTRAINT IF EXISTS "FK_NODE_CANDIDATES_WKNODE_COMPOSITE";

-- 2. 删除 NODE_CANDIDATES 表的所有主键约束（先查找并删除所有可能的主键约束）
DO $$
DECLARE
    pk_constraint_name TEXT;
BEGIN
    -- 查找表的主键约束名称
    SELECT constraint_name INTO pk_constraint_name
    FROM information_schema.table_constraints
    WHERE table_schema = current_schema()
      AND table_name = 'HXNODE_CANDIDATES'
      AND constraint_type = 'PRIMARY KEY'
    LIMIT 1;
    
    -- 如果找到主键约束，删除它
    IF pk_constraint_name IS NOT NULL THEN
        EXECUTE format('ALTER TABLE "HXNODE_CANDIDATES" DROP CONSTRAINT IF EXISTS %I CASCADE', pk_constraint_name);
        RAISE NOTICE '已删除 HXNODE_CANDIDATES 表的主键约束: %', pk_constraint_name;
    ELSE
        RAISE NOTICE 'HXNODE_CANDIDATES 表没有找到主键约束';
    END IF;
END $$;

-- 3. 删除 VERSION 字段（如果存在，必须在创建新主键之前删除）
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = current_schema()
        AND table_name = 'HXNODE_CANDIDATES' 
        AND column_name = 'VERSION'
    ) THEN
        ALTER TABLE "HXNODE_CANDIDATES" DROP COLUMN "VERSION";
        RAISE NOTICE '已删除 HXNODE_CANDIDATES 表的 VERSION 字段';
    ELSE
        RAISE NOTICE 'HXNODE_CANDIDATES 表的 VERSION 字段不存在';
    END IF;
END $$;

-- 4. 创建新的复合主键（不包含 Version）
ALTER TABLE "HXNODE_CANDIDATES" ADD CONSTRAINT "PK_NODE_CANDIDATES" PRIMARY KEY ("NODEID", "CANDIDATEID");

-- 5. 创建新的外键约束（只关联 NodeId，不包含 Version）
ALTER TABLE "HXNODE_CANDIDATES" 
    ADD CONSTRAINT "FK_NODE_CANDIDATES_WKNODE" 
    FOREIGN KEY ("NODEID") 
    REFERENCES "HXWKNODES" ("ID") 
    ON DELETE CASCADE;

-- 6. 创建索引以提高查询性能
CREATE INDEX IF NOT EXISTS "IX_HXNODE_CANDIDATES_NODEID" ON "HXNODE_CANDIDATES" ("NODEID");
CREATE INDEX IF NOT EXISTS "IX_HXNODE_CANDIDATES_CANDIDATEID" ON "HXNODE_CANDIDATES" ("CANDIDATEID");

-- ============================================
-- 第三部分：修改 WKNODEPARAS 表的外键，并删除 VERSION 字段
-- ============================================

-- 1. 删除旧的外键约束（如果存在）
ALTER TABLE "HXWKNODEPARAS" DROP CONSTRAINT IF EXISTS "FK_WKNODEPARAS_WKNODE_COMPOSITE";

-- 2. 删除 VERSION 字段（如果存在，必须在创建新外键之前删除）
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = current_schema()
        AND table_name = 'HXWKNODEPARAS' 
        AND column_name = 'VERSION'
    ) THEN
        ALTER TABLE "HXWKNODEPARAS" DROP COLUMN "VERSION";
        RAISE NOTICE '已删除 HXWKNODEPARAS 表的 VERSION 字段';
    ELSE
        RAISE NOTICE 'HXWKNODEPARAS 表的 VERSION 字段不存在';
    END IF;
END $$;

-- 3. 创建新的外键约束（只关联 WkNodeId，不包含 Version）
ALTER TABLE "HXWKNODEPARAS" 
    ADD CONSTRAINT "FK_WKNODEPARAS_WKNODE" 
    FOREIGN KEY ("WKNODEID") 
    REFERENCES "HXWKNODES" ("ID") 
    ON DELETE CASCADE;

-- 4. 创建索引以提高查询性能
CREATE INDEX IF NOT EXISTS "IX_HXWKNODEPARAS_WKNODEID" ON "HXWKNODEPARAS" ("WKNODEID");

-- ============================================
-- 第四部分：修改 _NODES_APPLICATION_FORMS 表的主键和外键，并删除 VERSION 字段
-- ============================================

-- 1. 删除旧的外键约束（如果存在，必须在删除主键之前删除）
ALTER TABLE "HX_NODES_APPLICATION_FORMS" DROP CONSTRAINT IF EXISTS "FK_NODES_APPLICATION_FORMS_WKNODE_COMPOSITE";

-- 2. 删除 _NODES_APPLICATION_FORMS 表的所有主键约束（先查找并删除所有可能的主键约束）
DO $$
DECLARE
    pk_constraint_name TEXT;
BEGIN
    -- 查找表的主键约束名称
    SELECT constraint_name INTO pk_constraint_name
    FROM information_schema.table_constraints
    WHERE table_schema = current_schema()
      AND table_name = 'HX_NODES_APPLICATION_FORMS'
      AND constraint_type = 'PRIMARY KEY'
    LIMIT 1;
    
    -- 如果找到主键约束，删除它
    IF pk_constraint_name IS NOT NULL THEN
        EXECUTE format('ALTER TABLE "HX_NODES_APPLICATION_FORMS" DROP CONSTRAINT IF EXISTS %I CASCADE', pk_constraint_name);
        RAISE NOTICE '已删除 HX_NODES_APPLICATION_FORMS 表的主键约束: %', pk_constraint_name;
    ELSE
        RAISE NOTICE 'HX_NODES_APPLICATION_FORMS 表没有找到主键约束';
    END IF;
END $$;

-- 3. 删除 VERSION 字段（如果存在，必须在创建新主键之前删除）
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = current_schema()
        AND table_name = 'HX_NODES_APPLICATION_FORMS' 
        AND column_name = 'VERSION'
    ) THEN
        ALTER TABLE "HX_NODES_APPLICATION_FORMS" DROP COLUMN "VERSION";
        RAISE NOTICE '已删除 HX_NODES_APPLICATION_FORMS 表的 VERSION 字段';
    ELSE
        RAISE NOTICE 'HX_NODES_APPLICATION_FORMS 表的 VERSION 字段不存在';
    END IF;
END $$;

-- 4. 创建新的复合主键（不包含 Version）
ALTER TABLE "HX_NODES_APPLICATION_FORMS" ADD CONSTRAINT "PK_NODES_APPLICATION_FORMS" PRIMARY KEY ("NODE_ID", "APPLICATION_ID");

-- 5. 创建新的外键约束（只关联 NodeId，不包含 Version）
ALTER TABLE "HX_NODES_APPLICATION_FORMS" 
    ADD CONSTRAINT "FK_NODES_APPLICATION_FORMS_WKNODE" 
    FOREIGN KEY ("NODE_ID") 
    REFERENCES "HXWKNODES" ("ID") 
    ON DELETE CASCADE;

-- 6. 创建索引以提高查询性能
CREATE INDEX IF NOT EXISTS "IX_HX_NODES_APPLICATION_FORMS_NODE_ID" ON "HX_NODES_APPLICATION_FORMS" ("NODE_ID");
CREATE INDEX IF NOT EXISTS "IX_HX_NODES_APPLICATION_FORMS_APPLICATION_ID" ON "HX_NODES_APPLICATION_FORMS" ("APPLICATION_ID");

-- ============================================
-- 第五部分：修改 HXDEFINITION_CANDIDATES 表的主键，添加 VERSION 字段到主键
-- ============================================

-- 1. 确保 VERSION 字段存在（如果不存在则添加）
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = current_schema()
        AND table_name = 'HXDEFINITION_CANDIDATES' 
        AND column_name = 'VERSION'
    ) THEN
        ALTER TABLE "HXDEFINITION_CANDIDATES" ADD COLUMN "VERSION" INTEGER NOT NULL DEFAULT 1;
        RAISE NOTICE '已为 HXDEFINITION_CANDIDATES 添加 VERSION 字段';
    ELSE
        RAISE NOTICE 'HXDEFINITION_CANDIDATES 的 VERSION 字段已存在';
    END IF;
    
    -- 确保 VERSION 字段不为 NULL
    UPDATE "HXDEFINITION_CANDIDATES" SET "VERSION" = 1 WHERE "VERSION" IS NULL OR "VERSION" <= 0;
    ALTER TABLE "HXDEFINITION_CANDIDATES" ALTER COLUMN "VERSION" SET NOT NULL;
    RAISE NOTICE '已确保 HXDEFINITION_CANDIDATES 的 VERSION 字段不为 NULL';
END $$;

-- 2. 删除旧的外键约束（如果存在，必须在删除主键之前删除）
ALTER TABLE "HXDEFINITION_CANDIDATES" DROP CONSTRAINT IF EXISTS "Pk_WkDef_Candidate";
ALTER TABLE "HXDEFINITION_CANDIDATES" DROP CONSTRAINT IF EXISTS "FK_DEFINITION_CANDIDATES_WKDEFINITION_COMPOSITE";

-- 3. 删除 HXDEFINITION_CANDIDATES 表的所有主键约束（先查找并删除所有可能的主键约束）
DO $$
DECLARE
    pk_constraint_name TEXT;
BEGIN
    -- 查找表的主键约束名称
    SELECT constraint_name INTO pk_constraint_name
    FROM information_schema.table_constraints
    WHERE table_schema = current_schema()
      AND table_name = 'HXDEFINITION_CANDIDATES'
      AND constraint_type = 'PRIMARY KEY'
    LIMIT 1;
    
    -- 如果找到主键约束，删除它
    IF pk_constraint_name IS NOT NULL THEN
        EXECUTE format('ALTER TABLE "HXDEFINITION_CANDIDATES" DROP CONSTRAINT IF EXISTS %I CASCADE', pk_constraint_name);
        RAISE NOTICE '已删除 HXDEFINITION_CANDIDATES 表的主键约束: %', pk_constraint_name;
    ELSE
        RAISE NOTICE 'HXDEFINITION_CANDIDATES 表没有找到主键约束';
    END IF;
END $$;

-- 4. 创建新的复合主键（包含 NodeId, CandidateId, Version）
DO $$
BEGIN
    ALTER TABLE "HXDEFINITION_CANDIDATES" 
        ADD CONSTRAINT "PK_HXDEFINITION_CANDIDATES" 
        PRIMARY KEY ("NODEID", "CANDIDATEID", "VERSION");
    
    RAISE NOTICE '已创建 HXDEFINITION_CANDIDATES 表的复合主键 (NODEID, CANDIDATEID, VERSION)';
END $$;

-- 5. 创建新的外键约束（关联到 WkDefinition 的复合主键 (Id, Version)）
DO $$
BEGIN
    ALTER TABLE "HXDEFINITION_CANDIDATES" 
        ADD CONSTRAINT "FK_DEFINITION_CANDIDATES_WKDEFINITION_COMPOSITE" 
        FOREIGN KEY ("NODEID", "VERSION") 
        REFERENCES "HXWKDEFINITIONS" ("ID", "VERSION") 
        ON DELETE CASCADE;
    
    RAISE NOTICE '已创建 HXDEFINITION_CANDIDATES 表的外键约束 FK_DEFINITION_CANDIDATES_WKDEFINITION_COMPOSITE';
END $$;

-- 6. 创建索引以提高查询性能
DO $$
BEGIN
    CREATE INDEX IF NOT EXISTS "IX_HXDEFINITION_CANDIDATES_NODEID" ON "HXDEFINITION_CANDIDATES" ("NODEID");
    CREATE INDEX IF NOT EXISTS "IX_HXDEFINITION_CANDIDATES_CANDIDATEID" ON "HXDEFINITION_CANDIDATES" ("CANDIDATEID");
    CREATE INDEX IF NOT EXISTS "IX_HXDEFINITION_CANDIDATES_VERSION" ON "HXDEFINITION_CANDIDATES" ("VERSION");
    CREATE INDEX IF NOT EXISTS "IX_HXDEFINITION_CANDIDATES_NODEID_VERSION" ON "HXDEFINITION_CANDIDATES" ("NODEID", "VERSION");
    
    RAISE NOTICE '已创建 HXDEFINITION_CANDIDATES 表的索引';
END $$;

-- ============================================
-- 第六部分：验证数据完整性
-- ============================================

-- 检查是否有孤立的数据
DO $$
DECLARE
    orphan_count INTEGER;
BEGIN
    -- 检查 NODE_CANDIDATES 中是否有孤立记录
    SELECT COUNT(*) INTO orphan_count
    FROM "HXNODE_CANDIDATES" nc
    LEFT JOIN "HXWKNODES" n ON nc."NODEID" = n."ID"
    WHERE n."ID" IS NULL;
    
    IF orphan_count > 0 THEN
        RAISE WARNING '发现 % 条 NODE_CANDIDATES 记录没有对应的 WKNODES 记录', orphan_count;
    ELSE
        RAISE NOTICE 'NODE_CANDIDATES 表数据完整性检查通过';
    END IF;
    
    -- 检查 WKNODEPARAS 中是否有孤立记录
    SELECT COUNT(*) INTO orphan_count
    FROM "HXWKNODEPARAS" np
    LEFT JOIN "HXWKNODES" n ON np."WKNODEID" = n."ID"
    WHERE n."ID" IS NULL;
    
    IF orphan_count > 0 THEN
        RAISE WARNING '发现 % 条 WKNODEPARAS 记录没有对应的 WKNODES 记录', orphan_count;
    ELSE
        RAISE NOTICE 'WKNODEPARAS 表数据完整性检查通过';
    END IF;
    
    -- 检查 _NODES_APPLICATION_FORMS 中是否有孤立记录
    SELECT COUNT(*) INTO orphan_count
    FROM "HX_NODES_APPLICATION_FORMS" naf
    LEFT JOIN "HXWKNODES" n ON naf."NODE_ID" = n."ID"
    WHERE n."ID" IS NULL;
    
    IF orphan_count > 0 THEN
        RAISE WARNING '发现 % 条 _NODES_APPLICATION_FORMS 记录没有对应的 WKNODES 记录', orphan_count;
    ELSE
        RAISE NOTICE '_NODES_APPLICATION_FORMS 表数据完整性检查通过';
    END IF;
    
    -- 检查 DEFINITION_CANDIDATES 中是否有孤立记录（关联到 WkDefinition 的复合主键）
    SELECT COUNT(*) INTO orphan_count
    FROM "HXDEFINITION_CANDIDATES" dc
    LEFT JOIN "HXWKDEFINITIONS" d ON dc."NODEID" = d."ID" AND dc."VERSION" = d."VERSION"
    WHERE d."ID" IS NULL;
    
    IF orphan_count > 0 THEN
        RAISE WARNING '发现 % 条 DEFINITION_CANDIDATES 记录没有对应的 WKDEFINITIONS 记录', orphan_count;
    ELSE
        RAISE NOTICE 'DEFINITION_CANDIDATES 表数据完整性检查通过';
    END IF;
END $$;

-- ============================================
-- 迁移完成
-- ============================================
-- 注意：
-- 1. WKNODES 表的 VERSION 字段保留（数据库列名保持为 VERSION，代码中已改名为 WkDefinitionVersion）
-- 2. Version 字段不再作为 WKNODES 主键的一部分，仅作为外键的一部分
-- 3. 所有外键约束都已更新为只关联主键，不包含 Version（除了 WKNODES 和 DEFINITION_CANDIDATES）
-- 4. NODE_CANDIDATES、WKNODEPARAS、_NODES_APPLICATION_FORMS 表的 VERSION 字段已删除
-- 5. DEFINITION_CANDIDATES 表的 VERSION 字段保留（作为主键的一部分）
-- 6. DEFINITION_CANDIDATES 表的主键已更新为 (NODEID, CANDIDATEID, VERSION) - 三个字段
-- 7. DEFINITION_CANDIDATES 表的外键已更新为关联到 WKDEFINITIONS 的复合主键 (ID, VERSION)
-- 8. 数据完整性已通过验证
