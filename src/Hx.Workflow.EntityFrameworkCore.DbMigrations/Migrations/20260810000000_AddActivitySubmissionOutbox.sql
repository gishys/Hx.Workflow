-- PostgreSQL
-- Adds the idempotent activity-submission/Outbox table.
-- This script is safe to execute repeatedly.

BEGIN;

CREATE TABLE IF NOT EXISTS "HXWKACTIVITYSUBMISSIONS"
(
    "ID"                   uuid                     NOT NULL,
    "WORKFLOWID"           uuid                     NOT NULL,
    "ACTIVITYNAME"         varchar(64)              NOT NULL,
    "PAYLOAD"              text                     NOT NULL,
    "REQUESTHASH"          varchar(64)              NOT NULL,
    "STATUS"               integer                  NOT NULL,
    "ERROR"                varchar(2000)            NULL,
    "CREATIONTIME"         timestamp with time zone NOT NULL,
    "LASTMODIFICATIONTIME" timestamp with time zone NOT NULL,
    "LOCKEDUNTIL"          timestamp with time zone NULL,
    "ATTEMPTCOUNT"         integer                  NOT NULL,
    "TENANTID"             uuid                     NULL,
    CONSTRAINT "PK_WKACTIVITYSUBMISSIONS" PRIMARY KEY ("ID")
);

CREATE INDEX IF NOT EXISTS "IX_WKACTIVITYSUBMISSIONS_PROCESSABLE"
    ON "HXWKACTIVITYSUBMISSIONS" ("STATUS", "LOCKEDUNTIL");

CREATE UNIQUE INDEX IF NOT EXISTS "UX_WKACTIVITYSUBMISSIONS_WORKFLOW_ACTIVITY"
    ON "HXWKACTIVITYSUBMISSIONS" ("WORKFLOWID", "ACTIVITYNAME");

-- Keep EF Core's migration history consistent when this SQL is applied manually.
-- The application already has this table when earlier EF migrations were applied.
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260810000000_AddActivitySubmissionOutbox', '8.0.4')
ON CONFLICT ("MigrationId") DO NOTHING;

COMMIT;
