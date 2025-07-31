-- 为WKNODEPARAS表添加VERSION字段
DO $$ 
BEGIN
    -- 检查VERSION字段是否已存在
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'HXWKNODEPARAS' 
        AND column_name = 'VERSION'
    ) THEN
        -- 添加VERSION字段
        ALTER TABLE "HXWKNODEPARAS" ADD COLUMN "VERSION" integer NOT NULL DEFAULT 1;
        
        -- 更新现有记录的VERSION字段
        UPDATE "HXWKNODEPARAS" 
        SET "VERSION" = (
            SELECT COALESCE(MAX("VERSION"), 1) 
            FROM "HXWKNODES" 
            WHERE "HXWKNODES"."ID" = "HXWKNODEPARAS"."WKNODEID"
        );
        
        RAISE NOTICE 'VERSION字段已添加到HXWKNODEPARAS表';
    ELSE
        RAISE NOTICE 'HXWKNODEPARAS表的VERSION字段已存在';
    END IF;
END $$;

-- 删除旧的外键约束
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'Pk_WkNode_OutcomeSteps'
    ) THEN
        ALTER TABLE "HXWKNODEPARAS" DROP CONSTRAINT "Pk_WkNode_OutcomeSteps";
        RAISE NOTICE '已删除旧的外键约束Pk_WkNode_OutcomeSteps';
    ELSE
        RAISE NOTICE '外键约束Pk_WkNode_OutcomeSteps不存在';
    END IF;
END $$;


-- 添加新的复合外键约束
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_WKNODEPARAS_WKNODE_COMPOSITE'
    ) THEN
        ALTER TABLE "HXWKNODEPARAS" 
        ADD CONSTRAINT "FK_WKNODEPARAS_WKNODE_COMPOSITE" 
        FOREIGN KEY ("WKNODEID", "VERSION") 
        REFERENCES "HXWKNODES"("ID", "VERSION") 
        ON DELETE CASCADE;
        
        RAISE NOTICE '已添加新的复合外键约束FK_WKNODEPARAS_WKNODE_COMPOSITE';
    ELSE
        RAISE NOTICE '外键约束FK_WKNODEPARAS_WKNODE_COMPOSITE已存在';
    END IF;
END $$;
