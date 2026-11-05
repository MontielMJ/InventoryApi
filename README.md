# Inventory API

RESTful API for product, category and inventory movement management. The solution is built with .NET 10, SQL Server, Entity Framework Core for reads, Dapper for writes, CQRS, OAuth2 (Keycloak) and Docker Compose.

## Table of contents

- [Overview](#overview)
- [Tech stack](#tech-stack)
- [Architecture](#architecture)
- [Data model](#data-model)
- [Project structure](#project-structure)
- [Quick start with Docker](#quick-start-with-docker)
- [Authentication](#authentication)
- [API endpoints](#api-endpoints)
- [Local development](#local-development)
- [Database migrations](#database-migrations)
- [Tests and coverage](#tests-and-coverage)
- [Configuration](#configuration)
- [Design decisions](#design-decisions)
- [Clean code rules](#clean-code-rules)
- [AI assisted development](#ai-assisted-development)
- [Known limitations](#known-limitations)

## Overview

The API exposes CRUD operations for products and categories, plus a ledger of inventory movements (`In` and `Out`). Stock is never edited directly through the product endpoints: it only changes when an inventory movement is registered, which keeps an auditable history and prevents negative stock.

Key characteristics:

- CQRS with one handler per command and one handler per query.
- Entity Framework Core for read operations with projections and `AsNoTracking`.
- Dapper with parameterized SQL for write operations.
- Inventory movements executed inside a SQL Server transaction with `UPDLOCK` to avoid lost updates.
- OAuth2 protection through Keycloak, with client credentials for machine-to-machine calls and authorization code plus PKCE for Swagger.
- Swagger UI with the OAuth2 flow configured.
- Unit tests for domain rules, handlers and validators.
- Everything configurable through environment variables.

## Tech stack

| Concern | Technology |
|---|---|
| Runtime | .NET 10 (LTS) |
| Web | ASP.NET Core controllers |
| Read side | Entity Framework Core 10 |
| Write side | Dapper 2.x |
| Database | SQL Server 2022 |
| CQRS | MediatR 12 |
| Validation | FluentValidation 11 |
| Authentication | OAuth2 / OpenID Connect with Keycloak 26 |
| Documentation | Swagger / OpenAPI with Swashbuckle |
| Tests | xUnit, NSubstitute, Shouldly, coverlet |
| Containers | Docker, Docker Compose |

## Architecture

The solution follows a layered architecture with dependencies pointing inward:

```text
Inventory.Api  ->  Inventory.Infrastructure  ->  Inventory.Application  ->  Inventory.Domain
```

- `Inventory.Domain`: entities, enums, business rules and domain exceptions. No framework dependencies.
- `Inventory.Application`: commands, queries, handlers, validators, DTOs and repository interfaces. This is the CQRS core.
- `Inventory.Infrastructure`: EF Core `DbContext`, configurations, migrations, read repositories, Dapper write repositories and dependency injection.
- `Inventory.Api`: controllers, authentication, Swagger, middleware and application startup.

Request flow:

```text
Controller -> MediatR -> ValidationBehavior -> Handler -> Read repository (EF Core)
                                                     \-> Write repository (Dapper)
```

Responsibilities are separated by design:

- Controllers only translate HTTP to commands/queries and HTTP status codes.
- Handlers contain orchestration and business flow.
- Domain rules live in entities and `StockPolicy`.
- Repositories only deal with persistence.

## Data model

```text
Categories
  Id (uniqueidentifier, PK)
  Name (nvarchar(120), unique)
  Description (nvarchar(500), null)
  IsActive (bit)
  CreatedAt, UpdatedAt (datetime2)

Products
  Id (uniqueidentifier, PK)
  Sku (nvarchar(40), unique)
  Name (nvarchar(200))
  Description (nvarchar(1000), null)
  CategoryId (FK -> Categories.Id, restrict delete)
  Price (decimal(18,2))
  Stock (int)
  MinimumStock (int)
  IsActive (bit)
  CreatedAt, UpdatedAt (datetime2)

InventoryMovements
  Id (uniqueidentifier, PK)
  ProductId (FK -> Products.Id, restrict delete)
  Type (int, 1 = In, 2 = Out)
  Quantity (int)
  UnitCost (decimal(18,2))
  Reason (nvarchar(500), null)
  Reference (nvarchar(100), null)
  CreatedBy (nvarchar(100))
  CreatedAt (datetime2)
```

`Products.Stock` is a denormalized value kept in sync with the movement ledger inside the same transaction. This keeps reads fast while preserving traceability. The update uses `UPDLOCK, ROWLOCK` on the product row to serialize concurrent movements and guarantee stock never becomes negative.

Seed data is inserted through the initial migration: three categories, three products and the corresponding `In` movements that justify their initial stock.

## Project structure

```text
.
├── src
│   ├── Inventory.Api
│   │   ├── Controllers
│   │   ├── Contracts
│   │   ├── Extensions
│   │   ├── Middleware
│   │   └── Program.cs
│   ├── Inventory.Application
│   │   ├── Abstractions
│   │   ├── Behaviors
│   │   ├── Categories
│   │   ├── Common
│   │   ├── Inventory
│   │   └── Products
│   ├── Inventory.Domain
│   │   ├── Entities
│   │   ├── Enums
│   │   ├── Exceptions
│   │   └── Rules
│   └── Inventory.Infrastructure
│       ├── Persistence
│       │   ├── Configurations
│       │   ├── Dapper
│       │   ├── Ef
│       │   └── Migrations
│       └── DependencyInjection.cs
├── tests
│   └── Inventory.UnitTests
├── docs
│   ├── ai
│   └── decision-questions.md
├── docker
│   └── keycloak
├── scripts
│   ├── check-clean-code.ps1
│   └── smoke-test.ps1
├── Dockerfile
├── docker-compose.yml
└── InventoryApi.sln
```

## Quick start with Docker

Requirements:

- Docker Desktop with Docker Compose v2.
- Ports `14330`, `8080` and `8081` available. Port `14330` is used on the host because a local SQL Server installation commonly owns `1433`.

Steps:

1. Optionally create a local environment file:

   ```bash
   cp .env.example .env
   ```

2. Start the full stack:

   ```bash
   docker compose up --build
   ```

   This starts:

   | Service | URL |
   |---|---|
   | API | http://localhost:8081 |
   | Swagger UI | http://localhost:8081/swagger |
   | Health check | http://localhost:8081/health |
   | Keycloak | http://localhost:8080 |
   | SQL Server | localhost,14330 |

3. Wait for SQL Server and Keycloak to be ready. The API applies EF Core migrations on startup and retries for up to 30 seconds.

4. Open Swagger UI, click **Authorize**, choose the `oauth2` scheme, and sign in with the demo user:

   ```text
   Username: inventory-admin
   Password: Admin123!
   ```

5. Stop the stack:

   ```bash
   docker compose down
   ```

   To also remove the database volume:

   ```bash
   docker compose down -v
   ```

### Connecting to the containerized database

The container listens on `1433` internally and is published on host port `14330`, so a local SQL Server installation can keep using `1433`. To connect from SQL Server Management Studio or another client:

| Setting | Value |
|---|---|
| Server name | `localhost,14330` or `127.0.0.1,14330` |
| Authentication | SQL Server Authentication |
| Login | `sa` |
| Password | `Your_password123`, or the value of `SQL_SA_PASSWORD` in `.env` |
| Database | `InventoryDb` |

Verify the connection with `sqlcmd`:

```bash
sqlcmd -S localhost,14330 -U sa -P "Your_password123" -C -Q "SELECT name FROM sys.databases"
```

## Authentication

Keycloak is imported with a `inventory` realm on startup. It contains:

| Client | Type | Purpose |
|---|---|---|
| `inventory-api` | Confidential, service account | Machine-to-machine access with `client_credentials` |
| `inventory-swagger` | Public, standard flow plus PKCE | Swagger UI authorization code flow |

### Client credentials (scripts, Postman, CI)

```bash
curl -s -X POST "http://localhost:8080/realms/inventory/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=inventory-api" \
  -d "client_secret=inventory-api-secret"
```

PowerShell:

```powershell
$response = Invoke-RestMethod -Method Post `
  -Uri "http://localhost:8080/realms/inventory/protocol/openid-connect/token" `
  -Body @{ grant_type = "client_credentials"; client_id = "inventory-api"; client_secret = "inventory-api-secret" }
$token = $response.access_token
```

### Authorization code with PKCE (Swagger)

Swagger UI is already configured with:

- Authorization URL: `http://localhost:8080/realms/inventory/protocol/openid-connect/auth`
- Token URL: `http://localhost:8080/realms/inventory/protocol/openid-connect/token`
- Client ID: `inventory-swagger`
- Redirect URI: `http://localhost:8081/swagger/oauth2-redirect.html`

The API validates JWT bearer tokens and checks issuer, audience, signature and lifetime. The `inventory-api` audience is added by a Keycloak protocol mapper.

### Why `Auth__Authority` and `Auth__Issuer` differ

The API container resolves Keycloak through the internal Docker network (`http://keycloak:8080`), but tokens obtained from the host contain `http://localhost:8080` as issuer. The API reads the OpenID metadata from the internal authority and validates the issuer against the public URL. This is a common split-horizon deployment issue.

## API endpoints

All endpoints except `/health` require a valid OAuth2 access token.

### Products

| Method | Route | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/products` | Paged list with filters | 200 | 401 |
| GET | `/api/products/{id}` | Product by id | 200 | 401, 404 |
| POST | `/api/products` | Create product | 201 | 400, 401, 404, 409 |
| PUT | `/api/products/{id}` | Update product | 204 | 400, 401, 404, 409 |
| DELETE | `/api/products/{id}` | Soft delete product | 204 | 401, 404 |

Query parameters for `GET /api/products`:

- `categoryId` (guid, optional)
- `search` (string, optional, matches name or SKU)
- `isActive` (bool, optional)
- `page` (int, default 1)
- `pageSize` (int, default 10, max 100)

### Categories

| Method | Route | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/categories?includeInactive=false` | List categories | 200 | 401 |
| GET | `/api/categories/{id}` | Category by id | 200 | 401, 404 |
| POST | `/api/categories` | Create category | 201 | 400, 401, 409 |
| PUT | `/api/categories/{id}` | Update category | 204 | 400, 401, 404, 409 |
| DELETE | `/api/categories/{id}` | Soft delete category | 204 | 401, 404, 409 |

A category cannot be deleted while it has active products.

### Inventory movements

| Method | Route | Description | Success | Errors |
|---|---|---|---|---|
| GET | `/api/inventory-movements?productId=&page=&pageSize=` | Paged movement history | 200 | 401 |
| POST | `/api/inventory-movements` | Register `In` or `Out` movement | 201 | 400, 401, 404, 409 |

Example: register an `Out` movement.

```bash
curl -X POST "http://localhost:8081/api/inventory-movements" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "type": "Out",
    "quantity": 3,
    "unitCost": 32.50,
    "reason": "Customer order",
    "reference": "ORD-1001"
  }'
```

If the requested quantity exceeds the current stock, the API returns `409 Conflict` with a `ProblemDetails` payload.

### Error format

Errors use RFC 7807 `ProblemDetails`:

```json
{
  "type": "https://httpstatuses.com/409",
  "title": "Conflict",
  "status": 409,
  "errors": [
    "Requested quantity 50 exceeds available stock 25."
  ]
}
```

Validation errors return `400` with all FluentValidation messages in `errors`.

## Local development

Requirements:

- .NET SDK 10.0.
- SQL Server and Keycloak, or Docker to run only those two services.
- `dotnet-ef` tool, restored from the local tool manifest.

Start the dependencies:

```bash
docker compose up -d sqlserver keycloak
```

`appsettings.Development.json` already contains local defaults that match the Compose stack, so no manual exports are needed:

| Setting | Local default |
|---|---|
| SQL Server | `localhost,14330`, database `InventoryDb`, user `sa` |
| Keycloak | `http://localhost:8080/realms/inventory` |
| Audience | `inventory-api` |
| Swagger client | `inventory-swagger` with the authorization and token URLs of the realm |

Configuration precedence is `appsettings.json`, then `appsettings.{Environment}.json`, then environment variables. The JSON files only contain non-production defaults or placeholders; production values must come from the environment.

To replace every default (for example in CI or when using another host), set the variables explicitly. PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DefaultConnection = "Server=localhost,14330;Database=InventoryDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=False"
$env:Auth__Authority = "http://localhost:8080/realms/inventory"
$env:Auth__Issuer = "http://localhost:8080/realms/inventory"
$env:Auth__Audience = "inventory-api"
$env:Swagger__ClientId = "inventory-swagger"
$env:Swagger__AuthorizationUrl = "http://localhost:8080/realms/inventory/protocol/openid-connect/auth"
$env:Swagger__TokenUrl = "http://localhost:8080/realms/inventory/protocol/openid-connect/token"
```

Bash:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost,14330;Database=InventoryDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;Encrypt=False"
export Auth__Authority="http://localhost:8080/realms/inventory"
export Auth__Issuer="http://localhost:8080/realms/inventory"
export Auth__Audience="inventory-api"
export Swagger__ClientId="inventory-swagger"
export Swagger__AuthorizationUrl="http://localhost:8080/realms/inventory/protocol/openid-connect/auth"
export Swagger__TokenUrl="http://localhost:8080/realms/inventory/protocol/openid-connect/token"
```

Run the API:

```bash
dotnet run --project src/Inventory.Api
```

The API starts on `http://localhost:8081` because of `launchSettings.json`.

### Debugging

- In Visual Studio or Rider, select `Inventory.Api` as the startup project and use the `Inventory.Api` profile. The profile sets `ASPNETCORE_ENVIRONMENT=Development`, so the local defaults are loaded automatically.
- In VS Code, use the C# debugger with the same profile or environment variables.
- The API logs unhandled exceptions with full stack traces. Domain and validation errors are logged as expected client errors.
- If startup fails with `Configuration value 'Auth:Authority' is required`, you are running in an environment without a value for that key.
- If SQL Server is not ready yet, the migration initializer retries up to 10 times with 3 second intervals.

## Database migrations

Restore the local tool:

```bash
dotnet tool restore
```

Create a migration:

```bash
dotnet tool run dotnet-ef migrations add <MigrationName> \
  --project src/Inventory.Infrastructure \
  --startup-project src/Inventory.Api \
  --output-dir Persistence/Migrations
```

Update the database manually:

```bash
dotnet tool run dotnet-ef database update \
  --project src/Inventory.Infrastructure \
  --startup-project src/Inventory.Api
```

The `InventoryDbContextFactory` reads `ConnectionStrings__DefaultConnection` from the environment, so the same variable used by the API also works for EF tools.

Migrations are applied automatically when the API starts. In production, this behavior can be replaced by a dedicated migration job, but for this exercise it guarantees `docker compose up` works with a single command.

## Tests and coverage

Run all unit tests:

```bash
dotnet test
```

Run tests with coverage collection:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

The coverage file is generated under `tests/Inventory.UnitTests/TestResults/<guid>/coverage.cobertura.xml`. It can be opened with tools such as ReportGenerator or Visual Studio.

Current test coverage focuses on the critical paths:

- `StockPolicy`: stock increases, decreases and rejects negative results.
- `RegisterMovementCommandHandler`: success, product not found and insufficient stock.
- `CreateProductCommandHandler`: success, unknown category and duplicated SKU.
- `UpdateProductCommandHandler`: success, unknown product and duplicated SKU.
- `DeleteCategoryCommandHandler`: success, category with active products and unknown category.
- `GetProductsQueryHandler`: pagination normalization and delegation.
- Validators for product creation and inventory movements.

Validate the Clean Code rules that can be automated:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/check-clean-code.ps1
```

With PowerShell 7 installed, `pwsh ./scripts/check-clean-code.ps1` also works.

With the full stack running, execute the end-to-end smoke test:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/smoke-test.ps1
```

It verifies the health endpoint, token issuance, Swagger, authentication enforcement, product and category CRUD, stock movements, insufficient stock handling and the category deletion rule. The script creates temporary data and cleans it up.

## Configuration

| Environment variable | Required in production | Development default | Description |
|---|---|---|---|
| `ConnectionStrings__DefaultConnection` | Yes | `localhost,14330`, database `InventoryDb`, user `sa`, demo password | SQL Server connection string |
| `Auth__Authority` | Yes | `http://localhost:8080/realms/inventory` | OIDC authority used to download metadata |
| `Auth__Issuer` | Yes | `http://localhost:8080/realms/inventory` | Expected `iss` claim value |
| `Auth__Audience` | Yes | `inventory-api` | Expected `aud` claim value |
| `Swagger__ClientId` | Yes | `inventory-swagger` | Public OAuth2 client used by Swagger UI |
| `Swagger__AuthorizationUrl` | Yes | `http://localhost:8080/realms/inventory/protocol/openid-connect/auth` | OAuth2 authorization endpoint |
| `Swagger__TokenUrl` | Yes | `http://localhost:8080/realms/inventory/protocol/openid-connect/token` | OAuth2 token endpoint |
| `ASPNETCORE_URLS` | No | `http://localhost:8081` via launch profile, `http://+:8080` in Docker | Listening URL |
| `SQL_SA_PASSWORD` | No | `Your_password123` | SQL Server SA password used by Docker Compose |
| `KEYCLOAK_ADMIN` | No | `admin` | Keycloak bootstrap admin user |
| `KEYCLOAK_ADMIN_PASSWORD` | No | `admin` | Keycloak bootstrap admin password |

The development defaults are stored in `src/Inventory.Api/appsettings.Development.json` and match the Compose stack. Environment variables always take precedence. `appsettings.json` keeps empty placeholders for production, so a deployment must provide the required values explicitly. The demo password is not a secret: it is intentionally public and only valid for the local containers.

## Design decisions

### CQRS without over-engineering

Commands and queries are separated into different folders and handlers, each with a single responsibility. MediatR is used as the in-process dispatcher. A pipeline behavior runs FluentValidation before every handler. There is no event sourcing, no message broker and no separate read database, because the scope of the exercise does not justify that operational cost.

### EF Core for reads, Dapper for writes

Read repositories use EF Core with `AsNoTracking` and typed projections. Write repositories use Dapper with parameterized SQL. The split follows the requirement literally and keeps each tool in the place where it is strongest: EF for expressive querying, Dapper for explicit, low-overhead writes.

Both sides share the same SQL Server schema, created and versioned by EF Core migrations. This avoids the common problem of two divergent schema definitions.

### Stock as a ledger plus a denormalized value

`InventoryMovements` is the source of truth for auditability. `Products.Stock` is a cached value for fast reads. Registering a movement performs three steps in one transaction:

1. Read the product row with `UPDLOCK, ROWLOCK`.
2. Validate and calculate the next stock.
3. Insert the movement and update the product stock.

This prevents negative stock under concurrency without requiring distributed locks or optimistic concurrency columns that Dapper would bypass anyway.

### Soft deletes

Products and categories are deactivated instead of physically deleted. Historical inventory movements must keep referencing existing rows, and a hard delete would either fail or destroy audit data.

### OAuth2 with Keycloak

Keycloak is free, widely used and easy to run in Docker. It provides a real OAuth2/OIDC server instead of a hand-rolled token issuer. The realm, clients, mapper and demo user are versioned in `docker/keycloak/realm-export.json`, so the environment is reproducible.

### ProblemDetails for errors

Domain exceptions are translated into RFC 7807 responses by a single middleware. Controllers do not contain try/catch blocks, and validation errors are returned consistently.

### Unit tests without over-mocking

Handlers are tested with NSubstitute substitutes for repository interfaces. Domain rules are tested directly. Validators are tested with `FluentValidation.TestHelper`. The goal is to protect business behavior, not to reach an arbitrary coverage number.

## Clean code rules

The exercise defines strict rules, and the repository applies them:

- All code, identifiers and comments are in English.
- There are no blank lines between code lines, except a single line before `return` statements.
- Methods are small and have a single responsibility.
- Configuration is read through the .NET configuration system, and environment variables take precedence over the JSON files. Local demo defaults live in `appsettings.Development.json`.
- Each class and enum lives in its own file.
- There is no commented-out code.
- Dependencies are passed as objects and interfaces, not as loose primitive parameters.
- Automated checks cover blank lines, comments and non-ASCII characters. Run `powershell -NoProfile -ExecutionPolicy Bypass -File ./scripts/check-clean-code.ps1`.

## AI assisted development

The use of generative AI is documented as required by the exercise:

- [`docs/ai/spec.md`](docs/ai/spec.md): the specification and API contract written before implementation.
- [`docs/ai/prompts.md`](docs/ai/prompts.md): the main prompts used and a reasoned summary.
- [`docs/ai/decisions.md`](docs/ai/decisions.md): what the AI proposed, what was accepted, what was changed and why.
- [`docs/decision-questions.md`](docs/decision-questions.md): answers to the decision-making questions.

## Known limitations

- Keycloak runs with `start-dev`, which is not suitable for production. A production deployment should use `start` with HTTPS, an external database and a proper hostname.
- SQL Server uses the Express edition in the Compose file. It is enough for the exercise but not for production workloads.
- Authorization only requires an authenticated user. Realm roles (`inventory-admin`, `inventory-reader`) are defined but policies are not enforced yet.
- There are no integration tests against a real SQL Server container. Unit tests cover handlers, domain rules and validators.
- Pagination does not return navigation links or total page metadata beyond `totalPages`.
- The API applies migrations at startup. For multi-instance deployments this should move to a dedicated migration job.
