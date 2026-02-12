# Registration Event Service - Complete Project Structure

A modern, event-driven AWS Lambda & ASP.NET Core integration for user/product registration with asynchronous event processing.

## 📁 Project Structure

```
Registration-Event-Service/
├── api/                          # Main API application (Clean Architecture)
│   ├── src/
│   │   ├── Domain/              # Core domain entities and events
│   │   ├── Application/         # Business logic and services
│   │   ├── Infrastructure/      # Data access, AWS SNS integration
│   │   └── Presentation/        # ASP.NET Core API endpoints
│   ├── docker/                  # Docker build files
│   ├── docker-compose.yml       # API and SQL Server orchestration
│   └── README.md               # API-specific documentation
│
├── lambda/                       # AWS Lambda handler (event processor)
│   ├── src/
│   │   ├── Events/             # Event models
│   │   ├── Handlers/           # SNS event handler (entry point)
│   │   ├── Services/           # Business logic (email, audit, etc.)
│   │   └── Models/             # Data models
│   ├── events/                 # Example SNS events for testing
│   ├── EventHandler.csproj     # Lambda project file
│   ├── appsettings.json        # Configuration
│   └── README.md               # Lambda-specific documentation
│
├── artifacts/                   # Build outputs (git ignored)
├── Directory.Build.props        # Centralized build properties
├── Directory.Packages.props     # Centralized NuGet versions
├── RegistrationEventService.slnx # Solution file (both API and Lambda)
├── PROJECT_IDEA.md              # Original project concept
└── README.md                    # This file
```

## 🎯 What This Project Does

1. **API** (`/api`): Registers users/products and publishes events to AWS SNS
2. **Lambda** (`/lambda`): Receives events and executes asynchronous operations:
   - Sends welcome/notification emails (simulated)
   - Logs audit trails
   - Can be extended for notifications, analytics, integrations

## 🔄 Event Flow

```
Client → API → SQL Server → AWS SNS → Lambda → Email/Audit/Logging
```

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose (for local database)
- AWS Account (for SNS/Lambda deployment)

### Running the API

```bash
cd api
docker-compose up -d          # Start SQL Server
dotnet run                     # Start API on http://localhost:5000
```

Access Swagger UI: http://localhost:5000

### Deploying the Lambda

```bash
cd lambda
dotnet publish -c Release -o publish
# Deploy to AWS (using AWS CLI, SAM, or AWS Console)
```

### Testing

```bash
# API unit tests
cd api
dotnet test

# Lambda local testing (with SAM)
cd lambda
sam local invoke SnsEventHandler -e events/user-created-event.json
```

## 📚 Key Technologies

### API
- **Framework**: ASP.NET Core 8
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, Presentation)
- **Database**: SQL Server + Entity Framework Core
- **Messaging**: AWS SNS
- **Logging**: Serilog

### Lambda
- **Runtime**: .NET 8.0 on AWS Lambda
- **Messaging**: AWS SNS
- **Logging**: Serilog → CloudWatch
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection

## 🏗️ Architecture Highlights

### API Layer
- **Entities**: `User`, `Product` (domain models)
- **Services**: Business logic (UserService, ProductService)
- **Repositories**: Data access abstraction
- **DTOs**: Request/Response models for API
- **Events**: Domain events (UserCreatedEvent, ProductCreatedEvent)

### Lambda Layer
- **Handler**: SNS event entry point
- **Event Processing**: Routes events to handlers
- **Services**: Email, Audit, Processing logic
- **Models**: SNS message deserialization

## 📖 Documentation

- [API README](api/README.md) - API-specific setup and architecture
- [Lambda README](lambda/README.md) - Lambda-specific setup and deployment
- [Project Idea](PROJECT_IDEA.md) - Original concept and design rationale
- [Docker Setup](DOCKER_SETUP.md) - Container orchestration details

## 🔐 Environment Variables

### API (.env or appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RegistrationEventDb;User Id=sa;Password=YourPassword123;"
  },
  "Aws": {
    "SnS": {
      "TopicArn": "arn:aws:sns:us-east-1:123456789012:user-registration-events"
    }
  }
}
```

### Lambda (Environment Variables)
```
AWS_LAMBDA_FUNCTION_NAME=user-registration-handler
AWS_LAMBDA_FUNCTION_TIMEOUT=30
AWS_LOG_LEVEL=Information
```

## 🧪 Example API Requests

### Create User
```bash
curl -X POST http://localhost:5000/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"John Doe","email":"john@example.com"}'
```

### Create Product
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name":"Laptop Pro",
    "sku":"SKU-001",
    "supplier":"TechCorp",
    "price":1299.99,
    "description":"High-end laptop"
  }'
```

## 🔄 CI/CD

GitHub Actions workflows (coming soon):
- API: Build, Test, Docker push
- Lambda: Build, Package, Deploy to AWS

## 📝 Development Workflow

1. Make changes in `/api` or `/lambda`
2. Run locally and test
3. Commit with clear messages
4. CI/CD automatically builds and deploys
5. Monitor Lambda execution in CloudWatch

## 🤝 Contributing

1. Create a feature branch
2. Make changes following Clean Architecture principles
3. Add tests
4. Submit pull request

## 📄 License

[Your License Here]

## 🎓 Learning Resources

This project is an excellent learning example for:
- Clean Architecture in .NET
- Event-Driven Architecture
- AWS SNS/Lambda integration
- Async/Await patterns
- Dependency Injection
- Docker containerization
- Unit testing best practices

---

**Last Updated**: February 2026