---
applyTo: "skillexa-core/**"
---

# Skillexa-Core — Instructions

## Stack

| Concern        | Choice                                                                                        |
| -------------- | --------------------------------------------------------------------------------------------- |
| Framework      | ASP.NET Core Web API (.NET 10)                                                                |
| Auth           | JWT Bearer authentication (self-issued tokens)                                                |
| DI container   | **Autofac** (replaces built-in DI)                                                            |
| ORM / Data     | EF Core (or Dapper) with PostgreSQL                                                           |
| Mapping        | **Mapperly** (source-generated DTO ↔ Entity mapping)                                          |
| Broker         | `IMessageBus` abstraction — RabbitMQ (local) / Azure Service Bus (prod)                       |
| Object storage | `IObjectStorage` abstraction — Azurite (local) / Azure Blob Storage (prod)                    |
| API docs       | OpenAPI spec (built-in `Microsoft.AspNetCore.OpenApi`) — consumed by Kiota in Skillexa-Portal |
| Root namespace | `Skillexa.Core`                                                                               |

## Role

Skillexa-Core is the **HTTP API and orchestration layer**. It:

1. Authenticates users and issues JWTs.
2. Accepts document creation requests and creates `Job` records in PostgreSQL.
3. Publishes `GeneratePdf` commands to the message broker.
4. Consumes `PdfStatusChanged` events from the broker and updates job state.
5. Generates short-lived signed download URLs for completed PDFs.
6. Enforces per-user / per-plan rate limits and provider quotas.

## Project Layout (recommended)

```
skillexa-core/
  Program.cs
  appsettings.json / appsettings.Development.json
  Properties/launchSettings.json
  Endpoints/             # Minimal API endpoint groups
    AuthEndpoints.cs
    DocumentsEndpoints.cs
    JobsEndpoints.cs
    AdminTemplatesEndpoints.cs
  Models/                # request/response DTOs
  Domain/                # domain entities (Job, User, Template, ProviderUsage…)
    IEntity.cs           # base interface for all entities
  Data/                  # DbContext, migrations, repositories
  Services/              # business logic services
  Messaging/             # IMessageBus, broker adapters, message contracts
  Storage/               # IObjectStorage, Azurite/Azure Blob adapters
  Auth/                  # JWT configuration, token generation, password hashing
  Middleware/            # error handling, rate limiting middleware
  Mapping/               # Mapperly mappers
  Modules/               # Autofac module registrations
```

## Domain Entities — `IEntity`

All domain entities **must** implement the `IEntity` interface:

```csharp
public interface IEntity
{
    long Id { get; set; }
}
```

- `Id` is a `long` (BIGINT) auto-incremented primary key.
- Every entity class (`User`, `Job`, `Template`, `ProviderUsage`, `ProviderQuota`, `RefreshToken`, `OutboxMessage`) implements `IEntity`.
- Lookup / reference tables (e.g., `JobStatus`) may use `int` keys and are **not** required to implement `IEntity`.
- EF Core entity configurations map `Id` to the `id` column with `ValueGeneratedOnAdd()`.

## Dependency Injection — Autofac

- Use **Autofac** as the DI container (`builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())`).
- Avoid mixing Autofac registrations with `builder.Services.Add*()` except for framework-required registrations (authentication, EF Core, etc.).
- Prefer `RegisterAssemblyTypes` with interface-based scanning for automatic registration where appropriate.

### Module Structure

Each architectural concern gets its own **Autofac module** under `Modules/`:

```
Modules/
  DataModule.cs          # DbContext, repositories, Unit of Work
  ServiceModule.cs       # business logic services (IJobService, IAuthService…)
  MessagingModule.cs     # IMessageBus + selected adapter (RabbitMQ / Azure Service Bus)
  StorageModule.cs       # IObjectStorage + selected adapter (Azurite / Azure Blob)
  MappingModule.cs       # Mapperly mapper registrations
  AuthModule.cs          # password hasher, token generator, auth-related services
```

### Module Responsibilities

| Module            | Registers                                                                                 | Notes                                                                                        |
| ----------------- | ----------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `DataModule`      | `ApplicationDbContext`, repository interfaces (`IJobRepository`, `IUserRepository`, etc.) | Reads connection string from config; lifetime = `InstancePerLifetimeScope`                   |
| `ServiceModule`   | All `I*Service` interfaces and their implementations                                      | Scans assembly for `*Service` classes; lifetime = `InstancePerLifetimeScope`                 |
| `MessagingModule` | `IMessageBus` → `RabbitMqMessageBus` or `AzureServiceBusMessageBus`                       | Reads `Messaging:Provider` from config to choose implementation; lifetime = `SingleInstance` |
| `StorageModule`   | `IObjectStorage` → `AzuriteObjectStorage` or `AzureBlobObjectStorage`                     | Reads `Storage:Provider` from config to choose implementation; lifetime = `SingleInstance`   |
| `MappingModule`   | Mapperly mapper classes                                                                   | Registers `[Mapper]`-annotated classes via assembly scanning                                 |
| `AuthModule`      | `IPasswordHasher`, `ITokenGenerator`, related auth services                               | Lifetime = `InstancePerLifetimeScope`                                                        |

### Registration in `Program.cs`

```csharp
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new DataModule(builder.Configuration));
    container.RegisterModule(new ServiceModule());
    container.RegisterModule(new MessagingModule(builder.Configuration));
    container.RegisterModule(new StorageModule(builder.Configuration));
    container.RegisterModule(new MappingModule());
    container.RegisterModule(new AuthModule());
});
```

### Rules

- **One module per concern** — never register unrelated services in the same module.
- Modules that need configuration accept `IConfiguration` (or a typed options section) via their constructor.
- Default lifetime is `InstancePerLifetimeScope` (scoped). Use `SingleInstance` only for truly stateless or connection-pooled services (broker clients, storage clients).
- When adding a new service or adapter, register it in the **appropriate existing module** — do not create ad-hoc registrations in `Program.cs`.

## Mapping — Mapperly

- Use **Mapperly** (source generator) for all DTO ↔ Entity mapping.
- Define `[Mapper]`-annotated partial classes in `Mapping/` (e.g., `JobMapper`, `UserMapper`).
- Mapperly generates mapping code at **compile time** — zero runtime overhead and build-time verification of all mappings.
- Endpoints and services call mapper methods directly (e.g., `JobMapper.ToDto(entity)`).
- **Never** manually map properties when a Mapperly mapper exists for the type pair.
- Keep mappers focused — one mapper per domain aggregate.

```csharp
// Example: Mapping/JobMapper.cs
[Mapper]
public static partial class JobMapper
{
    public static partial JobDto ToDto(Job entity);
    public static partial Job ToEntity(CreateJobRequest dto);
}
```

## JWT Authentication

- Skillexa-Core **issues its own JWTs** (no external identity provider required).
- `POST /auth/login` validates credentials (bcrypt/Argon2id hash) and returns an access token (+ optional refresh token).
- `POST /auth/refresh` issues a new access token given a valid refresh token.
- Tokens are signed with a symmetric key (`JwtSettings:Secret` from config/secrets).
- Configure `AddAuthentication().AddJwtBearer()` in `Program.cs`.
- All endpoints except `/auth/login` and `/auth/refresh` require `[Authorize]`.

## OpenAPI Spec

- The API **must** expose a valid OpenAPI 3.x specification at `/openapi/v1.json` (built-in `Microsoft.AspNetCore.OpenApi`).
- This spec is the **contract** that Skillexa-Portal's Kiota client is generated from.
- Keep DTOs and route metadata accurate — any breaking change requires regenerating the Portal client.
- Use `.WithOpenApi()`, `[EndpointSummary]`, and `[EndpointDescription]` on endpoint definitions for metadata.

## Endpoint Catalog

| Method   | Path                         | Auth         | Purpose                                                 |
| -------- | ---------------------------- | ------------ | ------------------------------------------------------- |
| POST     | `/auth/login`                | Anonymous    | Authenticate, return JWT                                |
| POST     | `/auth/refresh`              | Anonymous    | Refresh access token                                    |
| POST     | `/documents`                 | Bearer       | Create document → enqueue `GeneratePdf`                 |
| GET      | `/jobs`                      | Bearer       | List current user’s jobs                                |
| GET      | `/jobs/{jobId}`              | Bearer       | Single job detail (status, error, timestamps)           |
| POST     | `/jobs/{jobId}/download-url` | Bearer       | Get signed download URL (owner check, status=Succeeded) |
| GET      | `/app/usage`                 | Bearer       | Provider usage / quota for current user                 |
| POST/PUT | `/admin/templates/*`         | Bearer+Admin | Template CRUD (admin only)                              |

## Database Access

- Use EF Core with **code-first migrations** (or Dapper for performance-critical reads).
- `ApplicationDbContext` registers entities: `User`, `Job`, `ProviderUsage`, `ProviderQuota`, `Template`, `OutboxMessage`.
- Connection string comes from `ConnectionStrings:DefaultConnection` (env var override in containers).

## Broker Publishing

- Inject `IMessageBus` and call `PublishAsync<GeneratePdf>(command)`.
- The command is published to queue/topic `pdf-generate`.
- Status events are consumed from `pdf-results`.
- See `messaging.instructions.md` for message contracts.

## Signed Download URLs

- Inject `IObjectStorage` and call `GenerateSignedUrlAsync(key, ttl)`.
- URL is read-only, single-object, TTL 5–15 minutes.
- Verify `job.UserId == currentUser` and `job.Status == Succeeded` before generating.

## Coding Standards

- Nullable reference types enabled (`<Nullable>enable</Nullable>`).
- Implicit usings enabled.
- Use `record` types for DTOs and message contracts.
- Prefer constructor injection via Autofac; **never** use service locator pattern.
- Return `TypedResults` (e.g., `TypedResults.Ok(dto)`, `TypedResults.NotFound()`) from minimal API endpoints.
- All domain entities implement `IEntity` (`long Id`).
- All DTO ↔ Entity mapping goes through Mapperly-generated mappers.
- All public API changes must be reflected in the OpenAPI spec.
