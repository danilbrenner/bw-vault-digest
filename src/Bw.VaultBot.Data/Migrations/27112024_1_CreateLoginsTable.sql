CREATE TABLE sync_events
(
    sync_id   UNIQUEIDENTIFIER NOT NULL,
    email     TEXT             NOT NULL,
    timestamp DATETIME         NOT NULL,
    CONSTRAINT PK_sync_events PRIMARY KEY (sync_id)
);

CREATE INDEX ix_sync_events_timestamp ON sync_events (timestamp ASC);

CREATE TABLE logins
(
    login_id UNIQUEIDENTIFIER NOT NULL,
    sync_id  UNIQUEIDENTIFIER NOT NULL REFERENCES sync_events (sync_id),
    name     TEXT             NOT NULL,
    strength INTEGER          NOT NULL,
    age      INTEGER          NOT NULL,
    CONSTRAINT PK_logins PRIMARY KEY (login_id, sync_id)
);
