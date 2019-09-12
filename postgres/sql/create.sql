CREATE TABLE timezones(
	timezone VARCHAR(30) PRIMARY KEY
);

CREATE TABLE hosts(
    id UUID PRIMARY KEY,
    tz VARCHAR(30) REFERENCES timezones(timezone),
    handle text
);

CREATE TABLE times(
    host UUID REFERENCES hosts(id),
    start BIGINT NOT NULL,
    "end" BIGINT NOT NULL,
    record text NOT NULL,
    booked UUID NULL DEFAULT null,
    PRIMARY KEY(host, start)
);

CREATE INDEX start_index ON times (start);

create user broker with password 'trtLAqkGY3nE3DyA';
GRANT CONNECT ON DATABASE meets to broker;