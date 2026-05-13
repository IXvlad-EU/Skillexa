---
description: "High-level Skillexa architecture, solution structure, and async data flow"
applyTo: "**"
---

# Skillexa — Project Overview

## What is Skillexa

Skillexa is a **skill-based job discovery and CV generation platform**. Users enter their skills and preferred job boards → the portal returns matching job listings → clicking "Generate CV" on any listing triggers async PDF generation.

Key architectural point: **the Engine does not call TheirStack**. Job listing data is fetched by Core when the user searches, packaged by the Portal at search time, and included directly in the `GeneratePdf` message payload sent to the Engine.

Generation is **asynchronous**:

1. **Skillexa-Portal** (Next.js SSR + BFF) calls `POST /job-listings/search` to retrieve listings, then `POST /documents` to trigger generation.
2. **Skillexa-Core** (ASP.NET Core Web API) proxies job listing searches to TheirStack, creates a `Document` record, and enqueues a `GeneratePdf` command (with full job data in the payload) to the message broker.
3. **Skillexa-Engine** (.NET Worker Service) consumes the command, extracts job data from the message payload, renders a PDF, stores artefacts in blob storage, and emits status events.
4. Skillexa-Core consumes the status events, updates the document, and exposes status & download URLs to the portal.
5. The user downloads the PDF via a short-lived signed URL served directly from blob storage.

## Solution Structure

| Application         | Path               | Tech                                                                            | Role                                                                                                 |
| ------------------- | ------------------ | ------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| **Skillexa-Portal** | `skillexa-portal/` | Next.js 16, React 19, TypeScript, TanStack Query, Kiota client, Mantine 7, SCSS | SSR + BFF — renders pages server-side, proxies API calls to Skillexa-Core                            |
| **Skillexa-Core**   | `skillexa-core/`   | ASP.NET Core (.NET 10), EF Core, Microsoft Entra ID auth                        | REST API — job management, download URL issuance, broker publishing & event consumption              |
| **Skillexa-Engine** | `skillexa-engine/` | .NET 10 Worker Service                                                          | Background processor — queue consumer, payload extraction, PDF rendering, blob uploads, event emission |

## Portability Goals

- **No hard dependency on any single cloud provider.** The same containers run locally (Docker Compose) or in Azure / AWS / on-prem.
- Infrastructure bindings are swapped via adapter interfaces and environment variables:
  - `IMessageBus` — RabbitMQ (local) ↔ Azure Service Bus (production)
  - `IObjectStorage` — Azurite (local) ↔ Azure Blob Storage (production)
  - PostgreSQL — local container ↔ managed service (Azure Database for PostgreSQL / RDS)
- All environment-specific values (connection strings, API keys, broker endpoints, storage credentials) come from **env vars / secrets** — never hard-coded.

## Async Data Flow (high-level)

```
Portal ──POST /job-listings/search──▸ Core ──▸ TheirStack API ──▸ listings response

Portal ──POST /documents──▸ Core ──enqueue GeneratePdf (payload: job data)──▸ Broker ──▸ Engine
                                                                                              │
                                                                               extract payload data
                                                                               PDF rendering
                                                                               Blob upload
                                                                                              │
Engine ──emit PdfStatusChanged──▸ Broker ──▸ Core ──update Document──▸ DB
                                                                              │
Portal ──GET /documents/{id}──▸ Core ────────────────────────────────────────▸ UI
Portal ──POST /documents/{id}/download-url──▸ Core ──signed URL──▸ Blob Storage
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
2. Proxies job listing search requests to TheirStack (`POST /job-listings/search`).
3. Accepts document creation requests and creates `Document` records in PostgreSQL.
4. Publishes `GeneratePdf` commands (with full job data payload) to the message broker.
5. Consumes `PdfStatusChanged` events from the broker and updates document state.
6. Generates short-lived signed download URLs for completed PDFs.
7. Enforces per-user / per-plan rate limits and provider quotas.

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

| Method   | Path                              | Auth         | Purpose                                                        |
| -------- | --------------------------------- | ------------ | -------------------------------------------------------------- |
| POST     | `/job-listings/search`            | Bearer       | Proxy job listing search to TheirStack; returns listings       |
| POST     | `/documents`                      | Bearer       | Create document → enqueue `GeneratePdf` (with job data payload)|
| GET      | `/documents`                      | Bearer       | List current user's documents                                  |
| GET      | `/documents/{id}`                 | Bearer       | Single document detail (status, error, timestamps)             |
| POST     | `/documents/{id}/download-url`    | Bearer       | Get signed download URL (owner check, status=Succeeded)        |
| GET      | `/app/usage`                      | Bearer       | Provider usage / quota for current user                        |
| POST/PUT | `/admin/templates/*`              | Bearer+Admin | Template CRUD (admin only)                                     |

## Database Access

- Use EF Core with **code-first migrations** (or Dapper for performance-critical reads).
- `ApplicationDbContext` registers entities: `User`, `Document`, `DocumentStatus`, `ProviderUsage`, `Template`, `OutboxMessage`.
- Connection string comes from `ConnectionStrings:DefaultConnection` (env var override in containers).

## Broker Publishing

- Inject `IMessageBus` and call `PublishAsync<GeneratePdf>(command)`.
- The command is published to queue/topic `pdf-generate`.
- Status events are consumed from `pdf-results`.
- See `messaging.instructions.md` for message contracts.

## Signed Download URLs

- Inject `IObjectStorage` and call `GenerateSignedUrlAsync(key, ttl)`.
- URL is read-only, single-object, TTL 5–15 minutes.
- Verify `document.UserId == currentUser` and `document.Status == Succeeded` before generating.

# Skillexa-Engine

## Stack

| Concern        | Choice                                                                     |
| -------------- | -------------------------------------------------------------------------- |
| Framework      | .NET 10 Worker Service (`Host.CreateApplicationBuilder`)                   |
| Hosting        | `BackgroundService` / `IHostedService`                                     |
| Broker         | `IMessageBus` abstraction — RabbitMQ (local) / Azure Service Bus (prod)    |
| Object storage | `IObjectStorage` abstraction — Azurite (local) / Azure Blob Storage (prod) |
| PDF rendering  | PDF library (QuestPDF, iText, Puppeteer — TBD)                             |
| Database       | PostgreSQL (EF Core / Dapper) for templates & quota                        |
| Root namespace | `Skillexa.Engine`                                                          |

## Role

Skillexa-Engine is the **asynchronous processing service**. It:

1. Consumes `GeneratePdf` commands from the `pdf-generate` queue/topic.
2. Sets document status to `Processing` (emits event and/or writes to DB).
3. Extracts job listing data from the `GeneratePdf` message payload.
4. Loads the template (`templateKey` + `templateVersion`) from PostgreSQL.
5. Renders the PDF document.
6. Uploads artefacts to blob storage:
   - `pdf/{documentId}.pdf`
   - `snapshots/{documentId}.json` (input data snapshot)
7. Emits a `PdfStatusChanged` event (`Succeeded` or `Failed`) to the `pdf-results` queue/topic.

## Processing Pipeline

```
1. Receive GeneratePdf message
2. Validate idempotencyKey (skip if already processed)
3. Emit PdfStatusChanged(Processing)
4. Extract job listing data from message payload
5. Load template from DB by templateKey + templateVersion
6. Render PDF from template + job data
7. Upload pdf/{documentId}.pdf to blob storage
8. Upload snapshots/{documentId}.json to blob storage
9. Emit PdfStatusChanged(Succeeded, pdfStorageKey, snapshotStorageKey)
```

## Idempotency

- The `documentId` serves as the natural idempotency key.
- Before processing, check if the document is already in a terminal state (`Succeeded` / `Failed`). If so, skip and acknowledge the message.
- This ensures redelivered messages (broker retry, container restart) do not produce duplicate PDFs.

## Retry & Dead-Letter Strategy

- **Transient errors** (transient DB errors, blob storage timeouts): retry with exponential backoff + jitter (e.g., 1s → 2s → 4s → …, max 5 retries).
- **Permanent failures** (missing template, payload validation errors): mark document as `Failed` immediately, emit the event, and **do not retry**.
- When maximum retries are exhausted, route the message to a **Dead-Letter Queue (DLQ)** for manual inspection.

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
| API client      | **openapi-fetch** typed client — see `api-client.instructions.md` |
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
- [api-client.instructions.md](api-client.instructions.md) — openapi-fetch API client generation and usage
- [nextjs.instructions.md](nextjs.instructions.md) — Next.js best practices and coding standards
- [authentication.instructions.md](authentication.instructions.md) — Microsoft Entra ID auth flow
