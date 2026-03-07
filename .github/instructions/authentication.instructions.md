---
applyTo: "**"
---

# Authentication — Instructions (Microsoft Entra ID)

## Overview

Skillexa uses **Microsoft Entra ID** (formerly Azure AD) as the sole identity provider. There are **no self-issued JWTs, no local password storage, and no custom login/refresh endpoints**. All authentication flows use the **OpenID Connect (OIDC) authorization code flow** with PKCE.

### High-Level Token Flow

```
Browser ──sign-in──▸ Entra ID ──auth code──▸ Portal (BFF)
                                                │
                                    exchange code for tokens
                                    (ID token + access token)
                                                │
Portal ──Bearer access_token──▸ Skillexa-Core (protected API)
```

1. **Skillexa-Portal** (Next.js BFF) acts as a **confidential client** — it initiates the OIDC sign-in, exchanges the authorization code for tokens server-side, and stores the session in an **httpOnly, Secure, SameSite=Strict** cookie. The browser **never** sees raw access tokens.
2. **Skillexa-Core** (ASP.NET Core API) acts as a **protected web API** — it validates the Bearer access token issued by Entra ID on every request. It **never** issues its own tokens.

---

## Entra ID App Registrations

Two app registrations are required in the [Microsoft Entra admin center](https://entra.microsoft.com/):

### 1. `Skillexa-Core-API` (Protected Web API)

| Setting                 | Value                                                                      |
| ----------------------- | -------------------------------------------------------------------------- |
| Supported account types | **Accounts in this organizational directory only** (single-tenant)         |
| Application ID URI      | `api://<Core-Client-ID>` (accept default or customize)                     |
| Expose an API → Scopes  | Add scope: `access_as_user` (admin consent: "Access Skillexa API as user") |
| Token version           | **v2.0** (set `accessTokenAcceptedVersion: 2` in manifest)                 |
| App roles (optional)    | `Admin` role for admin endpoints                                           |

### 2. `Skillexa-Portal-Web` (Confidential Client — BFF)

| Setting                  | Value                                                                    |
| ------------------------ | ------------------------------------------------------------------------ |
| Supported account types  | **Accounts in this organizational directory only** (single-tenant)       |
| Platform                 | **Web**                                                                  |
| Redirect URI             | `http://localhost:3000/api/auth/callback/azure-ad` (dev)                 |
| Front-channel logout URL | `http://localhost:3000/api/auth/signout` (dev)                           |
| Client credentials       | **Client secret** (dev) / **Certificate or federated credential** (prod) |
| API permissions          | Add `Skillexa-Core-API` → `access_as_user` (delegated)                   |
| ID tokens                | Enable under **Implicit grant and hybrid flows** (for OIDC)              |

> **Production redirect URIs** use the real domain with HTTPS, e.g., `https://portal.skillexa.com/api/auth/callback/azure-ad`.

---

## Skillexa-Core — Protected Web API (ASP.NET Core)

### NuGet Package

```
Microsoft.Identity.Web   (latest stable)
```

### Configuration — `appsettings.json`

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

### Code — `Program.cs`

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

### Protecting Endpoints

- Apply `.RequireAuthorization()` on all endpoints **except** health checks and OpenAPI metadata.
- Use policy-based authorization for role-restricted endpoints:

```csharp
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin")); // matches Entra app role
```

### Extracting User Identity

The validated JWT populates `HttpContext.User`. Use standard claims:

| Claim                                        | Purpose                          |
| -------------------------------------------- | -------------------------------- |
| `ClaimTypes.NameIdentifier` (`oid` or `sub`) | Unique user ID (Entra Object ID) |
| `preferred_username`                         | User's email / UPN               |
| `name`                                       | Display name                     |
| `roles`                                      | App roles (e.g., `Admin`)        |

```csharp
var entraObjectId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
var email = context.User.FindFirstValue("preferred_username");
```

### User Provisioning (Just-In-Time)

Since users no longer register locally:

- On the **first authenticated request**, look up the user by `entra_object_id` in the `users` table.
- If not found, **auto-create** a user record (JIT provisioning) using claims from the token (`oid`, `preferred_username`, `name`).

---

## Skillexa-Portal — Confidential Client (Next.js BFF)

### Package

```
next-auth   (v5)
```

Use the **Microsoft Entra ID** provider built into `next-auth`.

### Environment Variables

```env
# .env.local (development)
AUTH_SECRET=<random-32-byte-secret-for-session-encryption>
AUTH_MICROSOFT_ENTRA_ID_ID=<Skillexa-Portal-Web Application (client) ID>
AUTH_MICROSOFT_ENTRA_ID_SECRET=<Skillexa-Portal-Web client secret>
AUTH_MICROSOFT_ENTRA_ID_TENANT_ID=<Directory (tenant) ID>
AUTH_MICROSOFT_ENTRA_ID_ISSUER=https://login.microsoftonline.com/<tenant-id>/v2.0

# Scope to request access token for Skillexa-Core API
AZURE_AD_API_SCOPE=api://<Skillexa-Core-API-Client-ID>/access_as_user
```

### NextAuth Configuration — `auth.ts`

```ts
import NextAuth from "next-auth";
import MicrosoftEntraId from "next-auth/providers/microsoft-entra-id";

export const { handlers, signIn, signOut, auth } = NextAuth({
  providers: [
    MicrosoftEntraId({
      clientId: process.env.AUTH_MICROSOFT_ENTRA_ID_ID!,
      clientSecret: process.env.AUTH_MICROSOFT_ENTRA_ID_SECRET!,
      tenantId: process.env.AUTH_MICROSOFT_ENTRA_ID_TENANT_ID!,
      authorization: {
        params: {
          scope: `openid profile email ${process.env.AZURE_AD_API_SCOPE}`,
        },
      },
    }),
  ],
  callbacks: {
    async jwt({ token, account }) {
      // Persist the access token from the initial sign-in
      if (account) {
        token.accessToken = account.access_token;
        token.expiresAt = account.expires_at;
      }
      return token;
    },
    async session({ session, token }) {
      // Make the access token available in the session (server-side only)
      session.accessToken = token.accessToken as string;
      return session;
    },
  },
  session: { strategy: "jwt" },
});
```

### Route Handler — `app/api/auth/[...nextauth]/route.ts`

```ts
import { handlers } from "@/auth";
export const { GET, POST } = handlers;
```

### Calling Skillexa-Core from Server Components / Route Handlers

```ts
import { auth } from "@/auth";

const session = await auth();
const response = await fetch(`${process.env.CORE_API_URL}/jobs`, {
  headers: {
    Authorization: `Bearer ${session?.accessToken}`,
  },
});
```

When using the **Kiota-generated client**, configure it to attach the access token from the session to every request automatically.

### Sign-In / Sign-Out UI

```tsx
import { signIn, signOut } from "@/auth";

// Sign in — redirects to Entra ID login page
<Button onClick={() => signIn("microsoft-entra-id")}>Sign in</Button>

// Sign out — clears session + redirects to Entra ID logout
<Button onClick={() => signOut()}>Sign out</Button>
```

---

## Token Validation Rules (Skillexa-Core)

`Microsoft.Identity.Web` handles these automatically, but be aware:

1. **Signature** — validated against Entra ID's public signing keys (fetched from the OIDC discovery endpoint).
2. **Issuer** — must match `https://login.microsoftonline.com/{tenant-id}/v2.0`.
3. **Audience** — must match `api://<Core-API client ID>` (the `aud` claim).
4. **Expiry** — tokens past `exp` are rejected.
5. **Token version** — v2.0 tokens (configured via `accessTokenAcceptedVersion: 2` in the app manifest).

Do **not** manually validate tokens or parse JWTs in application code. Rely on `Microsoft.Identity.Web` middleware.
