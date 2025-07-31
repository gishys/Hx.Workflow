-- 修复WkStepBodyParam的外键关系
-- 将WKNODEID字段重命名为WKSTEPBODYID，并更新外键约束

-- 删除旧的外键约束
DO $$ 
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'Pk_WkStepBody_WkParam'
    ) THEN
        ALTER TABLE "HXWKSTEPBODYPARAMS" DROP CONSTRAINT "Pk_WkStepBody_WkParam";
        RAISE NOTICE '已删除旧的外键约束Pk_WkStepBody_WkParam';
    ELSE
        RAISE NOTICE '外键约束Pk_WkStepBody_WkParam不存在';
    END IF;
END $$;

-- 重命名字段
DO $$ 
BEGIN
    -- 检查字段是否存在
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'HXWKSTEPBODYPARAMS' 
        AND column_name = 'WkNodeId'
    ) THEN
        -- 重命名字段
        ALTER TABLE "HXWKSTEPBODYPARAMS" RENAME COLUMN "WkNodeId" TO "WKSTEPBODYID";
        RAISE NOTICE '已将WkNodeId字段重命名为WKSTEPBODYID';
    ELSE
        RAISE NOTICE 'WkNodeId字段不存在，可能已经重命名';
    END IF;
END $$;

-- 重命名StepBodyParaType字段
DO $$ 
BEGIN
    -- 检查字段是否存在
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'HXWKSTEPBODYPARAMS' 
        AND column_name = 'StepBodyParaType'
    ) THEN
        -- 重命名字段
        ALTER TABLE "HXWKSTEPBODYPARAMS" RENAME COLUMN "StepBodyParaType" TO "STEPBODYPARATYPE";
        RAISE NOTICE '已将StepBodyParaType字段重命名为STEPBODYPARATYPE';
    ELSE
        RAISE NOTICE 'StepBodyParaType字段不存在，可能已经重命名';
    END IF;
END $$;

-- 重命名HXWKEXECUTIONPOINTER表的Materials字段
DO $$ 
BEGIN
    -- 检查字段是否存在
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'HXWKEXECUTIONPOINTER' 
        AND column_name = 'Materials'
    ) THEN
        -- 重命名字段
        ALTER TABLE "HXWKEXECUTIONPOINTER" RENAME COLUMN "Materials" TO "MATERIALS";
        RAISE NOTICE '已将HXWKEXECUTIONPOINTER表的Materials字段重命名为MATERIALS';
    ELSE
        RAISE NOTICE 'HXWKEXECUTIONPOINTER表的Materials字段不存在，可能已经重命名';
    END IF;
END $$;

-- 添加新的外键约束
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_WKSTEPBODYPARAMS_WKSTEPBODY'
    ) THEN
        ALTER TABLE "HXWKSTEPBODYPARAMS" 
        ADD CONSTRAINT "FK_WKSTEPBODYPARAMS_WKSTEPBODY" 
        FOREIGN KEY ("WKSTEPBODYID") 
        REFERENCES "HXWKSTEPBODYS"("Id") 
        ON DELETE CASCADE;
        
        RAISE NOTICE '已添加新的外键约束FK_WKSTEPBODYPARAMS_WKSTEPBODY';
    ELSE
        RAISE NOTICE '外键约束FK_WKSTEPBODYPARAMS_WKSTEPBODY已存在';
    END IF;
END $$; 