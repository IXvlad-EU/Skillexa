---
applyTo: "{skillexa-core,skillexa-engine}/**"
---

# Unit of Work & Repository — Instructions

## Overview

Skillexa-Core uses the **Unit of Work (UoW)** and **Repository** patterns to manage data access and transactional boundaries. EF Core's `DbContext` already implements both patterns internally — our abstractions sit on top to provide **business-oriented** interfaces, explicit transaction control, and honest test seams.

```
Command Handler
    │
    ├──▸ IUserRepository.GetByEntraIdAsync(...)
    ├──▸ IJobRepository.AddAsync(...)
    ├──▸ IOutboxRepository.AddAsync(...)      ← outbox message
    │
    └──▸ IUnitOfWork.SaveChangesAsync()       ← single atomic commit
```

- **Repository** = a collection-like abstraction over an **aggregate root**, expressing business operations instead of raw CRUD.
- **Unit of Work** = a transactional boundary that groups all changes across repositories into a **single atomic commit**.

## Core Principles

1. **Only the Unit of Work calls `SaveChangesAsync`.** Repositories **never** call `SaveChangesAsync` — they only stage changes (Add, Remove, attach). The handler decides when to commit.
2. **One repository per aggregate root.** `IJobRepository`, `IUserRepository`, `ITemplateRepository` — not a generic `IRepository<T>`. Child entities (e.g., future `JobAttachment`) are managed through their aggregate root's repository.
3. **Repositories speak business language.** Method names express use cases (`GetByEntraIdAsync`, `GetWithStatusAsync`), not generic CRUD (`Update`, `Delete`).
4. **Query handlers do NOT use UoW.** Queries read directly from `ApplicationDbContext` with `AsNoTracking()`. The UoW is exclusively for **command handlers** that mutate state.
5. **All repositories in a UoW share the same `DbContext` instance.** This is guaranteed by Autofac's `InstancePerLifetimeScope` — one `ApplicationDbContext` per request scope.

## Folder Structure

```
skillexa-core/
  Data/
    ApplicationDbContext.cs
    Repositories/
      Interfaces/              # interfaces only
        IJobRepository.cs
        IUserRepository.cs
        ITemplateRepository.cs
        IOutboxRepository.cs
        IProviderUsageRepository.cs
      Implementations/         # implementations
        JobRepository.cs
        UserRepository.cs
        TemplateRepository.cs
        OutboxRepository.cs
        ProviderUsageRepository.cs
    UnitOfWork/
      Interfaces/              # interfaces only
        IUnitOfWork.cs
      Implementations/         # implementations
        UnitOfWork.cs
    Configurations/            # EF Core entity configurations
    Migrations/                # EF Core migrations
```

## `IUnitOfWork` Interface

```csharp
// Data/UnitOfWork/Interfaces/IUnitOfWork.cs
public interface IUnitOfWork
{
    IJobRepository Jobs { get; }
    IUserRepository Users { get; }
    ITemplateRepository Templates { get; }
    IOutboxRepository OutboxMessages { get; }
    IProviderUsageRepository ProviderUsages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

- Exposes **repository properties** so handlers access all repositories through a single entry point.
- `SaveChangesAsync` delegates to `ApplicationDbContext.SaveChangesAsync()`, committing all staged changes in one transaction.

## `UnitOfWork` Implementation

```csharp
// Data/UnitOfWork/Implementations/UnitOfWork.cs  (implements Data/UnitOfWork/Interfaces/IUnitOfWork.cs)
public sealed class UnitOfWork(
    ApplicationDbContext db,
    IJobRepository jobs,
    IUserRepository users,
    ITemplateRepository templates,
    IOutboxRepository outboxMessages,
    IProviderUsageRepository providerUsages) : IUnitOfWork
{
    public IJobRepository Jobs => jobs;
    public IUserRepository Users => users;
    public ITemplateRepository Templates => templates;
    public IOutboxRepository OutboxMessages => outboxMessages;
    public IProviderUsageRepository ProviderUsages => providerUsages;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => db.SaveChangesAsync(cancellationToken);
}
```

- Constructor-injected repositories all share the same `ApplicationDbContext` (Autofac scoped lifetime).
- No explicit `IDisposable` / `IAsyncDisposable` — the `DbContext` lifetime is managed by the DI container.

## Repository Contracts

### Rules

| Rule                            | Detail                                                         |
| ------------------------------- | -------------------------------------------------------------- |
| **Interface location**          | `Interfaces/` — separate from `Implementations/`               |
| **Naming**                      | `I{AggregateRoot}Repository` → `{AggregateRoot}Repository`     |
| **No `SaveChangesAsync`**       | Repositories stage changes only — UoW commits                  |
| **No `IQueryable<T>` exposure** | Return materialized collections, never `IQueryable`            |
| **No generic base**             | Avoid `IGenericRepository<T>` — use aggregate-specific methods |
| **CancellationToken**           | Last parameter with `default` on all async methods             |
| **Entity type**                 | Domain entities (`IEntity`) only — never DTOs                  |
| **LINQ lambda naming**          | Descriptive names: `job => job.Id`, never `j => j.Id`          |

### Example: `IJobRepository`

```csharp
// Data/Repositories/Interfaces/IJobRepository.cs
public interface IJobRepository
{
    Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Job?> GetByIdForUserAsync(long id, long userId, CancellationToken cancellationToken = default);
    Task AddAsync(Job job, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdempotencyKeyAsync(long idempotencyKey, CancellationToken cancellationToken = default);
}
```

### Example: `JobRepository`

```csharp
// Data/Repositories/Implementations/JobRepository.cs  (implements Data/Repositories/Interfaces/IJobRepository.cs)
public sealed class JobRepository(ApplicationDbContext db) : IJobRepository
{
    public Task<Job?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => db.Jobs.FirstOrDefaultAsync(job => job.Id == id, cancellationToken);

    public Task<Job?> GetByIdForUserAsync(long id, long userId, CancellationToken cancellationToken = default)
        => db.Jobs.FirstOrDefaultAsync(job => job.Id == id && job.UserId == userId, cancellationToken);

    public async Task AddAsync(Job job, CancellationToken cancellationToken = default)
        => await db.Jobs.AddAsync(job, cancellationToken);

    public Task<bool> ExistsByIdempotencyKeyAsync(long idempotencyKey, CancellationToken cancellationToken = default)
        => db.Jobs.AnyAsync(job => job.IdempotencyKey == idempotencyKey, cancellationToken);
}
```

### Example: `IUserRepository`

```csharp
// Data/Repositories/Interfaces/IUserRepository.cs
public interface IUserRepository
{
    Task<User?> GetByEntraIdAsync(string entraObjectId, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
```

## Transactional Outbox Integration

When a command handler writes to the database **and** publishes a broker message, both operations must succeed or fail together. The **transactional outbox** achieves this:

1. The handler adds the domain entity (e.g., `Job`) via repository.
2. The handler adds an `OutboxMessage` via `IOutboxRepository`.
3. The handler calls `IUnitOfWork.SaveChangesAsync()` — both the entity and the outbox message are committed in a **single database transaction**.
4. A background dispatcher reads unpublished outbox messages and publishes them to the broker.

```csharp
// Inside a command handler
await unitOfWork.Jobs.AddAsync(job, cancellationToken);
await unitOfWork.OutboxMessages.AddAsync(new OutboxMessage
{
    Type = "GeneratePdf",
    PayloadJson = JsonSerializer.Serialize(generatePdfMessage),
    CreatedAt = DateTime.UtcNow,
}, cancellationToken);

await unitOfWork.SaveChangesAsync(cancellationToken); // atomic commit
```

This eliminates the dual-write problem (DB commit succeeds but broker publish fails, or vice versa).

## Autofac Registration

Register repositories and the UoW in the `DataModule`:

```csharp
// Modules/DataModule.cs
public class DataModule(IConfiguration configuration) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // ApplicationDbContext is registered via builder.Services.AddDbContext<>() in Program.cs

        // Repositories — assembly scan
        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

        // Unit of Work
        builder.RegisterType<UnitOfWork>()
            .As<IUnitOfWork>()
            .InstancePerLifetimeScope();
    }
}
```

- `InstancePerLifetimeScope` ensures one `DbContext` → one `UnitOfWork` → consistent repositories per HTTP request.

## Usage in Command Handlers

Command handlers receive `IUnitOfWork` (not individual repositories or `ApplicationDbContext`):

```csharp
public class CreateDocumentHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDocumentCommand, CreateDocumentResult>
{
    public async Task<CreateDocumentResult> HandleAsync(
        CreateDocumentCommand command, CancellationToken cancellationToken = default)
    {
        // 1. Check idempotency
        if (await unitOfWork.Jobs.ExistsByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken))
            throw new DuplicateJobException(command.IdempotencyKey);

        // 2. Build domain entity
        var job = new Job { /* ... */ };

        // 3. Stage entity + outbox message
        await unitOfWork.Jobs.AddAsync(job, cancellationToken);
        await unitOfWork.OutboxMessages.AddAsync(new OutboxMessage { /* ... */ }, cancellationToken);

        // 4. Atomic commit
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateDocumentResult(job.Id, "Queued");
    }
}
```

## Usage in Query Handlers

Query handlers **do NOT use `IUnitOfWork`**. They inject `ApplicationDbContext` directly and use `AsNoTracking()` projections:

```csharp
public class GetJobsHandler(ApplicationDbContext db)
    : IQueryHandler<GetJobsQuery, IReadOnlyList<GetJobsResult>>
{
    public async Task<IReadOnlyList<GetJobsResult>> HandleAsync(
        GetJobsQuery query, CancellationToken cancellationToken = default)
    {
        return await db.Jobs
            .AsNoTracking()
            .Where(job => job.UserId == query.UserId)
            .Select(job => new GetJobsResult(/* projection */))
            .ToListAsync(cancellationToken);
    }
}
```

Queries are read-only — no change tracking, no transaction boundary, no UoW overhead.

## What NOT to Do

| Anti-Pattern                                      | Why                                                  |
| ------------------------------------------------- | ---------------------------------------------------- |
| Call `SaveChangesAsync` inside a repository       | UoW loses control of commit timing                   |
| Create `IGenericRepository<T>`                    | Anemic pass-throughs with no aggregate boundaries    |
| Return `IQueryable<T>` from a repository          | Leaks EF Core internals to callers                   |
| Inject `IUnitOfWork` in query handlers            | Queries are read-only — never stage or commit        |
| Inject `ApplicationDbContext` in command handlers | Use `IUnitOfWork` for transactional boundaries       |
| One repository per table                          | Repositories are per aggregate root, not per table   |
| Dispose `IUnitOfWork` manually                    | Lifetime managed by Autofac's scoped container       |
| Access `db.Set<T>()` directly via `IUnitOfWork`   | All entity access must go through repository methods |
