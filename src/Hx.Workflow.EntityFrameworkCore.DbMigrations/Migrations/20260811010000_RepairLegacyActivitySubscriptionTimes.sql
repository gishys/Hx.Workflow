-- PostgreSQL operational recovery script.
-- Safe to run repeatedly while the workflow service is stopped.
-- Repairs subscriptions created before SubscribeAsOf persistence was fixed.

BEGIN;

UPDATE "HXWKSUBSCRIPTIONS" subscription
SET "SUBSCRIBEASOF" = workflow_event."EVENTTIME"
FROM "HXWKEVENTS" workflow_event
WHERE subscription."SUBSCRIBEASOF" = '-infinity'::timestamptz
  AND workflow_event."EVENTNAME" = subscription."EVENTNAME"
  AND workflow_event."EVENTKEY" = subscription."EVENTKEY"
  AND workflow_event."EVENTDATA"::jsonb ->> 'SubscriptionId' = subscription."ID"::text;

COMMIT;
