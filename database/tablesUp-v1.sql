CREATE TABLE hosts(
    sub UUID PRIMARY KEY,
    handle text NOT NULL
);

CREATE INDEX handle_index ON hosts (handle);

CREATE TABLE times(
    host UUID REFERENCES hosts(sub),
    start BIGINT NOT NULL,
    "end" BIGINT NOT NULL,
    record text NOT NULL,
    booked UUID NULL DEFAULT null,
    PRIMARY KEY(host, start)
);

ALTER TABLE times ADD CONSTRAINT one_booking_per_start UNIQUE (start, booked);
