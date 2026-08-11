-- PostgreSQL operational recovery script.
-- Run while the workflow service is stopped, before deploying the corresponding code fix.
-- It restores activity payload values that were persisted as JsonElement/ValueKind wrappers.

BEGIN;

WITH repair_source AS
(
    SELECT
        e."ID" AS event_id,
        e."EVENTDATA"::jsonb ->> 'SubscriptionId' AS subscription_id,
        submission."PAYLOAD"::jsonb AS payload
    FROM "HXWKEVENTS" e
    INNER JOIN "HXWKACTIVITYSUBMISSIONS" submission
        ON submission."ACTIVITYNAME" = e."EVENTKEY"
    WHERE e."EVENTNAME" = 'WorkflowCore.Activity'
      AND e."ISPROCESSED" = FALSE
      AND e."EVENTDATA" LIKE '%System.Text.Json.JsonElement%'
)
UPDATE "HXWKEVENTS" target_event
SET "EVENTDATA" = jsonb_build_object(
        '$type', 'WorkflowCore.Models.ActivityResult, WorkflowCore',
        'Status', 0,
        'SubscriptionId', source.subscription_id,
        'Data', jsonb_build_object(
            '$type', 'System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[System.Object, System.Private.CoreLib]], System.Private.CoreLib'
        ) || source.payload
    )::text
FROM repair_source source
WHERE target_event."ID" = source.event_id
  AND source.subscription_id IS NOT NULL;

COMMIT;
