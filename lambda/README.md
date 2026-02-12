# Lambda Event Handler

AWS Lambda handler for the Registration Event Service. Processes SNS events published by the main API and executes asynchronous operations (email, auditing, etc.).

## Structure

```
lambda/
├── EventHandler.csproj           # Lambda project file (targets .NET 8.0)
├── appsettings.json              # Configuration
└── src/
    ├── Events/                   # Domain events (mirrors API events)
    │   ├── UserCreatedEvent.cs
    │   └── ProductCreatedEvent.cs
    ├── Handlers/
    │   └── SnsEventHandler.cs     # Main Lambda entry point
    ├── Services/
    │   ├── EmailService.cs        # Simulates email sending
    │   ├── AuditService.cs        # Audit logging
    │   └── EventProcessingService.cs  # Event routing and orchestration
    └── Models/
        └── SnsMessage.cs          # SNS message models
```

## Key Features

- **Event-Driven**: Reacts to SNS events published by the API
- **Async Processing**: Non-blocking operations for email and auditing
- **Structured Logging**: Serilog integration for CloudWatch
- **Dependency Injection**: Clean, testable service architecture
- **Error Handling**: Comprehensive logging and exception handling

## Event Types Supported

### UserCreatedEvent
- Triggers welcome email
- Logs audit trail
- Records user registration

### ProductCreatedEvent
- Triggers product notification
- Logs audit trail
- Records product registration

## Deployment

```bash
# Build Lambda package
dotnet publish -c Release -o publish

# Deploy to AWS (using SAM or manual upload)
sam deploy
# or
aws lambda update-function-code --function-name user-registration-handler --zip-file fileb://publish.zip
```

## Local Testing

```bash
# Mock SNS event
dotnet test

# Or simulate with SAM
sam local invoke SnsEventHandler -e events/sns-event.json
```

## CloudWatch Logs

All Lambda executions are logged to CloudWatch with structured logging:
- Event processing timestamps
- Audit trail for compliance
- Error details for debugging
