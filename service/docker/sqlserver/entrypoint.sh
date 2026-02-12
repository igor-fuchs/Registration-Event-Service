#!/bin/bash
# =============================================================================
# Custom entrypoint: starts SQL Server and runs the init script once ready
# =============================================================================
set -e

# Start SQL Server in the background
/opt/mssql/bin/sqlservr &
MSSQL_PID=$!

# Wait for SQL Server to be ready (up to 60 seconds)
echo "Waiting for SQL Server to start..."
for i in {1..60}; do
    if /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -Q "SELECT 1" &>/dev/null; then
        echo "✔ SQL Server is ready."
        break
    fi
    sleep 1
done

# Run the initialization script
echo "Running initialization script..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -C -d master -i /docker/init.sql

echo "Initialization complete. SQL Server is running."

# Bring SQL Server back to the foreground
wait $MSSQL_PID
