-- One-time recovery for the Host-candidate activity that was consumed before
-- the one-of-many Host completion fix was deployed.
--
-- workflowId   = 3a230fe7-8162-35f4-e917-9c5a712f498e
-- activityName = 3a230fe9-580e-b452-e53d-c2fa1ea9c30c
-- submissionId = 3a230fea-9fc4-5c8d-d2d0-394efbe94d90
--
-- Required order:
-- 1. Deploy the fixed workflow package.
-- 2. Stop every process that can run the WorkflowCore host.
-- 3. Run this script against the workflow PostgreSQL database.
-- 4. Start exactly one WorkflowCore host and wait for the pointer to advance.
--
-- The script reuses the already-persisted ActivityResult, changes only its
-- SubscriptionId to the current waiting subscription, and makes the event
-- runnable again. It deliberately leaves the idempotency submission at status
-- EventPublished (2); the submission reconciler will set it to Succeeded (3)
-- after the pointer completes.

BEGIN;

SET LOCAL lock_timeout = '10s';

LOCK TABLE public."HXWKEXECUTIONPOINTER" IN SHARE ROW EXCLUSIVE MODE;
LOCK TABLE public."HXPOINTER_CANDIDATES" IN SHARE ROW EXCLUSIVE MODE;
LOCK TABLE public."HXWKEVENTS" IN SHARE ROW EXCLUSIVE MODE;
LOCK TABLE public."HXWKSUBSCRIPTIONS" IN SHARE ROW EXCLUSIVE MODE;
LOCK TABLE public."HXWKACTIVITYSUBMISSIONS" IN SHARE ROW EXCLUSIVE MODE;

DO $recovery$
DECLARE
    workflow_id uuid := '3a230fe7-8162-35f4-e917-9c5a712f498e'::uuid;
    activity_id uuid := '3a230fe9-580e-b452-e53d-c2fa1ea9c30c'::uuid;
    submission_id uuid := '3a230fea-9fc4-5c8d-d2d0-394efbe94d90'::uuid;
    current_subscription_id uuid;
    persisted_event_id uuid;
    matching_count integer;
    updated_count integer;
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM public."HXWKEXECUTIONPOINTER"
        WHERE "ID" = activity_id
          AND "WKINSTANCEID" = workflow_id
          AND "STEPNAME" = '核定'
          AND "STATUS" = 5
          AND "EVENTNAME" = 'WorkflowCore.Activity'
          AND "EVENTKEY" = activity_id::text
          AND "EVENTPUBLISHED" = FALSE
          AND "ENDTIME" IS NULL
    ) THEN
        RAISE EXCEPTION
            'Recovery cancelled: the target pointer is not the waiting 核定 activity.';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM public."HXWKACTIVITYSUBMISSIONS"
        WHERE "ID" = submission_id
          AND "WORKFLOWID" = workflow_id
          AND "ACTIVITYNAME" = activity_id::text
          AND "STATUS" = 2
    ) THEN
        RAISE EXCEPTION
            'Recovery cancelled: the expected EventPublished submission does not exist.';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM public."HXPOINTER_CANDIDATES"
        WHERE "NODEID" = activity_id
          AND "EXEOPERATETYPE" = 1
          AND "PARENTSTATE" = 6
    ) OR NOT EXISTS (
        SELECT 1
        FROM public."HXPOINTER_CANDIDATES"
        WHERE "NODEID" = activity_id
          AND "EXEOPERATETYPE" = 1
          AND "PARENTSTATE" IN (2, 3, 7)
    ) THEN
        RAISE EXCEPTION
            'Recovery cancelled: completed Host + waiting Host signature was not found.';
    END IF;

    SELECT count(*)
    INTO matching_count
    FROM public."HXWKSUBSCRIPTIONS"
    WHERE "WORKFLOWID" = workflow_id
      AND "EXECUTIONPOINTERID" = activity_id
      AND "EVENTNAME" = 'WorkflowCore.Activity'
      AND "EVENTKEY" = activity_id::text;

    IF matching_count <> 1 THEN
        RAISE EXCEPTION
            'Recovery cancelled: expected one current subscription, found %.',
            matching_count;
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM public."HXWKSUBSCRIPTIONS"
        WHERE "WORKFLOWID" = workflow_id
          AND "EXECUTIONPOINTERID" = activity_id
          AND "EVENTNAME" = 'WorkflowCore.Activity'
          AND "EVENTKEY" = activity_id::text
          AND "SUBSCRIBEASOF" <= now()
    ) THEN
        RAISE EXCEPTION
            'Recovery cancelled: the current subscription is not yet effective.';
    END IF;

    IF EXISTS (
        SELECT 1
        FROM public."HXPOINTER_CANDIDATES"
        WHERE "NODEID" = activity_id
          AND "EXEOPERATETYPE" = 3
          AND "PARENTSTATE" IN (2, 3, 7)
    ) THEN
        RAISE EXCEPTION
            'Recovery cancelled: the pointer has a pending Countersign candidate.';
    END IF;

    SELECT "ID"
    INTO current_subscription_id
    FROM public."HXWKSUBSCRIPTIONS"
    WHERE "WORKFLOWID" = workflow_id
      AND "EXECUTIONPOINTERID" = activity_id
      AND "EVENTNAME" = 'WorkflowCore.Activity'
      AND "EVENTKEY" = activity_id::text;

    SELECT count(*)
    INTO matching_count
    FROM public."HXWKEVENTS"
    WHERE "EVENTNAME" = 'WorkflowCore.Activity'
      AND "EVENTKEY" = activity_id::text;

    IF matching_count <> 1 THEN
        RAISE EXCEPTION
            'Recovery cancelled: expected one persisted activity event, found %.',
            matching_count;
    END IF;

    SELECT "ID"
    INTO persisted_event_id
    FROM public."HXWKEVENTS"
    WHERE "EVENTNAME" = 'WorkflowCore.Activity'
      AND "EVENTKEY" = activity_id::text
      AND "ISPROCESSED" = TRUE;

    IF persisted_event_id IS NULL THEN
        RAISE EXCEPTION
            'Recovery cancelled: the consumed activity event was not found.';
    END IF;

    IF NOT EXISTS (
        SELECT 1
        FROM public."HXWKEVENTS" e
        JOIN public."HXPOINTER_CANDIDATES" c
          ON c."NODEID" = activity_id
         AND c."CANDIDATEID"::text =
             (e."EVENTDATA"::jsonb #>> '{Data,CurrentUserId}')
         AND c."EXEOPERATETYPE" = 1
         AND c."PARENTSTATE" = 6
        WHERE e."ID" = persisted_event_id
          AND e."EVENTDATA"::jsonb ->> '$type'
              LIKE 'WorkflowCore.Models.ActivityResult,%'
          AND e."EVENTDATA"::jsonb ->> 'Status' = '0'
          AND e."EVENTDATA"::jsonb #>> '{Data,DecideBranching}' = '登簿'
          AND e."EVENTDATA"::jsonb #>> '{Data,ExecutionType}' = '1'
    ) THEN
        RAISE EXCEPTION
            'Recovery cancelled: the stored ActivityResult payload is not the expected completed Host decision.';
    END IF;

    UPDATE public."HXWKSUBSCRIPTIONS"
    SET "EXTERNALTOKEN" = NULL,
        "EXTERNALWORKERID" = NULL,
        "EXTERNALTOKENEXPIRY" = NULL
    WHERE "ID" = current_subscription_id;
    GET DIAGNOSTICS updated_count = ROW_COUNT;

    IF updated_count <> 1 THEN
        RAISE EXCEPTION
            'Recovery cancelled: subscription update affected % rows.', updated_count;
    END IF;

    UPDATE public."HXWKEVENTS"
    SET "EVENTDATA" = jsonb_set(
            "EVENTDATA"::jsonb,
            '{SubscriptionId}',
            to_jsonb(current_subscription_id::text),
            FALSE)::text,
        "EVENTTIME" = now(),
        "ISPROCESSED" = FALSE
    WHERE "ID" = persisted_event_id;
    GET DIAGNOSTICS updated_count = ROW_COUNT;

    IF updated_count <> 1 THEN
        RAISE EXCEPTION
            'Recovery cancelled: event replay update affected % rows.', updated_count;
    END IF;

    RAISE NOTICE
        'Recovery prepared: event %, current subscription %.',
        persisted_event_id,
        current_subscription_id;
END
$recovery$;

COMMIT;

SELECT
    p."STEPNAME",
    p."STATUS" AS pointer_status,
    p."EVENTPUBLISHED",
    e."ISPROCESSED",
    s."STATUS" AS submission_status,
    s."ERROR"
FROM public."HXWKEXECUTIONPOINTER" p
JOIN public."HXWKEVENTS" e
  ON e."EVENTNAME" = 'WorkflowCore.Activity'
 AND e."EVENTKEY" = p."ID"::text
JOIN public."HXWKACTIVITYSUBMISSIONS" s
  ON s."WORKFLOWID" = p."WKINSTANCEID"
 AND s."ACTIVITYNAME" = p."ID"::text
WHERE p."ID" = '3a230fe9-580e-b452-e53d-c2fa1ea9c30c'::uuid;

-- Immediately after this script and before the host starts, expected values:
-- pointer_status=5, EVENTPUBLISHED=false, ISPROCESSED=false,
-- submission_status=2. After the host processes the replay, the old pointer
-- should become Complete(3), 登簿 should start, and submission_status should be 3.
