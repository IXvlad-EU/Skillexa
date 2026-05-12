---
description: "Autofac DI module conventions for Skillexa-Core and Skillexa-Engine — how to define, register, and wire new dependencies using RegisterModule"
applyTo: "{skillexa-core,skillexa-engine}/**/*.cs"
---

# Autofac DI Modules

All dependency registrations go through **Autofac `Module` subclasses**. Never register application services directly in `Program.cs` (except `AddDbContext`, `AddAuthentication`, and typed `HttpClient` — those use `IServiceCollection` because they are framework-owned).

## Core Principles

- Every logical concern gets its own `Module` class inside `{project}/Modules/`.
- All modules are wired in `Program.cs` via `builder.Host.ConfigureContainer<ContainerBuilder>`.
- Use `RegisterModule(new XyzModule(...))` — never call `builder.RegisterType<T>()` directly in `Program.cs`.
- Pass `IConfiguration` (or other host-level objects) into a module via its constructor when it needs config values.
- Prefer assembly scanning (`RegisterAssemblyTypes`) over explicit per-type registrations where a naming convention exists.
- Always specify an explicit lifetime: `InstancePerLifetimeScope` (default for request-scoped), `SingleInstance`, or `InstancePerDependency`.

## Module Structure

```csharp
// {Project}/Modules/XyzModule.cs
using Autofac;

namespace Skillexa.Core.Modules;

public class XyzModule(IConfiguration configuration) : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // explicit single registration
        builder.RegisterType<XyzService>()
            .As<IXyzService>()
            .InstancePerLifetimeScope();

        // assembly scan by naming convention
        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(type => type.Name.EndsWith("Handler", StringComparison.Ordinal))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
    }
}
```

## Wiring in Program.cs

```csharp
builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterModule(new DataModule());
    container.RegisterModule(new CqrsModule());
    container.RegisterModule(new MessagingModule(builder.Configuration));
    container.RegisterModule(new StorageModule(builder.Configuration));
});
```

- Add new modules here. Never scatter `builder.RegisterType<T>()` calls outside this block.
- Commented-out `RegisterModule` lines (e.g. `// container.RegisterModule(new MessagingModule(...))`) are placeholders — uncomment when the module is implemented.

## Existing Modules

| Module            | Project      | Concern                                                                        |
| ----------------- | ------------ | ------------------------------------------------------------------------------ |
| `DataModule`      | Core, Engine | Repositories + `IUnitOfWork` (assembly scan + explicit UoW)                    |
| `CqrsModule`      | Core, Engine | `ICommandHandler<,>` + `IQueryHandler<,>` (assembly scan via closed generics)  |
| `MessagingModule` | Core, Engine | `IMessageBus` — RabbitMQ or Azure Service Bus (placeholder)                    |
| `StorageModule`   | Core, Engine | `IObjectStorage` — Azurite or Azure Blob Storage (placeholder)                 |

## Assembly Scan Patterns

```csharp
// By name suffix
builder.RegisterAssemblyTypes(ThisAssembly)
    .Where(type => type.Name.EndsWith("Repository", StringComparison.Ordinal))
    .AsImplementedInterfaces()
    .InstancePerLifetimeScope();

// By closed generic interface
builder.RegisterAssemblyTypes(ThisAssembly)
    .AsClosedTypesOf(typeof(ICommandHandler<,>))
    .InstancePerLifetimeScope();
```

## IServiceCollection vs Autofac

Use `builder.Services` (MS DI) only for:
- `AddDbContext<T>` — EF Core requires it.
- `AddAuthentication` / `AddMicrosoftIdentityWebApi` — Microsoft.Identity.Web requires it.
- `AddHttpClient<TClient, TImpl>` — `IHttpClientFactory` lifecycle is owned by MS DI.
- `AddOpenApi` / framework middleware.

Everything else (handlers, repositories, services, adapters) goes in an Autofac module.

## Anti-Patterns

| Anti-Pattern                                                           | Why Forbidden                                                                      |
| ---------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| `builder.RegisterType<T>()` directly in `Program.cs`                   | Bypasses module boundaries; scatters registrations                                 |
| Registering services in `builder.Services` when Autofac is available   | Inconsistent container; Autofac won't see it via `AsImplementedInterfaces` scans   |
| One module per class                                                   | Creates noise; group by concern, not by type                                       |
| Missing lifetime specifier                                             | Defaults to `InstancePerDependency` (transient) — often wrong for DB-touching services |
| Passing `HttpClient` into a module constructor                         | `IHttpClientFactory` is MS DI–owned; register typed clients with `AddHttpClient` in `Program.cs` |
