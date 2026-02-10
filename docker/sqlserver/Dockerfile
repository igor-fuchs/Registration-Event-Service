# =============================================================================
# Custom SQL Server 2022 image for local development
# Pre-creates database, login, schema, table and seed data
# =============================================================================
FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

# Copy initialization assets
COPY init.sql /docker-entrypoint-initdb.d/init.sql
COPY entrypoint.sh /docker-entrypoint-initdb.d/entrypoint.sh

# Make the entrypoint executable
RUN chmod +x /docker-entrypoint-initdb.d/entrypoint.sh

USER mssql

# Use custom entrypoint that runs SQL Server + init script
ENTRYPOINT ["/docker-entrypoint-initdb.d/entrypoint.sh"]
