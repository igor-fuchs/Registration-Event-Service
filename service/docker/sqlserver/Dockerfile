# =============================================================================
# Custom SQL Server 2022 image for local development
# Pre-creates database, login, schema, table and seed data
# =============================================================================
FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

# Copy initialization assets
COPY init.sql /docker/init.sql
COPY entrypoint.sh /docker/entrypoint.sh

# Fix Windows CRLF line endings and execute permissions for the entrypoint script
RUN sed -i 's/\r$//' /docker/entrypoint.sh
RUN chmod +x /docker/entrypoint.sh

USER mssql

# Use custom entrypoint that runs SQL Server + init script
ENTRYPOINT ["/docker/entrypoint.sh"]
