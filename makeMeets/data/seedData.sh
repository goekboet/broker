#! /bin/bash

\copy timezones FROM '/home/seed/tz.csv' WITH DELIMITER ',' CSV HEADER;