# Skillexa — Project Overview

## What is Skillexa

Skillexa is a **skill-based job discovery and CV generation platform**. Users enter their skills and preferred job boards, the portal returns matching job listings, and clicking "Generate CV" creates a document workflow.

Key architectural point: **TheirStack searches happen in Core**. The Engine should process document-generation payloads; it should not perform job-listing searches.

Current implementation status:

1. **Skillexa-Portal** (Next.js SSR + BFF) calls BFF route handlers, which call Core through an `openapi-fetch` client generated from Core OpenAPI types.
2. **Skillexa-Core** proxies job-listing searches to TheirStack, persists `Document` records, and stages `GeneratePdf` payloads in `outbox_messages`.
3. **Skillexa-Engine** contains CQRS/database scaffolding and a `ProcessGeneratePdf` handler with quota/template checks. Broker consumption, PDF rendering, blob upload, and status-event publishing are still placeholders.
4. Download URL generation currently validates document ownership/status and returns a placeholder storage URL until the object storage adapter is implemented.

## Solution Structure

| Application         | Path               | Tech                                                                            | Role                                                                                                 |
| ------------------- | ------------------ | ------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| **Skillexa-Portal** | `skillexa-portal/` | Next.js 16, React 19, TypeScript, TanStack Query, openapi-fetch, Mantine 7, SCSS | SSR + BFF — renders pages server-side, proxies API calls to Skillexa-Core                            |
| **Skillexa-Core**   | `skillexa-core/`   | ASP.NET Core (.NET 10), EF Core, Microsoft Entra ID auth                        | REST API — job search proxy, document persistence, outbox staging, placeholder download URLs         |
| **Skillexa-Engine** | `skillexa-engine/` | .NET 10 Worker Service                                                          | Background processor scaffold — quota/template checks; broker/PDF/blob/status integration pending    |

## Portability Goals

- **No hard dependency on any single cloud provider.** The same containers run locally (Docker Compose) or in Azure / AWS / on-prem.
- Infrastructure bindings should be swapped via adapter interfaces and environment variables as those adapters are implemented:
  - Messaging — RabbitMQ is configured locally; Service Bus is not implemented yet.
  - Object storage — blob storage adapters are not implemented yet.
  - PostgreSQL — local container ↔ managed service (Azure Database for PostgreSQL / RDS)
- All environment-specific values (connection strings, API keys, broker endpoints, storage credentials) come from **env vars / secrets** — never hard-coded.

## Async Data Flow (high-level)

```
Portal ──POST /job-listings/search──▸ Core ──▸ TheirStack API ──▸ listings response

Portal ──POST /documents──▸ Core ──create Document + outbox GeneratePdf payload──▸ DB

Planned async path:

Outbox dispatcher ──GeneratePdf──▸ Broker ──▸ Engine ──PDF/blob/status event──▸ Core
                                                                              │
Portal ──GET /documents/{id}──▸ Core ────────────────────────────────────────▸ UI
Portal ──POST /documents/{id}/download-url──▸ Core ──placeholder URL until storage adapter exists
```

## Related Instructions

- `skillexa-core/ai`
- `skillexa-engine/ai`
- `skillexa-portal/ai`
