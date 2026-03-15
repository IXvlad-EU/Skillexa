---
description: "High-level Skillexa architecture, solution structure, and async data flow"
applyTo: "**"
---

# Skillexa — Project Overview

## What is Skillexa

Skillexa is a web platform that aggregates job data from external providers (TheirStack) and generates tailored CV/resume PDF documents on user request. Generation is **asynchronous**:

1. **Skillexa-Portal** (Next.js SSR + BFF) sends requests to the backend API.
2. **Skillexa-Core** (ASP.NET Core Web API) creates a `Job` record, enqueues a `GeneratePdf` command to the message broker.
3. **Skillexa-Engine** (.NET Worker Service) consumes the command, calls TheirStack, renders a PDF, stores artefacts in blob storage, and emits status events.
4. Skillexa-Core consumes the status events, updates the job, and exposes status & download URLs to the portal.
5. The user downloads the PDF via a short-lived signed URL served directly from blob storage.

## Solution Structure

| Application         | Path               | Tech                                                                            | Role                                                                                                 |
| ------------------- | ------------------ | ------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| **Skillexa-Portal** | `skillexa-portal/` | Next.js 16, React 19, TypeScript, TanStack Query, Kiota client, Mantine 7, SCSS | SSR + BFF — renders pages server-side, proxies API calls to Skillexa-Core                            |
| **Skillexa-Core**   | `skillexa-core/`   | ASP.NET Core (.NET 10), EF Core, Microsoft Entra ID auth                        | REST API — job management, download URL issuance, broker publishing & event consumption              |
| **Skillexa-Engine** | `skillexa-engine/` | .NET 10 Worker Service                                                          | Background processor — queue consumer, TheirStack calls, PDF rendering, blob uploads, event emission |

## Portability Goals

- **No hard dependency on any single cloud provider.** The same containers run locally (Docker Compose) or in Azure / AWS / on-prem.
- Infrastructure bindings are swapped via adapter interfaces and environment variables:
  - `IMessageBus` — RabbitMQ (local) ↔ Azure Service Bus (production)
  - `IObjectStorage` — Azurite (local) ↔ Azure Blob Storage (production)
  - PostgreSQL — local container ↔ managed service (Azure Database for PostgreSQL / RDS)
- All environment-specific values (connection strings, API keys, broker endpoints, storage credentials) come from **env vars / secrets** — never hard-coded.

## Async Data Flow (high-level)

```
Portal ──POST /documents──▸ Core ──enqueue GeneratePdf──▸ Broker ──▸ Engine
                                                                       │
                                                                       ▼
                                                              TheirStack API
                                                              PDF rendering
                                                              Blob upload
                                                                       │
Engine ──emit PdfStatusChanged──▸ Broker ──▸ Core ──update Job──▸ DB
                                                                       │
Portal ──GET /jobs/{id}──▸ Core ──────────────────────────────────────▸ UI
Portal ──POST /jobs/{id}/download-url──▸ Core ──signed URL──▸ Blob Storage
```

# Skillexa-Core

## Stack

| Concern        | Choice                                                                                        |
| -------------- | --------------------------------------------------------------------------------------------- |
| Framework      | ASP.NET Core Web API (.NET 10)                                                                |
| Auth           | Microsoft Entra ID (JWT Bearer via `Microsoft.Identity.Web`)                                  |
| DI container   | **Autofac** (replaces built-in DI)                                                            |
| ORM / Data     | EF Core (or Dapper) with PostgreSQL                                                           |
| Mapping        | **Mapperly** (source-generated DTO ↔ Entity mapping)                                          |
| Broker         | `IMessageBus` abstraction — RabbitMQ (local) / Azure Service Bus (prod)                       |
| Object storage | `IObjectStorage` abstraction — Azurite (local) / Azure Blob Storage (prod)                    |
| API docs       | OpenAPI spec (built-in `Microsoft.AspNetCore.OpenApi`) — consumed by Kiota in Skillexa-Portal |
| Root namespace | `Skillexa.Core`                                                                               |

## Role

Skillexa-Core is the **HTTP API and orchestration layer**. It:

1. Validates Entra ID Bearer tokens and provisions users (JIT).
2. Accepts document creation requests and creates `Job` records in PostgreSQL.
3. Publishes `GeneratePdf` commands to the message broker.
4. Consumes `PdfStatusChanged` events from the broker and updates job state.
5. Generates short-lived signed download URLs for completed PDFs.
6. Enforces per-user / per-plan rate limits and provider quotas.

## Authentication

- Skillexa-Core is a **protected web API** — it validates Bearer tokens issued by **Microsoft Entra ID**.
- Uses `Microsoft.Identity.Web` (`AddMicrosoftIdentityWebApi`) for JWT validation.
- No self-issued tokens, no local password storage, no login/refresh endpoints.
- All endpoints (except health checks and OpenAPI metadata) require `.RequireAuthorization()`.
- See `authentication.instructions.md` for full Entra ID configuration details.

## OpenAPI Spec

- The API **must** expose a valid OpenAPI 3.x specification at `/openapi/v1.json` (built-in `Microsoft.AspNetCore.OpenApi`).
- This spec is the **contract** that Skillexa-Portal's Kiota client is generated from.
- Keep DTOs and route metadata accurate — any breaking change requires regenerating the Portal client.
- Use `.WithOpenApi()`, `[EndpointSummary]`, and `[EndpointDescription]` on endpoint definitions for metadata.

## Endpoint Catalog

| Method   | Path                         | Auth         | Purpose                                                 |
| -------- | ---------------------------- | ------------ | ------------------------------------------------------- |
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

# Skillexa-Engine

## Stack

| Concern           | Choice                                                                     |
| ----------------- | -------------------------------------------------------------------------- |
| Framework         | .NET 10 Worker Service (`Host.CreateApplicationBuilder`)                   |
| Hosting           | `BackgroundService` / `IHostedService`                                     |
| Broker            | `IMessageBus` abstraction — RabbitMQ (local) / Azure Service Bus (prod)    |
| Object storage    | `IObjectStorage` abstraction — Azurite (local) / Azure Blob Storage (prod) |
| External provider | TheirStack API (REST/HTTP)                                                 |
| PDF rendering     | PDF library (QuestPDF, iText, Puppeteer — TBD)                             |
| Database          | PostgreSQL (EF Core / Dapper) for templates & quota                        |
| Root namespace    | `Skillexa.Engine`                                                          |

## Role

Skillexa-Engine is the **asynchronous processing service**. It:

1. Consumes `GeneratePdf` commands from the `pdf-generate` queue/topic.
2. Sets job status to `Processing` (emits event and/or writes to DB).
3. Enforces provider quota (atomic DB check/decrement for TheirStack daily limits).
4. Calls the TheirStack API to fetch job data.
5. Loads the template (`templateKey` + `templateVersion`) from PostgreSQL.
6. Renders the PDF document.
7. Uploads artefacts to blob storage:
   - `pdf/{jobId}.pdf`
   - `snapshots/{jobId}.json` (input data snapshot)
8. Emits a `PdfStatusChanged` event (`Succeeded` or `Failed`) to the `pdf-results` queue/topic.

## Processing Pipeline

```
1. Receive GeneratePdf message
2. Validate idempotencyKey (skip if already processed)
3. Emit PdfStatusChanged(Processing)
4. Check / decrement provider quota (atomic DB operation)
   └─ If exhausted → emit PdfStatusChanged(Failed, errorCode="QuotaExceeded") → stop
5. Call TheirStack API
   └─ On transient error (429/5xx) → retry with exponential backoff + jitter
   └─ On permanent error → emit PdfStatusChanged(Failed) → route to DLQ
6. Load template from DB by templateKey + templateVersion
7. Render PDF from template + provider data
8. Upload pdf/{jobId}.pdf to blob storage
9. Upload snapshots/{jobId}.json to blob storage
10. Emit PdfStatusChanged(Succeeded, pdfStorageKey, snapshotStorageKey)
```

## Idempotency

- The `jobId` serves as the natural idempotency key.
- Before processing, check if the job is already in a terminal state (`Succeeded` / `Failed`). If so, skip and acknowledge the message.
- This ensures redelivered messages (broker retry, container restart) do not produce duplicate PDFs or double-decrement quotas.

## Retry & Dead-Letter Strategy

- **Transient errors** (HTTP 429, 5xx from TheirStack, transient DB errors): retry with exponential backoff + jitter (e.g., 1s → 2s → 4s → …, max 5 retries).
- **Permanent failures** (4xx from TheirStack, missing template, validation errors): mark job as `Failed` immediately, emit the event, and **do not retry**.
- When maximum retries are exhausted, route the message to a **Dead-Letter Queue (DLQ)** for manual inspection.

## Provider Quota

- TheirStack has a daily API call limit.
- Before each call, atomically `UPDATE provider_quota SET used = used + 1 WHERE provider = 'theirstack' AND day_key = @today AND used < limit` — if rows affected = 0, quota is exceeded.
- Quota state lives in PostgreSQL (`provider_quota` table).

# Skillexa-Portal

## Stack

| Concern         | Choice                                                              |
| --------------- | ------------------------------------------------------------------- |
| Framework       | Next.js 16 (App Router, SSR)                                        |
| Language        | TypeScript (strict mode)                                            |
| React           | 19.x                                                                |
| UI library      | **Mantine 7** — see `mantine-ui.instructions.md`                    |
| Styling         | Mantine CSS modules + PostCSS                                       |
| Data fetching   | TanStack Query — see `tanstack-query.instructions.md`               |
| API client      | **Kiota**-generated TypeScript client — see `kiota.instructions.md` |
| Output          | `standalone` (Docker-friendly)                                      |
| Package manager | pnpm (workspace)                                                    |

---

## Architecture: SSR + BFF

- The portal uses **Server-Side Rendering** via the Next.js App Router.
- It also acts as a **Backend-for-Frontend (BFF)**: the browser never talks directly to Skillexa-Core. All API calls go through Next.js Route Handlers or Server Actions, which forward them to Skillexa-Core internally (server-to-server).
- Benefits: no CORS issues, JWT tokens stay on the server side (httpOnly cookies), reduced client bundle.

## Auth Flow

- Sign-in: user clicks "Sign in" → `next-auth` (v4) redirects to **Microsoft Entra ID** OIDC login → Entra ID returns auth code → `next-auth` exchanges it for tokens server-side → session stored in an **httpOnly, Secure, SameSite=Strict** cookie.
- Subsequent requests: the BFF reads the session via `getServerSession(authOptions)`, attaches the Entra ID access token as a `Bearer` header when calling Core.
- Token refresh: `next-auth` handles token refresh with Entra ID silently.
- Client components use `useSession()` from `next-auth/react` for auth state (user name, loading status). The app is wrapped in `<SessionProvider>` inside `app/providers.tsx`.
- Client components never see or handle raw access tokens.
- See `authentication.instructions.md` for full Entra ID configuration.

## Related Instructions

- [mantine-ui.instructions.md](mantine-ui.instructions.md) — Mantine 7 setup, components, and styling
- [tanstack-query.instructions.md](tanstack-query.instructions.md) — TanStack Query hooks and SSR prefetching
- [kiota.instructions.md](kiota.instructions.md) — Kiota API client generation and usage
- [nextjs.instructions.md](nextjs.instructions.md) — Next.js best practices and coding standards
- [authentication.instructions.md](authentication.instructions.md) — Microsoft Entra ID auth flow
