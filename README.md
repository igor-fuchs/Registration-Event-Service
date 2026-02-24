# User Registration Event Service — Documentação Técnica

## Sumário

- [1. Visão Geral do Projeto](#1-visão-geral-do-projeto)
- [2. Stack Tecnológica](#2-stack-tecnológica)
- [3. Arquitetura do Sistema](#3-arquitetura-do-sistema)
- [4. Documentação da API](#4-documentação-da-api)
- [5. Estrutura do Banco de Dados](#5-estrutura-do-banco-de-dados)
- [6. Autenticação e Autorização](#6-autenticação-e-autorização)
- [7. Configuração de Ambiente](#7-configuração-de-ambiente)
- [8. Executando o Projeto](#8-executando-o-projeto)
- [9. Deploy](#9-deploy)
- [10. Tratamento de Erros e Logging](#10-tratamento-de-erros-e-logging)
- [11. Estratégia de Testes](#11-estratégia-de-testes)

---

## 1. Visão Geral do Projeto

### Propósito

O **User Registration Event Service** é uma API REST orientada a eventos (*event-driven*) construída com ASP.NET Core. O sistema resolve o problema de executar ações secundárias (notificação, auditoria, integração) após o cadastro de usuários e produtos — sem travar a API e sem acoplamento forte.

Em vez de a API executar todas as responsabilidades diretamente, ela **publica um evento** no AWS SNS. Consumidores independentes (AWS Lambda) reagem a esses eventos de forma assíncrona.

### Funcionalidades Principais

| Funcionalidade | Descrição |
|---|---|
| Cadastro de Usuários | Criação de usuários com validação de e-mail único |
| Cadastro de Produtos | Criação de produtos com validação de SKU único |
| Consulta de Registros | Recuperação individual ou listagem completa de usuários e produtos |
| Publicação de Eventos | Emissão automática de eventos `UserCreatedEvent` e `ProductCreatedEvent` para AWS SNS |
| Processamento Assíncrono | Lambda consome eventos para auditoria, simulação de e-mail e logging |

### Resumo da Arquitetura

```
Cliente (Postman/Swagger)
    │
    ▼
ASP.NET Core Web API  ──►  SQL Server (persistência)
    │
    ▼
AWS SNS (Event Broker)
    │
    ▼
AWS Lambda (.NET)  ──►  CloudWatch Logs
    ├── Simulação de e-mail
    └── Auditoria
```

---

## 2. Stack Tecnológica

### Linguagem de Programação

| Item | Versão |
|---|---|
| C# | 13 |
| .NET | 10.0 (API) / 8.0 (Lambda) |

### Frameworks e Bibliotecas

| Componente | Tecnologia | Versão |
|---|---|---|
| Web Framework | ASP.NET Core | 10.0 |
| ORM | Entity Framework Core | 9.0.1 |
| Documentação API | Swashbuckle (Swagger) | 7.2.0 |
| AWS SDK | AWSSDK.SimpleNotificationService | 3.7.400.63 |
| Lambda Runtime | Amazon.Lambda.Core | 2.2.0 |
| Lambda SNS Events | Amazon.Lambda.SNSEvents | 2.1.1 |
| Serialização Lambda | Amazon.Lambda.Serialization.SystemTextJson | 2.4.0 |
| Logging (Lambda) | Serilog | 3.1.1 |
| Testes | xUnit | 2.9.3 |
| Mocking | Moq | 4.20.72 |
| Assertions | FluentAssertions | 7.0.0 |
| Code Coverage | Coverlet | 6.0.4 |

### Banco de Dados

| Item | Detalhes |
|---|---|
| SGBD | SQL Server 2022 (Developer Edition) |
| Driver | Microsoft.EntityFrameworkCore.SqlServer |
| Ambiente local | Docker container com script de inicialização |

### Infraestrutura

| Componente | Tecnologia |
|---|---|
| Containerização | Docker / Docker Compose |
| Event Broker | AWS SNS (Simple Notification Service) |
| Computação Serverless | AWS Lambda |
| Observabilidade | AWS CloudWatch Logs |

### Serviços Externos

| Serviço | Finalidade |
|---|---|
| AWS SNS | Broker de eventos — distribui mensagens para assinantes |
| AWS Lambda | Consumidor de eventos — executa lógica de reação assíncrona |
| AWS CloudWatch | Logs de execução e monitoramento da Lambda |

---

## 3. Arquitetura do Sistema

### Visão de Alto Nível

O projeto segue os princípios de **Clean Architecture**, separando responsabilidades em camadas concêntricas com dependências apontando sempre para o centro (Domain).

```
┌──────────────────────────────────────────────────┐
│                  Presentation                     │
│         (Controllers, Middleware, Swagger)         │
├──────────────────────────────────────────────────┤
│                  Infrastructure                   │
│    (EF Core, Repositories, AWS SNS Publisher)     │
├──────────────────────────────────────────────────┤
│                   Application                     │
│          (Services, DTOs, Abstractions)           │
├──────────────────────────────────────────────────┤
│                     Domain                        │
│     (Entities, Events, Exceptions, Contracts)     │
└──────────────────────────────────────────────────┘
```

### Componentes e Responsabilidades

#### Domain (`RegistrationEventService.Domain`)

Camada central sem dependências externas. Contém:

| Componente | Responsabilidade |
|---|---|
| `Entities/User.cs` | Entidade de domínio `User` — factory method `Create()` com timestamp UTC |
| `Entities/Product.cs` | Entidade de domínio `Product` — factory method `Create()` com validação de descrição nula |
| `Events/IDomainEvent.cs` | Interface marcadora para eventos de domínio |
| `Events/UserCreatedEvent.cs` | Evento emitido quando um usuário é criado |
| `Events/ProductCreatedEvent.cs` | Evento emitido quando um produto é criado |
| `Exceptions/DomainException.cs` | Exceção base para todas as exceções de domínio |
| `Exceptions/UserAlreadyExistsException.cs` | E-mail duplicado |
| `Exceptions/UserNotFoundException.cs` | Usuário não encontrado |
| `Exceptions/ProductAlreadyExistsException.cs` | SKU duplicado |
| `Exceptions/ProductNotFoundException.cs` | Produto não encontrado |
| `Abstractions/IUserRepository.cs` | Contrato de persistência para `User` |
| `Abstractions/IProductRepository.cs` | Contrato de persistência para `Product` |
| `Abstractions/IUnitOfWork.cs` | Contrato de unidade de trabalho (transações atômicas) |
| `Abstractions/IEventPublisher.cs` | Contrato para publicação de eventos em broker externo |

#### Application (`RegistrationEventService.Application`)

Orquestra a lógica de aplicação. Depende apenas da camada Domain.

| Componente | Responsabilidade |
|---|---|
| `Services/UserService.cs` | Coordena criação de usuário, persistência e publicação de evento |
| `Services/ProductService.cs` | Coordena criação de produto, persistência e publicação de evento |
| `DTOs/CreateUserRequest.cs` | DTO de entrada com validações (DataAnnotations) |
| `DTOs/CreateProductRequest.cs` | DTO de entrada com validações (DataAnnotations) |
| `DTOs/UserResponse.cs` | DTO de saída para dados de usuário |
| `DTOs/ProductResponse.cs` | DTO de saída para dados de produto |
| `Abstractions/IUserService.cs` | Contrato do serviço de usuário |
| `Abstractions/IProductService.cs` | Contrato do serviço de produto |
| `DependencyInjection.cs` | Extensão para registrar serviços da camada Application no DI |

#### Infrastructure (`RegistrationEventService.Infrastructure`)

Implementa contratos definidos nas camadas internas. Contém detalhes de tecnologia.

| Componente | Responsabilidade |
|---|---|
| `Persistence/ApplicationDbContext.cs` | DbContext do EF Core — implementa `IUnitOfWork` |
| `Persistence/Configurations/UserConfiguration.cs` | Fluent API — schema `auth`, tabela `Users` |
| `Persistence/Configurations/ProductConfiguration.cs` | Fluent API — schema `catalog`, tabela `Products` |
| `Persistence/Repositories/UserRepository.cs` | Implementação EF Core de `IUserRepository` |
| `Persistence/Repositories/ProductRepository.cs` | Implementação EF Core de `IProductRepository` |
| `Messaging/SnsEventPublisher.cs` | Publica eventos serializados em JSON no AWS SNS |
| `Messaging/SnsOptions.cs` | Configuração do tópico SNS (TopicArn, Region) |
| `DependencyInjection.cs` | Extensão para registrar infraestrutura no DI (EF Core, repositórios, SNS) |

#### Presentation (`RegistrationEventService.Presentation`)

Ponto de entrada HTTP. Não contém lógica de negócio.

| Componente | Responsabilidade |
|---|---|
| `Program.cs` | Configuração do host, DI, Swagger e pipeline HTTP |
| `Controllers/UsersController.cs` | Endpoints REST para usuários |
| `Controllers/ProductsController.cs` | Endpoints REST para produtos |
| `Middleware/ExceptionHandlingMiddleware.cs` | Tratamento global de exceções → `ProblemDetails` (RFC 7807) |

#### Lambda (`EventHandler`)

Projeto separado que consome eventos do SNS.

| Componente | Responsabilidade |
|---|---|
| `Handlers/SnsEventHandler.cs` | Entry point da Lambda — processa registros SNS |
| `Services/EventProcessingService.cs` | Roteia eventos por tipo e orquestra handlers |
| `Services/EmailService.cs` | Simula envio de e-mail (welcome e notificação de produto) |
| `Services/AuditService.cs` | Registra trail de auditoria via logging estruturado |
| `Events/UserCreatedEvent.cs` | Contrato do evento `UserCreated` (espelho do domínio) |
| `Events/ProductCreatedEvent.cs` | Contrato do evento `ProductCreated` (espelho do domínio) |

### Fluxo de Dados

```
1. Cliente envia POST /api/users (ou /api/products)
              │
2. Controller recebe request e delega ao Service
              │
3. Service valida duplicidade (e-mail ou SKU)
              │
4. Service cria entidade via factory method (User.Create / Product.Create)
              │
5. Repository.AddAsync() → EF Core tracking
              │
6. UnitOfWork.SaveChangesAsync() → SQL Server (INSERT)
              │
7. EventPublisher.PublishAsync() → AWS SNS (JSON + MessageAttribute eventType)
              │
8. SNS distribui mensagem para assinantes
              │
9. Lambda.FunctionHandler() recebe SNSEvent
              │
10. EventProcessingService roteia por eventType:
     ├── UserCreatedEvent → EmailService + AuditService
     └── ProductCreatedEvent → EmailService + AuditService
              │
11. Logs gravados no CloudWatch
```

---

## 4. Documentação da API

A API é acessível via Swagger UI em ambiente de desenvolvimento na raiz (`/`).

### Usuários

#### Criar Usuário

| | |
|---|---|
| **Rota** | `POST /api/users` |
| **Descrição** | Cria um novo usuário e publica evento `UserCreatedEvent` no AWS SNS |
| **Autenticação** | Nenhuma (ver [seção 6](#6-autenticação-e-autorização)) |

**Request Body:**

```json
{
  "name": "Igor Fuchs",
  "email": "igor@email.com"
}
```

**Validações:**

| Campo | Regra |
|---|---|
| `name` | Obrigatório, 2–100 caracteres |
| `email` | Obrigatório, formato válido, máximo 255 caracteres |

**Response `201 Created`:**

```json
{
  "id": 1,
  "name": "Igor Fuchs",
  "email": "igor@email.com",
  "createdAt": "2026-02-24T15:30:00Z"
}
```

**Headers de resposta:** `Location: /api/users/1`

**Status Codes:**

| Código | Descrição |
|---|---|
| `201 Created` | Usuário criado com sucesso |
| `400 Bad Request` | Erro de validação nos dados enviados |
| `409 Conflict` | E-mail já cadastrado |
| `500 Internal Server Error` | Erro inesperado no servidor |

---

#### Buscar Usuário por ID

| | |
|---|---|
| **Rota** | `GET /api/users/{id}` |
| **Descrição** | Retorna um usuário pelo identificador |
| **Autenticação** | Nenhuma |

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `int` | ID do usuário |

**Response `200 OK`:**

```json
{
  "id": 1,
  "name": "Igor Fuchs",
  "email": "igor@email.com",
  "createdAt": "2026-02-24T15:30:00Z"
}
```

**Status Codes:**

| Código | Descrição |
|---|---|
| `200 OK` | Usuário encontrado |
| `404 Not Found` | Usuário não encontrado |

---

#### Listar Todos os Usuários

| | |
|---|---|
| **Rota** | `GET /api/users` |
| **Descrição** | Retorna a lista de todos os usuários cadastrados |
| **Autenticação** | Nenhuma |

**Response `200 OK`:**

```json
[
  {
    "id": 1,
    "name": "Alice Johnson",
    "email": "alice@example.com",
    "createdAt": "2026-02-24T10:00:00Z"
  },
  {
    "id": 2,
    "name": "Bob Smith",
    "email": "bob@example.com",
    "createdAt": "2026-02-24T10:05:00Z"
  }
]
```

**Status Codes:**

| Código | Descrição |
|---|---|
| `200 OK` | Lista retornada (pode ser vazia) |

---

### Produtos

#### Criar Produto

| | |
|---|---|
| **Rota** | `POST /api/products` |
| **Descrição** | Cria um novo produto e publica evento `ProductCreatedEvent` no AWS SNS |
| **Autenticação** | Nenhuma |

**Request Body:**

```json
{
  "name": "Wireless Mouse",
  "sku": "SKU-0001",
  "supplier": "Northwind Supplies",
  "price": 49.90,
  "description": "Ergonomic wireless mouse"
}
```

**Validações:**

| Campo | Regra |
|---|---|
| `name` | Obrigatório, 2–150 caracteres |
| `sku` | Obrigatório, 3–64 caracteres |
| `supplier` | Obrigatório, 2–150 caracteres |
| `price` | Obrigatório, entre 0.01 e 9.999.999 |
| `description` | Opcional, máximo 1000 caracteres |

**Response `201 Created`:**

```json
{
  "id": 1,
  "name": "Wireless Mouse",
  "sku": "SKU-0001",
  "supplier": "Northwind Supplies",
  "price": 49.90,
  "description": "Ergonomic wireless mouse",
  "createdAt": "2026-02-24T15:30:00Z"
}
```

**Headers de resposta:** `Location: /api/products/1`

**Status Codes:**

| Código | Descrição |
|---|---|
| `201 Created` | Produto criado com sucesso |
| `400 Bad Request` | Erro de validação nos dados enviados |
| `409 Conflict` | SKU já cadastrado |
| `500 Internal Server Error` | Erro inesperado no servidor |

---

#### Buscar Produto por ID

| | |
|---|---|
| **Rota** | `GET /api/products/{id}` |
| **Descrição** | Retorna um produto pelo identificador |
| **Autenticação** | Nenhuma |

**Parâmetros de rota:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `id` | `int` | ID do produto |

**Response `200 OK`:**

```json
{
  "id": 1,
  "name": "Wireless Mouse",
  "sku": "SKU-0001",
  "supplier": "Northwind Supplies",
  "price": 49.90,
  "description": "Ergonomic wireless mouse",
  "createdAt": "2026-02-24T15:30:00Z"
}
```

**Status Codes:**

| Código | Descrição |
|---|---|
| `200 OK` | Produto encontrado |
| `404 Not Found` | Produto não encontrado |

---

#### Listar Todos os Produtos

| | |
|---|---|
| **Rota** | `GET /api/products` |
| **Descrição** | Retorna a lista de todos os produtos cadastrados |
| **Autenticação** | Nenhuma |

**Response `200 OK`:**

```json
[
  {
    "id": 1,
    "name": "Wireless Mouse",
    "sku": "SKU-0001",
    "supplier": "Northwind Supplies",
    "price": 49.90,
    "description": "Ergonomic wireless mouse",
    "createdAt": "2026-02-24T15:30:00Z"
  }
]
```

**Status Codes:**

| Código | Descrição |
|---|---|
| `200 OK` | Lista retornada (pode ser vazia) |

---

### Formato de Erros

Todas as respostas de erro seguem o padrão **RFC 7807 (Problem Details)**:

```json
{
  "status": 409,
  "title": "Conflict",
  "detail": "A user with the email 'igor@email.com' already exists.",
  "instance": "/api/users"
}
```

---

### Eventos SNS

Os eventos publicados no AWS SNS seguem estes contratos:

#### UserCreatedEvent

```json
{
  "userId": 1,
  "email": "igor@email.com",
  "createdAt": "2026-02-24T15:30:00Z"
}
```

**Message Attribute:** `eventType = "UserCreatedEvent"`

#### ProductCreatedEvent

```json
{
  "productId": 1,
  "name": "Wireless Mouse",
  "sku": "SKU-0001",
  "supplier": "Northwind Supplies",
  "price": 49.90,
  "createdAt": "2026-02-24T15:30:00Z"
}
```

**Message Attribute:** `eventType = "ProductCreatedEvent"`

---

## 5. Estrutura do Banco de Dados

### Entidades Principais

O banco utiliza schemas separados para organização lógica:

#### Tabela `auth.Users`

| Coluna | Tipo | Constraints | Descrição |
|---|---|---|---|
| `Id` | `INT IDENTITY(1,1)` | `PK` | Identificador único auto-incremento |
| `Name` | `NVARCHAR(100)` | `NOT NULL` | Nome completo do usuário |
| `Email` | `NVARCHAR(255)` | `NOT NULL`, `UNIQUE` | E-mail (chave de unicidade) |
| `CreatedAt` | `DATETIME2` | `NOT NULL` | Data/hora de criação (UTC) |

#### Tabela `catalog.Products`

| Coluna | Tipo | Constraints | Descrição |
|---|---|---|---|
| `Id` | `INT IDENTITY(1,1)` | `PK` | Identificador único auto-incremento |
| `Name` | `NVARCHAR(150)` | `NOT NULL` | Nome do produto |
| `Sku` | `NVARCHAR(64)` | `NOT NULL`, `UNIQUE` | Stock Keeping Unit (chave de unicidade) |
| `Supplier` | `NVARCHAR(150)` | `NOT NULL` | Fornecedor do produto |
| `Price` | `DECIMAL(18,2)` | `NOT NULL` | Preço unitário |
| `Description` | `NVARCHAR(1000)` | `NULL` | Descrição opcional |
| `CreatedAt` | `DATETIME2` | `NOT NULL` | Data/hora de criação (UTC) |

### Relacionamentos

Atualmente as entidades **não possuem relacionamentos entre si**. São agregados independentes, adequados ao escopo didático do projeto.

### Índices

| Tabela | Índice | Tipo | Coluna |
|---|---|---|---|
| `auth.Users` | `PK_Users` | Clustered | `Id` |
| `auth.Users` | `UQ_Users_Email` | Unique | `Email` |
| `catalog.Products` | `PK_Products` | Clustered | `Id` |
| `catalog.Products` | `UQ_Products_Sku` | Unique | `Sku` |

### Dados de Seed

O script `init.sql` cria automaticamente dados de amostra:

**Usuários:**
- Alice Johnson (`alice@example.com`)
- Bob Smith (`bob@example.com`)

**Produtos:**
- Wireless Mouse (`SKU-0001`, R$ 49,90)
- USB-C Hub (`SKU-0002`, R$ 129,00)

---

## 6. Autenticação e Autorização

### Estado Atual

O projeto **não implementa autenticação ou autorização** na API. Todos os endpoints são públicos e acessíveis sem credenciais.

Isso é intencional — o foco do projeto é demonstrar padrões de **arquitetura orientada a eventos** e **Clean Architecture**, não segurança.

### Autenticação AWS

Para publicação de eventos no SNS, a API utiliza as credenciais da AWS configuradas no ambiente:

- **Desenvolvimento local (Docker):** as credenciais são montadas do host via volume read-only (`~/.aws:/root/.aws:ro`)
- **Produção:** recomenda-se o uso de IAM Roles (EC2 Instance Profile ou ECS Task Role)

### Sugestão para Evolução

Para adicionar autenticação em uma evolução futura:

```
1. JWT Bearer Authentication (via AWS Cognito ou Identity Server)
2. Middleware de autorização no pipeline HTTP
3. Políticas de autorização por recurso
4. Claims-based access control
```

---

## 7. Configuração de Ambiente

### Variáveis de Ambiente

| Variável | Descrição | Exemplo |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação | `Development` |
| `ASPNETCORE_URLS` | URLs de binding | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | String de conexão do SQL Server | `Server=sqlserver,1433;Database=RegistrationEventService;...` |
| `Aws__Sns__TopicArn` | ARN do tópico SNS | `arn:aws:sns:us-east-1:123456789012:api-data` |
| `Aws__Sns__Region` | Região AWS do tópico | `us-east-1` |
| `SA_PASSWORD` | Senha do SA do SQL Server (Docker) | `Password_123` |
| `ACCEPT_EULA` | Aceite do EULA do SQL Server | `Y` |
| `DOTNET_USE_POLLING_FILE_WATCHER` | Habilita hot reload no Docker | `true` |

### Arquivos de Configuração

| Arquivo | Propósito |
|---|---|
| `appsettings.json` | Configuração base (produção) |
| `appsettings.Development.json` | Overrides para desenvolvimento (log verboso, TopicArn real) |

### Gestão de Secrets

| Ambiente | Estratégia |
|---|---|
| Desenvolvimento local | Credenciais no `appsettings.json` e volume Docker (`~/.aws`) |
| Produção | AWS Secrets Manager ou Systems Manager Parameter Store + IAM Roles |

> **Importante:** Nunca comite credenciais reais da AWS no repositório. Utilize `aws configure` localmente e IAM Roles em produção.

---

## 8. Executando o Projeto

### Pré-requisitos

| Ferramenta | Versão Mínima |
|---|---|
| Docker Desktop | 4.x |
| Docker Compose | v2 |
| .NET SDK | 10.0 (API) / 8.0 (Lambda) |
| AWS CLI | 2.x (para credenciais locais) |
| Conta AWS | Com permissão para SNS |

### Instalação

```bash
# 1. Clone o repositório
git clone https://github.com/seu-usuario/Registration-Event-Service.git
cd Registration-Event-Service/service

# 2. Configure credenciais AWS locais (se ainda não configuradas)
aws configure
# AWS Access Key ID: <sua-key>
# AWS Secret Access Key: <seu-secret>
# Default region name: us-east-1
```

### Execução Local (Docker)

```bash
# Subir toda a infraestrutura (SQL Server + API com hot reload)
docker compose up --build

# A API estará disponível em:
# http://localhost:5000          (Swagger UI)
# http://localhost:5000/api/users
# http://localhost:5000/api/products

# O SQL Server estará disponível em:
# localhost:1433
```

O Docker Compose executa automaticamente:

1. **SQL Server** — builda a imagem customizada, executa `init.sql` (cria banco, schemas, tabelas e seed data)
2. **API** — restaura pacotes, inicia com `dotnet watch run` (hot reload habilitado)

### Execução sem Docker

```bash
# 1. Inicie um SQL Server local ou ajuste a connection string

# 2. Restaure dependências
dotnet restore RegistrationEventService.slnx

# 3. Execute a API
dotnet run --project src/Presentation/Presentation.csproj
```

### Executando os Testes

```bash
# Rodar todos os testes unitários
dotnet test

# Com cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Com output detalhado
dotnet test --verbosity detailed
```

### Parando a Infraestrutura

```bash
# Parar containers
docker compose down

# Parar e remover volumes (limpa dados do banco)
docker compose down -v
```

---

## 9. Deploy

### Estratégia de Deploy

O projeto é composto por dois artefatos de deploy independentes:

| Componente | Plataforma | Estratégia |
|---|---|---|
| API (service) | AWS ECS / EC2 / App Service | Container Docker |
| Lambda (lambda) | AWS Lambda | Publicação via `dotnet lambda deploy-function` |

### API — Deploy Containerizado

```bash
# Build da imagem de produção
docker build -f docker/api/Dockerfile -t registration-api:latest .

# Push para ECR (exemplo)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com
docker tag registration-api:latest <account-id>.dkr.ecr.us-east-1.amazonaws.com/registration-api:latest
docker push <account-id>.dkr.ecr.us-east-1.amazonaws.com/registration-api:latest
```

### Lambda — Deploy

```bash
cd lambda

# Publicar a Lambda
dotnet lambda deploy-function EventHandler \
  --function-handler "EventHandler::EventHandler.Handlers.SnsEventHandler::FunctionHandler" \
  --function-runtime dotnet8 \
  --function-memory 256 \
  --function-timeout 30
```

### CI/CD

O projeto não possui pipeline CI/CD configurado atualmente. Sugestão de pipeline:

```
┌─────────────┐     ┌───────────┐     ┌──────────┐     ┌────────────┐
│   Push/PR    │────►│   Build   │────►│  Tests   │────►│   Deploy   │
│  (GitHub)    │     │ (restore) │     │ (xunit)  │     │ (ECS/Lambda)│
└─────────────┘     └───────────┘     └──────────┘     └────────────┘
```

**Ferramentas sugeridas:** GitHub Actions, AWS CodePipeline ou Azure DevOps.

### Notas de Infraestrutura

- O SQL Server em produção deve ser um **Azure SQL** ou **Amazon RDS for SQL Server** (não um container Docker)
- A API em produção deve usar um **Dockerfile de produção** otimizado (multi-stage build com imagem `aspnet` runtime)
- A Lambda deve ter uma **SNS Subscription** configurada apontando para o tópico SNS da API
- Considerar **VPC** para isolamento de rede entre API, banco e Lambda

---

## 10. Tratamento de Erros e Logging

### Tratamento Global de Exceções

O middleware `ExceptionHandlingMiddleware` intercepta todas as exceções não tratadas e as converte em respostas HTTP padronizadas no formato **RFC 7807 (Problem Details)**.

**Mapeamento de exceções:**

| Exceção | HTTP Status | Title |
|---|---|---|
| `UserAlreadyExistsException` | `409 Conflict` | Conflict |
| `ProductAlreadyExistsException` | `409 Conflict` | Conflict |
| `UserNotFoundException` | `404 Not Found` | Not Found |
| `ProductNotFoundException` | `404 Not Found` | Not Found |
| `DomainException` (base) | `400 Bad Request` | Bad Request |
| Qualquer outra exceção | `500 Internal Server Error` | Internal Server Error |

**Comportamento de logging:**

- Exceções `5xx` → `LogError` com stack trace completo
- Exceções de domínio → `LogWarning` com tipo e mensagem

### Logging da API

Configurado via `appsettings.json`:

| Ambiente | Nível padrão | EF Core | ASP.NET Core |
|---|---|---|---|
| Produção | `Information` | `Warning` | `Warning` |
| Desenvolvimento | `Debug` | `Information` (SQL queries) | `Information` |

### Logging da Lambda

- Utiliza **Serilog** com sink para console (CloudWatch captura `stdout`)
- Logging estruturado com propriedades nomeadas (`{UserId}`, `{EventType}`, etc.)
- Tags de contexto: `[AUDIT]` para trail de auditoria

### Resiliência

- O EF Core está configurado com **retry automático** no SQL Server:
  - Máximo de 3 tentativas
  - Delay máximo de 10 segundos entre tentativas
- O `SnsEventPublisher` loga erros e propaga exceções para que o chamador decida a estratégia

---

## 11. Estratégia de Testes

### Estrutura de Testes

Os testes ficam no projeto `service/tests/` e utilizam:

| Ferramenta | Propósito |
|---|---|
| **xUnit** | Framework de testes |
| **Moq** | Mocking de dependências |
| **FluentAssertions** | Assertions legíveis |
| **Coverlet** | Cobertura de código |

### Testes Implementados

#### Camada Domain

| Teste | Descrição |
|---|---|
| `UserTests.Create_WithValidData_ShouldReturnUserWithCorrectProperties` | Valida factory method de `User` |
| `UserTests.Create_ShouldSetCreatedAtToUtcNow` | Valida timestamp UTC |
| `ProductTests.Create_WithValidData_ShouldReturnProductWithCorrectProperties` | Valida factory method de `Product` |
| `ProductTests.Create_WithNullDescription_ShouldDefaultToEmptyString` | Valida default de descrição nula |
| `ProductTests.Create_ShouldSetCreatedAtToUtcNow` | Valida timestamp UTC |

#### Camada Application

| Teste | Descrição |
|---|---|
| `UserServiceTests.CreateUserAsync_WithValidRequest_ShouldCreateUserAndPublishEvent` | Fluxo completo de criação (persist + evento) |
| `UserServiceTests.CreateUserAsync_WithDuplicateEmail_ShouldThrowUserAlreadyExistsException` | Rejeição de e-mail duplicado |
| `UserServiceTests.GetUserByIdAsync_WithExistingUser_ShouldReturnUserResponse` | Busca com sucesso |
| `UserServiceTests.GetUserByIdAsync_WithNonExistingUser_ShouldThrowUserNotFoundException` | Busca com falha |
| `UserServiceTests.GetAllUsersAsync_ShouldReturnCorrectNumberOfUsers` | Listagem parametrizada (0, 1, 5) |
| `ProductServiceTests.CreateProductAsync_WithValidRequest_ShouldCreateProductAndPublishEvent` | Fluxo completo de criação |
| `ProductServiceTests.CreateProductAsync_WithDuplicateSku_ShouldReturnExistingProduct` | Rejeição de SKU duplicado |
| `ProductServiceTests.GetProductByIdAsync_WithExistingProduct_ShouldReturnProduct` | Busca com sucesso |
| `ProductServiceTests.GetProductByIdAsync_WithNonExistingProduct_ShouldThrowProductNotFoundException` | Busca com falha |
| `ProductServiceTests.GetAllProductsAsync_ShouldReturnCorrectNumberOfProducts` | Listagem parametrizada (0, 1, 5) |

### Padrão dos Testes

Todos os testes seguem o padrão **Arrange-Act-Assert (AAA)**:

```csharp
[Fact]
public async Task CreateUserAsync_WithValidRequest_ShouldCreateUserAndPublishEvent()
{
    // Arrange — preparação dos mocks e dados
    var request = new CreateUserRequest("John Doe", "john.doe@example.com");
    _userRepositoryMock
        .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
        .ReturnsAsync((User?)null);

    // Act — execução do método testado
    var result = await _sut.CreateUserAsync(request);

    // Assert — verificação dos resultados
    result.Name.Should().Be(request.Name);
    _eventPublisherMock.Verify(
        p => p.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()),
        Times.Once);
}
```

### Executando Testes

```bash
# Todos os testes
dotnet test

# Apenas testes de domínio
dotnet test --filter "FullyQualifiedName~Domain"

# Apenas testes de aplicação
dotnet test --filter "FullyQualifiedName~Application"

# Com cobertura
dotnet test --collect:"XPlat Code Coverage"
```

---