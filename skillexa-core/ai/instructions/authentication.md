# Skillexa-Core — Portal-Issued JWT Validation

Skillexa-Core does not trust provider tokens from Microsoft or Google directly. It validates only short-lived RS256 JWTs issued by Skillexa-Portal.

## Package

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.*" />
```

Do not use `Microsoft.Identity.Web` in Core.

## Configuration

```jsonc
{
  "JWT": {
    "Issuer": "skillexa-portal",
    "Audience": "skillexa-core",
    "PublicKey": "<RSA public key PEM>"
  }
}
```

Environment variables use .NET configuration binding:

```env
JWT__Issuer=skillexa-portal
JWT__Audience=skillexa-core
JWT__PublicKey="-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----"
```

PEM values may contain literal `\n`; the app normalizes them before importing the key.

## Validation Rules

- Validate issuer, audience, lifetime, signature, and `RS256`.
- Require `sub` and `email` claims.
- Treat `uid` as optional.
- Use `name` as display name when present; fall back to email.

Expected Portal JWT claims:

| Claim | Purpose |
| --- | --- |
| `iss` | `skillexa-portal` |
| `aud` | `skillexa-core` |
| `sub` | Provider-scoped subject, e.g. `google:123` |
| `email` | Normalized verified email |
| `name` | Display name |
| `uid` | Optional Core user ID for DB-free user lookup |

## Protecting Endpoints

- Apply `.RequireAuthorization()` on all Core API endpoints except OpenAPI metadata and health checks.
- Do not leave auth bypasses, `AUTH_REQUIRED` checks, or `// TODO: .RequireAuthorization()` comments in Core.

## User Provisioning

Portal should call `POST /provision` once during initial sign-in with a bootstrap Core JWT that has no `uid` claim. Core returns the Core user ID so Portal can store it in the encrypted NextAuth JWT and include it as `uid` in later Core JWTs.

For user-scoped endpoints, resolve the current Core user ID as follows:

1. If the validated Portal JWT includes a positive numeric `uid`, use it directly.
2. Otherwise, read `email` and `name` from the validated Portal JWT.
3. Normalize email with `Trim().ToLowerInvariant()`.
4. Look up `users.email`.
5. Create a user when no row exists.

The fallback path exists for first-login bootstrap tokens, old sessions, and failed login-time provisioning.

Do not persist provider object IDs in Core. Microsoft and Google accounts with the same verified email map to the same Core user.

## EF Migrations

- Generate migrations with `dotnet ef migrations add <Name>`.
- Do not manually edit generated migration or snapshot files.
- If a generated migration is wrong, remove it with `dotnet ef migrations remove`, fix the model/configuration, and regenerate it.
- If a design-time `DbContext` factory is required, it must load standard configuration (`appsettings`, environment-specific appsettings, environment variables, command-line args). Never hard-code local connection strings in it.
