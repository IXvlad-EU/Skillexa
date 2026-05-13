---
description: "C# parameter naming conventions — no abbreviations, full descriptive names for all method, constructor, and lambda parameters"
applyTo: "**/*.cs"
---

# Parameter Naming

Use full, descriptive names for every parameter in method signatures, constructors, and lambdas. Never abbreviate.

## Core Principles

- Name parameters after their type or role — `httpContext`, not `ctx`; `cancellationToken`, not `ct`.
- Apply the same rule to lambda parameters unless the lambda is a single-expression framework callback where convention dictates otherwise (e.g., LINQ projections: `user => user.Id`).
- `CancellationToken` parameters must be named `cancellationToken`, placed last, and use `= default`.
- Always use `= default` on optional `CancellationToken` parameters.

## Patterns

### Good Example

```csharp
// Method
public Task<User?> GetByEntraIdAsync(string entraObjectId, CancellationToken cancellationToken = default);

// Endpoint lambda
app.MapPost("/documents", async (
    CreateDocumentRequest request,
    HttpContext httpContext,
    CancellationToken cancellationToken) => { ... });

// Constructor
public UnitOfWork(
    ApplicationDbContext dbContext,
    IJobRepository jobRepository,
    IUserRepository userRepository) { ... }
```

### Bad Example

```csharp
// ❌ abbreviated type names
public Task<User?> GetByEntraIdAsync(string id, CancellationToken ct = default);

// ❌ abbreviated lambda parameters
app.MapPost("/documents", async (CreateDocumentRequest req, HttpContext ctx, CancellationToken ct) => { ... });

// ❌ single-letter or cryptic names
public UnitOfWork(ApplicationDbContext db, IJobRepository j, IUserRepository u) { ... }
```

## Anti-Patterns

| Anti-Pattern                  | Correct Form                    | Why Forbidden                                     |
| ----------------------------- | ------------------------------- | ------------------------------------------------- |
| `CancellationToken ct`        | `CancellationToken cancellationToken` | Abbreviation; inconsistent with .NET guidelines   |
| `HttpContext ctx`             | `HttpContext httpContext`        | `ctx` is ambiguous and non-descriptive            |
| `SomeRequest req`             | `SomeRequest request`           | `req` is an abbreviation of the type name         |
| `IConfiguration cfg`         | `IConfiguration configuration`  | `cfg` is an abbreviation                          |
| `ApplicationDbContext db`     | `ApplicationDbContext dbContext` | `db` is too terse for a constructor parameter     |
| `ILogger log`                 | `ILogger logger`                | `log` omits the noun suffix                       |
| Omitting `= default` on `CancellationToken` | `CancellationToken cancellationToken = default` | Forces callers to always pass a value |
| `CancellationToken` not last  | Place after all other parameters | Violates .NET parameter ordering convention       |

## Exceptions

Short names are acceptable only in these specific contexts:

- **LINQ projection lambdas** where the body references the variable once and the type is obvious from context:
  ```csharp
  db.Jobs.Where(job => job.UserId == userId).Select(job => job.Id)
  ```
- **Framework registration callbacks** (e.g., Serilog host setup) where short names are standard:
  ```csharp
  builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));
  ```
  Even here, prefer readable names (`context`, `loggerConfig`) over abbreviations (`ctx`, `lc`).
