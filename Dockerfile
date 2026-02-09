# Multi-stage Dockerfile for User Registration Event Service
# Optimized for production deployments with minimal image size

# =============================================================================
# Stage 1: Build
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (for layer caching)
COPY Directory.Build.Props ./
COPY Directory.Packages.Props ./
COPY RegistrationEventService.slnx ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Presentation/Presentation.csproj src/Presentation/

# Restore dependencies
RUN dotnet restore RegistrationEventService.slnx

# Copy source code
COPY src/ src/

# Build and publish
RUN dotnet publish src/Presentation/Presentation.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =============================================================================
# Stage 2: Runtime
# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system appgroup && adduser --system appuser --ingroup appgroup

# Copy published application
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/api/users || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Presentation.dll"]
