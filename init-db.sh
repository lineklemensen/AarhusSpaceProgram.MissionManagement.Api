#!/bin/bash
sleep 10
/opt/mssql-tools/bin/sqlcmd -S sqlserver -U sa -P "${SA_PASSWORD}" -i /init/create-db-user.sql