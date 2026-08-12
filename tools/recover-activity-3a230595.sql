-- Purpose: recover the stale activity submission created before request validation was deployed.
-- IMPORTANT: stop every workflow service instance before running this script.
-- The transaction aborts unless the pointer is still waiting and every matching event is unprocessed.

BEGIN;

LOCK TABLE public."HXWKEVENTS" IN SHARE ROW EXCLUSIVE MODE;
LOCK TABLE public."HXWKSUBSCRIPTIONS" IN SHARE ROW EXCLUSIVE MODE;
LOCK TABLE public."HXWKACTIVITYSUBMISSIONS" IN SHARE ROW EXCLUSIVE MODE;

DO $recovery$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM public."HXWKEXECUTIONPOINTER"
        WHERE "ID" = '3a230595-2e27-1c97-b5a6-db0071976f01'::uuid
          AND "WKINSTANCEID" = '3a22fcd1-5044-563d-d2da-93159d8272fd'::uuid
          AND "STEPNAME" = '核定'
          AND "STATUS" = 5
          AND "EVENTNAME" = 'WorkflowCore.Activity'
          AND "EVENTKEY" = '3a230595-2e27-1c97-b5a6-db0071976f01'
          AND "EVENTPUBLISHED" = FALSE
    ) THEN
        RAISE EXCEPTION '恢复已取消：执行指针不再是等待中的“核定”节点。';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM public."HXWKEVENTS"
        WHERE "EVENTKEY" = '3a230595-2e27-1c97-b5a6-db0071976f01'
          AND "ISPROCESSED" = TRUE
    ) THEN
        RAISE EXCEPTION '恢复已取消：错误事件已被处理，不能删除后重提。';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM public."HXWKEVENTS"
        WHERE "EVENTKEY" = '3a230595-2e27-1c97-b5a6-db0071976f01'
          AND "ISPROCESSED" = FALSE
    ) THEN
        RAISE EXCEPTION '恢复已取消：未找到待处理的错误事件。';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM public."HXWKSUBSCRIPTIONS"
        WHERE "WORKFLOWID" = '3a22fcd1-5044-563d-d2da-93159d8272fd'::uuid
          AND "EXECUTIONPOINTERID" = '3a230595-2e27-1c97-b5a6-db0071976f01'::uuid
          AND "EVENTKEY" = '3a230595-2e27-1c97-b5a6-db0071976f01'
    ) THEN
        RAISE EXCEPTION '恢复已取消：当前节点的活动订阅不存在。';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM public."HXWKACTIVITYSUBMISSIONS"
        WHERE "ID" = '3a23059c-8534-2bdf-410a-e909e8d30d9e'::uuid
          AND "WORKFLOWID" = '3a22fcd1-5044-563d-d2da-93159d8272fd'::uuid
          AND "ACTIVITYNAME" = '3a230595-2e27-1c97-b5a6-db0071976f01'
          AND "STATUS" = 2
    ) THEN
        RAISE EXCEPTION '恢复已取消：目标错误提交不存在或状态已变化。';
    END IF;
END
$recovery$;

DELETE FROM public."HXWKEVENTS"
WHERE "EVENTKEY" = '3a230595-2e27-1c97-b5a6-db0071976f01'
  AND "ISPROCESSED" = FALSE;

UPDATE public."HXWKSUBSCRIPTIONS"
SET "EXTERNALTOKEN" = NULL,
    "EXTERNALWORKERID" = NULL,
    "EXTERNALTOKENEXPIRY" = NULL
WHERE "WORKFLOWID" = '3a22fcd1-5044-563d-d2da-93159d8272fd'::uuid
  AND "EXECUTIONPOINTERID" = '3a230595-2e27-1c97-b5a6-db0071976f01'::uuid
  AND "EVENTKEY" = '3a230595-2e27-1c97-b5a6-db0071976f01';

DELETE FROM public."HXWKACTIVITYSUBMISSIONS"
WHERE "ID" = '3a23059c-8534-2bdf-410a-e909e8d30d9e'::uuid
  AND "WORKFLOWID" = '3a22fcd1-5044-563d-d2da-93159d8272fd'::uuid
  AND "ACTIVITYNAME" = '3a230595-2e27-1c97-b5a6-db0071976f01'
  AND "STATUS" = 2;

COMMIT;

-- After COMMIT:
-- 1. Start the workflow service.
-- 2. Submit only the correct request: step=核定, DecideBranching=登簿.
