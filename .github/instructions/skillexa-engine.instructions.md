---
applyTo: "skillexa-engine/**"
---

# Skillexa-Engine — Instructions

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

## Project Layout (recommended)

```
skillexa-engine/
  Program.cs
  Worker.cs              # main BackgroundService (or split per consumer)
  appsettings.json / appsettings.Development.json
  Consumers/             # message handlers
    GeneratePdfConsumer.cs
  Services/
    PdfRenderingService.cs
    TheirStackClient.cs
    QuotaService.cs
  Messaging/             # IMessageBus, contracts (shared or referenced)
  Storage/               # IObjectStorage adapter
  Data/                  # DbContext / repositories for templates & quota
  Templates/             # template loading logic
```

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

## Environment Variables

| Variable                               | Purpose                                 |
| -------------------------------------- | --------------------------------------- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string            |
| `Messaging__Provider`                  | `RabbitMQ` or `AzureServiceBus`         |
| `Messaging__ConnectionString`          | Broker connection string                |
| `Storage__Provider`                    | `Azurite` or `AzureBlobStorage`         |
| `Storage__ConnectionString`            | Storage connection string               |
| `Storage__ContainerName`               | Blob container name                     |
| `TheirStack__ApiKey`                   | TheirStack API key                      |
| `TheirStack__BaseUrl`                  | TheirStack API base URL                 |
| `TheirStack__DailyLimit`               | Max daily calls (fallback if not in DB) |

## Coding Standards

- Nullable reference types enabled.
- Use `record` types for message contracts.
- Keep the `Worker` class thin — delegate all logic to injected services.
- Each consumer should be a single-responsibility class.
- Log structured data (`ILogger` with named placeholders) at appropriate levels.
- Never swallow exceptions silently — log and emit a `Failed` event.
- **Always use block bodies `{ }` for methods** — never expression-bodied members (`=>`). This applies to all methods including single-expression ones.
