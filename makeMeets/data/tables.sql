CREATE TABLE timezones(
	timezone VARCHAR(30) PRIMARY KEY
);

CREATE TABLE hosts(
    id UUID PRIMARY KEY,
    tz VARCHAR(30) REFERENCES timezones(timezone),
    handle text
);

CREATE TABLE meets(
    host UUID REFERENCES hosts(id),
    start BIGINT,
    end BIGINT, 
)
