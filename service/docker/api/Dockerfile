# =============================================================================
# Development Dockerfile for User Registration Event Service
# Uses dotnet watch for hot reload during development
# =============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0

WORKDIR /src

# Copy solution and project files first (for layer caching on restore)
COPY Directory.Build.props ./
COPY Directory.Packages.props ./
COPY RegistrationEventService.slnx ./
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Presentation/Presentation.csproj src/Presentation/
COPY tests/UnitTests.csproj tests/

# Restore dependencies
RUN dotnet restore RegistrationEventService.slnx

# Copy full source code
COPY src/ src/

# Expose port
EXPOSE 8080

# Environment
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Entry point with hot reload
ENTRYPOINT ["dotnet", "watch", "run", "--project", "src/Presentation/Presentation.csproj", "--no-launch-profile"]
