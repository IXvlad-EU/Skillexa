---
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

| Application | Path | Tech | Role |
|---|---|---|---|
| **Skillexa-Portal** | `skillexa-portal/` | Next.js 16, React 19, TypeScript, TanStack Query, Kiota client, Tailwind CSS 4 | SSR + BFF — renders pages server-side, proxies API calls to Skillexa-Core |
| **Skillexa-Core** | `skillexa-core/` | ASP.NET Core (.NET 10), EF Core, JWT auth | REST API — auth, job management, download URL issuance, broker publishing & event consumption |
| **Skillexa-Engine** | `skillexa-engine/` | .NET 10 Worker Service | Background processor — queue consumer, TheirStack calls, PDF rendering, blob uploads, event emission |

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
