#! /bin/bash

\copy timezones FROM '/home/seed/tz.csv' WITH DELIMITER ',' CSV HEADER;
\copy hosts FROM '/home/seed/hosts.csv' WITH DELIMITER ',' CSV HEADER;
\copy times FROM '/home/seed/times.csv' WITH DELIMITER ',' CSV HEADER;