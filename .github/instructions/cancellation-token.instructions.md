---
description: "CancellationToken parameter naming convention for all C# code in Skillexa"
applyTo: "**/*.cs"
---

# CancellationToken Naming

Always name `CancellationToken` parameters `cancellationToken`. Never abbreviate to `ct` or any other shorthand.

## Core Principles

- Use `cancellationToken` as the parameter name in every method signature, interface, and lambda.
- The `default` modifier is required on all optional `CancellationToken` parameters.
- Position: always the **last** parameter.

## Patterns

### Good Example

```csharp
public Task<User?> GetByEntraIdAsync(string entraObjectId, CancellationToken cancellationToken = default);

public async Task<IReadOnlyList<SearchJobListingsResult>> HandleAsync(
    SearchJobListingsQuery query, CancellationToken cancellationToken = default)
{
    return await client.SearchAsync(request, cancellationToken);
}
```

### Bad Example

```csharp
// ❌ abbreviated name
public Task<User?> GetByEntraIdAsync(string entraObjectId, CancellationToken ct = default);

// ❌ passed through under abbreviation
var results = await handler.HandleAsync(query, ct);
```

## Anti-Patterns

| Anti-Pattern                          | Why Forbidden                                                                    |
| ------------------------------------- | -------------------------------------------------------------------------------- |
| `CancellationToken ct`                | Abbreviation reduces readability and is inconsistent with .NET naming guidelines |
| `CancellationToken token`             | Non-standard name; use `cancellationToken`                                       |
| Omitting `= default`                  | Forces callers to always pass a value; breaks consistency                        |
| Positional placement other than last  | Violates .NET conventions                                                        |
