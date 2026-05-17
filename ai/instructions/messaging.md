# Messaging — Instructions

## Broker Abstraction

Messaging is configured for RabbitMQ in Docker Compose and appsettings, and both .NET projects reference `RabbitMQ.Client`.

The actual broker abstraction, dispatcher, consumers, and Azure Service Bus adapter are **not implemented yet**. When adding them, keep broker-specific code behind a local adapter interface instead of coupling handlers to RabbitMQ directly.

| Environment            | Intended implementation             |
| ---------------------- | ----------------------------------- |
| **Local development**  | RabbitMQ (Docker Compose container) |
| **Production (Azure)** | Azure Service Bus (not implemented) |

The intended provider selection is configuration-based:

```
Messaging__Provider = "RabbitMQ" | "AzureServiceBus"
Messaging__ConnectionString = "<connection string>"
```

### Adapter Interface Shape

```csharp
public interface IMessageBus
{
    Task PublishAsync<T>(T message, string destination, CancellationToken cancellationToken = default);
    Task SubscribeAsync<T>(string source, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default);
}
```

- `destination` / `source` are logical names (e.g., `pdf-generate`, `pdf-results`), mapped to queues/topics/subscriptions by the adapter.
- Adapters should handle serialization (JSON), connection management, retries, and dead-letter configuration.

## Queues / Topics

| Logical Name   | Direction     | Purpose                           |
| -------------- | ------------- | --------------------------------- |
| `pdf-generate` | Core → Engine | Planned queue for `GeneratePdf` commands    |
| `pdf-results`  | Engine → Core | Planned queue for `PdfStatusChanged` events |

## Message Contracts

Messages should be serialized as JSON. Use `record` types in C# for message contracts.

### Command: `GeneratePdf`

```jsonc
{
  "messageType": "GeneratePdf",
  "messageVersion": 1,
  "documentId": 123,
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
  "correlationId": 789,
  "idempotencyKey": 123
}
```

### Event: `PdfStatusChanged`

```jsonc
{
  "messageType": "PdfStatusChanged",
  "messageVersion": 1,
  "documentId": 123,
  "status": "Processing | Succeeded | Failed",
  "pdfStorageKey": "pdf/{documentId}.pdf", // present when Succeeded
  "snapshotStorageKey": "snapshots/{documentId}.json", // present when available
  "errorCode": "string | null", // present when Failed
  "errorMessage": "string | null", // present when Failed
  "correlationId": 789,
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
  "correlationId": 789,
}
```

## Required Fields on Every Message

- `messageType` — discriminator for deserialization.
- `messageVersion` — allows non-breaking schema evolution.
- `correlationId` — ties related messages together for tracing.
- `idempotencyKey` — (on commands) prevents duplicate processing.

## Adapter Implementation Rules

- **RabbitMQ adapter**: use `RabbitMQ.Client`. Declare queues/exchanges on startup. Use publisher confirms. Configure DLX (dead-letter exchange) for failed messages.
- **Azure Service Bus adapter**: use `Azure.Messaging.ServiceBus` if/when production Service Bus support is added. Map logical names to Service Bus queues or topic/subscription pairs. Use `ServiceBusProcessor` for consumption. Configure dead-letter sub-queue.
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
