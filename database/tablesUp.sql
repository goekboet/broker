CREATE TABLE hosts(
    sub UUID NOT NULL,
    handle text NOT NULL,
    name text NOT NULL,
    PRIMARY KEY(sub),
    CONSTRAINT unique_handle UNIQUE(handle)
);

CREATE INDEX name_prefix_search ON hosts (name text_pattern_ops);

CREATE TABLE times(
    start BIGINT NOT NULL,
    "end" BIGINT NOT NULL,
    host text REFERENCES hosts(handle),
    booked UUID NULL DEFAULT null,
    PRIMARY KEY(host, start),
    record text
);

CREATE INDEX host_times ON times(host);
CREATE INDEX booked_times ON times(booked);

ALTER TABLE times ADD CONSTRAINT one_booking_per_start UNIQUE (start, booked);


