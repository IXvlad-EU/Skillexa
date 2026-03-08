---
applyTo: "{skillexa-core,skillexa-engine}/**"
---

# CQRS — Instructions

## Overview

Skillexa-Core follows the **Command Query Responsibility Segregation (CQRS)** pattern. Read operations (queries) and write operations (commands) use **separate models** within a **single PostgreSQL data store**. This keeps the architecture simple — no separate read database, no Event Sourcing — while gaining clear separation of concerns, optimized DTOs, and independent scalability of read/write paths.

```
                          ┌─────────────┐
  POST /documents ──────▸ │  Command    │──▸ IUnitOfWork (repositories) ──▸ DbContext ──▸ PostgreSQL
                          │  Handler    │──▸ IUnitOfWork.SaveChangesAsync() (atomic commit)
                          └─────────────┘

                          ┌─────────────┐
  GET /jobs ────────────▸ │  Query      │──▸ Read DTO ◂── DbContext (AsNoTracking projection) ◂── PostgreSQL
                          │  Handler    │
                          └─────────────┘
```

## Core Principles

1. **Commands change state; queries return data.** Never mix the two in a single handler.
2. **Commands never return domain entities.** They return at most a thin acknowledgement DTO (e.g., `{ jobId, status }`).
3. **Queries never mutate state.** They produce read-optimized DTOs or projections — no side-effects.
4. **Single data store, separate models.** Both command and query paths hit the same PostgreSQL database but use distinct model types (domain entities vs. read DTOs).
5. **No Event Sourcing.** The write side persists current state directly via EF Core. Async side-effects (e.g., PDF generation) are handled by publishing messages to the broker, not by replaying an event stream.
6. **Commands use `IUnitOfWork`; queries use `ApplicationDbContext` directly.** Command handlers access repositories and commit via `IUnitOfWork.SaveChangesAsync()`. Query handlers inject `ApplicationDbContext` and read with `AsNoTracking()`. See `unit-of-work.instructions.md` for full details.

## Folder Structure

```
skillexa-core/
  Commands/                  # command interfaces + definitions + handlers
    ICommand.cs
    ICommandHandler.cs
    CreateDocument/
      CreateDocumentCommand.cs
      CreateDocumentHandler.cs
      CreateDocumentResult.cs
    UpdateJobStatus/
      UpdateJobStatusCommand.cs
      UpdateJobStatusHandler.cs
  Queries/                   # query interfaces + definitions + handlers
    IQuery.cs
    IQueryHandler.cs
    GetJobs/
      GetJobsQuery.cs
      GetJobsHandler.cs
      GetJobsResult.cs        # read DTO
    GetJobById/
      GetJobByIdQuery.cs
      GetJobByIdHandler.cs
      GetJobByIdResult.cs
```

Each command or query lives in its own subfolder containing the **request**, **handler**, and (if needed) **result** types. This prevents bloated "service" classes and makes each operation discoverable.

## Contracts

### Commands

```csharp
// Commands/ICommand.cs
public interface ICommand<TResult>;

// Commands/ICommandHandler.cs
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
```

### Queries

```csharp
// Queries/IQuery.cs
public interface IQuery<TResult>;

// Queries/IQueryHandler.cs
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

## Command Rules

| Rule                  | Detail                                                              |
| --------------------- | ------------------------------------------------------------------- |
| **Type**              | `record` (immutable)                                                |
| **Naming**            | Verb + noun: `CreateDocument`, `UpdateJobStatus`                    |
| **Validation**        | Validate at handler entry; throw a domain exception on bad input    |
| **Persistence**       | Access entities via `IUnitOfWork` — never inject `DbContext` direct |
| **Commit**            | Call `IUnitOfWork.SaveChangesAsync()` exactly once per handler      |
| **Side-effects**      | Stage outbox messages — never call `IMessageBus` synchronously      |
| **Return value**      | Thin result record — never the full domain entity                   |
| **Idempotency**       | Set `idempotencyKey`; enforce unique constraint on `jobs` table     |
| **Transaction scope** | One handler = one unit of work; stage writes + outbox, then commit  |

### Example Command

```csharp
// Commands/CreateDocument/CreateDocumentCommand.cs
public record CreateDocumentCommand(
    long UserId,
    string TemplateKey,
    int? TemplateVersion,
    string PayloadJson) : ICommand<CreateDocumentResult>;

// Commands/CreateDocument/CreateDocumentResult.cs
public record CreateDocumentResult(long JobId, string Status);

// Commands/CreateDocument/CreateDocumentHandler.cs
public class CreateDocumentHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDocumentCommand, CreateDocumentResult>
{
    public async Task<CreateDocumentResult> HandleAsync(
        CreateDocumentCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Build domain entity
        var job = new Job
        {
            UserId = command.UserId,
            StatusId = JobStatuses.Queued,
            TemplateKey = command.TemplateKey,
            TemplateVersion = command.TemplateVersion ?? await ResolveActiveVersion(command.TemplateKey, cancellationToken),
            Payload = command.PayloadJson,
            IdempotencyKey = /* generate or derive */,
            CorrelationId = /* generate */,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // 2. Stage entity via repository
        await unitOfWork.Jobs.AddAsync(job, cancellationToken);

        // 3. Stage outbox message for async side-effect
        await unitOfWork.OutboxMessages.AddAsync(new OutboxMessage
        {
            Type = "GeneratePdf",
            PayloadJson = JsonSerializer.Serialize(new GeneratePdf { JobId = job.Id, /* ... */ }),
            CreatedAt = DateTime.UtcNow,
        }, cancellationToken);

        // 4. Atomic commit — entity + outbox message in one transaction
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Return thin result
        return new CreateDocumentResult(job.Id, "Queued");
    }
}
```

## Query Rules

| Rule                       | Detail                                                     |
| -------------------------- | ---------------------------------------------------------- |
| **Type**                   | `record` (immutable)                                       |
| **Naming**                 | `Get` / `List` + noun: `GetJobById`, `GetJobs`             |
| **No mutation**            | Never call `SaveChangesAsync` or publish messages          |
| **Projection**             | Use `.Select()` or Mapperly — never return domain entities |
| **Performance**            | Always `AsNoTracking()`; consider Dapper for hot queries   |
| **Filtering / pagination** | Accept `Page`, `PageSize`, filters in the query record     |
| **Return value**           | Read DTO record or paginated wrapper                       |

### Example Query

```csharp
// Queries/GetJobs/GetJobsQuery.cs
public record GetJobsQuery(long UserId, int Page = 1, int PageSize = 20) : IQuery<IReadOnlyList<GetJobsResult>>;

// Queries/GetJobs/GetJobsResult.cs
public record GetJobsResult(
    long Id,
    string Status,
    string TemplateKey,
    string? ErrorCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// Queries/GetJobs/GetJobsHandler.cs
public class GetJobsHandler(ApplicationDbContext db)
    : IQueryHandler<GetJobsQuery, IReadOnlyList<GetJobsResult>>
{
    public async Task<IReadOnlyList<GetJobsResult>> HandleAsync(
        GetJobsQuery query, CancellationToken cancellationToken = default)
    {
        return await db.Jobs
            .AsNoTracking()
            .Where(job => job.UserId == query.UserId)
            .OrderByDescending(job => job.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(job => new GetJobsResult(
                job.Id,
                job.Status.Name,
                job.TemplateKey,
                job.ErrorCode,
                job.CreatedAt,
                job.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}
```

## Wiring — Autofac Registration

Register all command and query handlers via assembly scanning in a dedicated Autofac module:

```csharp
// Modules/CqrsModule.cs
public class CqrsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = typeof(CqrsModule).Assembly;

        // Register all ICommandHandler<,> implementations
        builder.RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(ICommandHandler<,>))
            .InstancePerLifetimeScope();

        // Register all IQueryHandler<,> implementations
        builder.RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(IQueryHandler<,>))
            .InstancePerLifetimeScope();
    }
}
```

Register the module in `Program.cs` alongside existing modules.
Repositories and `IUnitOfWork` are registered separately in `DataModule` — see `unit-of-work.instructions.md`.

```csharp
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new DataModule(builder.Configuration));
    container.RegisterModule(new CqrsModule());
    // ... other modules
});
```

## Endpoint Integration

Minimal API endpoints act as **thin adapters** — they extract request data, construct the command/query, dispatch to the handler, and map the result to an HTTP response. Endpoints must **not** contain business logic or data access calls.

```csharp
// Endpoints/DocumentsEndpoints.cs
group.MapPost("/documents", async (
    CreateDocumentRequest req,
    ICommandHandler<CreateDocumentCommand, CreateDocumentResult> handler,
    HttpContext ctx,
    CancellationToken cancellationToken) =>
{
    var userId = ctx.User.GetUserId(); // extension method
    var command = new CreateDocumentCommand(userId, req.TemplateKey, req.TemplateVersion, req.PayloadJson);
    var result = await handler.HandleAsync(command, cancellationToken);
    return TypedResults.Accepted($"/jobs/{result.JobId}", result);
})
.RequireAuthorization()
.WithOpenApi();
```

Endpoints never inject `IUnitOfWork`, `ApplicationDbContext`, or repositories directly — all data access is encapsulated inside command/query handlers.

## Cross-Cutting Concerns

### Validation

- Validate inside the handler (or via a decorator/pipeline behavior wrapping the handler).
- Use FluentValidation or manual checks — keep validation co-located with the command/query definition.

### Logging & Correlation

- Log command/query execution with `correlationId` for traceability.
- A decorator pattern around handlers is recommended for consistent structured logging without polluting handler code.

### Error Handling

- Command handlers throw typed domain exceptions (e.g., `JobNotFoundException`, `QuotaExceededException`).
- A global exception-handling middleware maps domain exceptions to RFC 9457 problem details responses.
- Query handlers return empty results (not exceptions) when no data is found, unless the endpoint specifies 404 semantics.

## What NOT to Do

| Anti-Pattern                                       | Why                                                |
| -------------------------------------------------- | -------------------------------------------------- |
| Return domain entity from a query handler          | Couples read model to write model                  |
| Mutate state inside a query handler                | Violates CQRS read/write separation                |
| Put business logic in endpoint delegates           | Endpoints are adapters — logic belongs in handlers |
| Generic CRUD service for both reads and writes     | Use dedicated handlers per operation               |
| Call `SaveChangesAsync` in a query                 | Queries must be side-effect free                   |
| Skip `AsNoTracking()` on query paths               | Unnecessary change tracking overhead               |
| Multiple unrelated commands in one handler class   | One handler per command/query                      |
| Inject `ApplicationDbContext` in a command handler | Commands must go through `IUnitOfWork`             |
| Inject `IUnitOfWork` in a query handler            | Queries are read-only — use `DbContext` directly   |
