BEGIN;

CREATE TABLE IF NOT EXISTS task_outbox
(
    id uuid PRIMARY KEY,
    occurred_at_utc timestamptz NOT NULL,
    routing_key varchar(100) NOT NULL,
    payload text NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_task_outbox_occurred_at_utc ON task_outbox (occurred_at_utc);

CREATE TABLE IF NOT EXISTS chat_outbox
(
    id uuid PRIMARY KEY,
    occurred_at_utc timestamptz NOT NULL,
    routing_key varchar(100) NOT NULL,
    payload text NOT NULL
);
CREATE INDEX IF NOT EXISTS ix_chat_outbox_occurred_at_utc ON chat_outbox (occurred_at_utc);

COMMIT;