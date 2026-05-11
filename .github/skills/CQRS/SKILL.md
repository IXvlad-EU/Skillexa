---
name: CQRS
description: 'Implement Commands and Queries following the Skillexa CQRS pattern. USE FOR: creating new command handlers (write operations); creating new query handlers (read operations); wiring handlers in CqrsModule; structuring command/query subfolders; applying CQRS rules for IUnitOfWork vs ApplicationDbContext. DO NOT USE FOR: EF Core migrations (use database instructions); message broker publishing (use messaging instructions); unrelated C# code. Keywords: CQRS, command, query, ICommandHandler, IQueryHandler, IUnitOfWork, AsNoTracking, handler, CqrsModule, Autofac, CreateDocument, GetJobs.'
---

## When to Use This Skill

Activate when the user asks to:
- Add a new command (write operation) to `Skillexa-Core` or `Skillexa-Engine`.
- Add a new query (read operation).
- Scaffold the folder/file structure for a handler.
- Review or fix CQRS violations (entity returned from query, mutation in query, DbContext in command, etc.).
- Wire handlers in `CqrsModule`.

---

## Architecture Overview

```
POST /documents ──▸ ICommandHandler ──▸ IUnitOfWork (repositories) ──▸ DbContext ──▸ PostgreSQL
                                     ──▸ IUnitOfWork.SaveChangesAsync() (atomic commit)

GET /jobs ────────▸ IQueryHandler  ──▸ DbContext.AsNoTracking() projection ──▸ PostgreSQL
```

Single PostgreSQL store. No Event Sourcing. No separate read database.

---

## Core Principles

1. **Commands change state; queries return data.** Never mix.
2. **Commands never return domain entities.** Thin result record only.
3. **Queries never mutate state.** No `SaveChangesAsync`, no side-effects.
4. **Commands use `IUnitOfWork`.** Never inject `ApplicationDbContext` into a command handler.
5. **Queries use `ApplicationDbContext` directly with `AsNoTracking()`.** Never inject `IUnitOfWork` into a query handler.
6. **No Event Sourcing.** Async side-effects are outbox messages published to the broker.

---

## Folder Layout

Each operation gets its own subfolder:

```
Commands/
  CreateDocument/
    CreateDocumentCommand.cs   # record : ICommand<CreateDocumentResult>
    CreateDocumentResult.cs    # record (thin DTO)
    CreateDocumentHandler.cs   # ICommandHandler<CreateDocumentCommand, CreateDocumentResult>

Queries/
  GetJobs/
    GetJobsQuery.cs            # record : IQuery<IReadOnlyList<GetJobsResult>>
    GetJobsResult.cs           # record (read DTO)
    GetJobsHandler.cs          # IQueryHandler<GetJobsQuery, IReadOnlyList<GetJobsResult>>
```

---

## Command Rules

| Rule              | Detail                                                                |
| ----------------- | --------------------------------------------------------------------- |
| Type              | `record` (immutable)                                                  |
| Naming            | Verb + noun: `CreateDocument`, `UpdateJobStatus`                      |
| Validation        | Validate at handler entry; throw domain exception on bad input        |
| Persistence       | Access entities via `IUnitOfWork` — never inject `DbContext` directly |
| Commit            | Call `IUnitOfWork.SaveChangesAsync()` exactly once per handler        |
| Side-effects      | Stage outbox messages — never call `IMessageBus` synchronously        |
| Return value      | Thin result record — never the full domain entity                     |
| Idempotency       | Set `idempotencyKey`; enforce unique constraint on `jobs` table       |
| Transaction scope | One handler = one unit of work; stage writes + outbox, then commit    |

---

## Query Rules

| Rule                   | Detail                                                   |
| ---------------------- | -------------------------------------------------------- |
| Type                   | `record` (immutable)                                     |
| Naming                 | `Get` / `List` + noun: `GetJobById`, `GetJobs`           |
| No mutation            | Never call `SaveChangesAsync` or publish messages        |
| Projection             | Use `.Select()` or Mapperly — never return domain entities |
| Performance            | Always `AsNoTracking()`; consider Dapper for hot queries  |
| Filtering / pagination | Accept `Page`, `PageSize`, filters in the query record   |
| Return value           | Read DTO record or paginated wrapper                     |

---

## Contracts

```csharp
// Commands/ICommand.cs
public interface ICommand<TResult>;

// Commands/ICommandHandler.cs
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

// Queries/IQuery.cs
public interface IQuery<TResult>;

// Queries/IQueryHandler.cs
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
```

---

## Autofac Registration

All handlers are registered via assembly scanning in `CqrsModule`:

```csharp
// Modules/CqrsModule.cs
public class CqrsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var assembly = typeof(CqrsModule).Assembly;

        builder.RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(ICommandHandler<,>))
            .InstancePerLifetimeScope();

        builder.RegisterAssemblyTypes(assembly)
            .AsClosedTypesOf(typeof(IQueryHandler<,>))
            .InstancePerLifetimeScope();
    }
}
```

Register in `Program.cs`:

```csharp
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new DataModule(builder.Configuration));
    container.RegisterModule(new CqrsModule());
});
```

---

## Endpoint Integration

Endpoints are **thin adapters** only — no business logic, no data access:

```csharp
group.MapPost("/documents", async (
    CreateDocumentRequest req,
    ICommandHandler<CreateDocumentCommand, CreateDocumentResult> handler,
    HttpContext ctx,
    CancellationToken cancellationToken) =>
{
    var userId = ctx.User.GetUserId();
    var command = new CreateDocumentCommand(userId, req.TemplateKey, req.TemplateVersion, req.PayloadJson);
    var result = await handler.HandleAsync(command, cancellationToken);
    return TypedResults.Accepted($"/jobs/{result.JobId}", result);
})
.RequireAuthorization()
.WithOpenApi();
```

---

## Anti-Patterns

| Anti-Pattern                                       | Why Forbidden                                      |
| -------------------------------------------------- | -------------------------------------------------- |
| Return domain entity from a query handler          | Couples read model to write model                  |
| Mutate state inside a query handler                | Violates CQRS read/write separation                |
| Put business logic in endpoint delegates           | Endpoints are adapters — logic belongs in handlers |
| Call `SaveChangesAsync` in a query                 | Queries must be side-effect free                   |
| Skip `AsNoTracking()` on query paths               | Unnecessary change tracking overhead               |
| Multiple unrelated commands in one handler class   | One handler per command/query                      |
| Inject `ApplicationDbContext` in a command handler | Commands must go through `IUnitOfWork`             |
| Inject `IUnitOfWork` in a query handler            | Queries are read-only — use `DbContext` directly   |

---

## Templates

- `templates/Command.cs` — scaffold for a new command (command record, result record, handler).
- `templates/Query.cs` — scaffold for a new query (query record, result record, handler).
