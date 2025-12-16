#!/bin/bash
set -e

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &

# Wait for SQL Server to start up (give it a few seconds)
echo "Waiting for SQL Server to start..."
sleep 20

# Run the initialization script
# Note: Azure SQL Edge uses the same tools path as standard SQL Server
echo "Running initialization script..."
/opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P "$MSSQL_SA_PASSWORD" \
  -i /usr/src/app/init-db.sql

echo "Initialization complete. SQL Server is ready."

# Keep the container running
wait