# Skillexa-Core — Protected Web API (ASP.NET Core)

## NuGet Package

```
Microsoft.Identity.Web   (3.* in `Skillexa.Core.csproj`)
```

## Configuration — `appsettings.json`

```jsonc
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "<Directory (tenant) ID>",
    "ClientId": "<Skillexa-Core-API Application (client) ID>",
    "Audience": "api://<Skillexa-Core-API Application (client) ID>",
  },
}
```

- **`TenantId`**: Use the actual tenant GUID for single-tenant apps. Use `"organizations"` only if multi-tenant is needed.
- **`ClientId`**: The Application (client) ID from the `Skillexa-Core-API` registration.
- **`Audience`**: Must match the Application ID URI configured when exposing the API scope.
- In production, source `TenantId` and `ClientId` from **environment variables or a secrets manager** — never hard-code.

## Code — `Program.cs`

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// ── Authentication ─────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// ... other service registrations (EF Core, Autofac, OpenAPI, etc.)

var app = builder.Build();

// ── Middleware pipeline ────────────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ... endpoints
app.Run();
```

## Protecting Endpoints

> **Development note:** Entra ID app registrations are not yet configured. During active development, authorization is applied as `// TODO: .RequireAuthorization()` comments and endpoints are temporarily public. Re-enable `.RequireAuthorization()` once registrations are set up.

- Apply `.RequireAuthorization()` on all endpoints **except** health checks and OpenAPI metadata.
- Use policy-based authorization for role-restricted endpoints:

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin")); // matches Entra app role
```

## Extracting User Identity

The validated JWT populates `HttpContext.User`. Use standard claims:

| Claim                                          | Purpose                          |
| ---------------------------------------------- | -------------------------------- |
| `ClaimTypes.NameIdentifier` (`oid` or `sub`)   | Unique user ID (Entra Object ID) |
| `preferred_username`                           | User's email / UPN               |
| `name`                                         | Display name                     |
| `roles`                                        | App roles (e.g., `Admin`)        |

```csharp
var entraObjectId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
var email = context.User.FindFirstValue("preferred_username");
```

## User Provisioning (Just-In-Time)

Since users no longer register locally:

- On the **first authenticated request**, look up the user by `entra_object_id` in the `users` table.
- If not found, **auto-create** a user record (JIT provisioning) using claims from the token (`oid`, `preferred_username`, `name`).
