---
applyTo: "**"
---

# Messaging — Instructions

## Broker Abstraction

All broker interactions go through the **`IMessageBus`** interface, which abstracts the underlying transport:

| Environment            | Implementation                      |
| ---------------------- | ----------------------------------- |
| **Local development**  | RabbitMQ (Docker Compose container) |
| **Production (Azure)** | Azure Service Bus                   |

The active implementation is selected at startup based on configuration:

```
Messaging__Provider = "RabbitMQ" | "AzureServiceBus"
Messaging__ConnectionString = "<connection string>"
```

### `IMessageBus` Interface (conceptual)

```csharp
public interface IMessageBus
{
    Task PublishAsync<T>(T message, string destination, CancellationToken ct = default);
    Task SubscribeAsync<T>(string source, Func<T, CancellationToken, Task> handler, CancellationToken ct = default);
}
```

- `destination` / `source` are logical names (e.g., `pdf-generate`, `pdf-results`), mapped to queues/topics/subscriptions by the adapter.
- Adapters handle serialization (JSON), connection management, retries, and dead-letter configuration.

## Queues / Topics

| Logical Name   | Direction     | Purpose                           |
| -------------- | ------------- | --------------------------------- |
| `pdf-generate` | Core → Engine | Carries `GeneratePdf` commands    |
| `pdf-results`  | Engine → Core | Carries `PdfStatusChanged` events |

## Message Contracts

All messages are serialized as JSON. Use `record` types in C#.

### Command: `GeneratePdf`

```jsonc
{
  "messageType": "GeneratePdf",
  "messageVersion": 1,
  "jobId": 123,
  "userId": 456,
  "templateKey": "string",
  "templateVersion": 1,
  "payload": {
    "jobTitle": "...",
    "keywords": ["..."],
    "salaryMin": 0,
    "salaryMax": 0,
    // ...domain fields
  },
  "correlationId": "uuid",
  "idempotencyKey": 123, // recommended: same as jobId
}
```

### Event: `PdfStatusChanged`

```jsonc
{
  "messageType": "PdfStatusChanged",
  "messageVersion": 1,
  "jobId": 123,
  "status": "Processing | Succeeded | Failed",
  "pdfStorageKey": "pdf/{jobId}.pdf", // present when Succeeded
  "snapshotStorageKey": "snapshots/{jobId}.json", // present when available
  "errorCode": "string | null", // present when Failed
  "errorMessage": "string | null", // present when Failed
  "correlationId": "uuid",
}
```

### Event: `ProviderUsageUpdated` (optional)

```jsonc
{
  "messageType": "ProviderUsageUpdated",
  "messageVersion": 1,
  "provider": "theirstack",
  "periodKey": "YYYY-MM-DD",
  "used": 42,
  "remaining": 58,
  "checkedAtUtc": "ISO-8601",
  "correlationId": "uuid",
}
```

## Required Fields on Every Message

- `messageType` — discriminator for deserialization.
- `messageVersion` — allows non-breaking schema evolution.
- `correlationId` — ties related messages together for tracing.
- `idempotencyKey` — (on commands) prevents duplicate processing.

## Adapter Implementation Rules

- **RabbitMQ adapter**: uses `RabbitMQ.Client`. Declare queues/exchanges on startup. Use publisher confirms. Configure DLX (dead-letter exchange) for failed messages.
- **Azure Service Bus adapter**: uses `Azure.Messaging.ServiceBus`. Map logical names to Service Bus queues or topic/subscription pairs. Use `ServiceBusProcessor` for consumption. Configure dead-letter sub-queue.
- Both adapters must:
  - Serialize/deserialize message bodies as UTF-8 JSON.
  - Set `messageType` as a message property/header for filtering.
  - Support graceful shutdown (drain in-flight messages on `CancellationToken`).
  - Log publish/consume events with `correlationId` for observability.

## Configuration

```jsonc
// appsettings.Development.json (RabbitMQ)
{
  "Messaging": {
    "Provider": "RabbitMQ",
    "ConnectionString": "amqp://guest:guest@rabbitmq:5672"
  }
}

// appsettings.json or env vars (Azure Service Bus)
{
  "Messaging": {
    "Provider": "AzureServiceBus",
    "ConnectionString": "<ServiceBus connection string>"
  }
}
```
