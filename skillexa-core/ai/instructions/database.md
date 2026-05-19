# Core Database Schema (high-level)

## Migration Rules

- Generate migrations with `dotnet ef migrations add <Name>`.
- Never manually edit generated migration or model snapshot files.
- If a migration is wrong, use `dotnet ef migrations remove`, correct the EF model/configuration, and regenerate.
- If a design-time `DbContext` factory is required, it must load standard configuration (`appsettings`, environment-specific appsettings, environment variables, command-line args). Never hard-code local connection strings in it.

## `users`

| Column            | Type                 | Notes                              |
| ----------------- | -------------------- | ---------------------------------- |
| `id`              | BIGINT (PK)          | Auto-increment                     |
| `email`           | VARCHAR(256), UNIQUE | Normalized verified email claim    |
| `display_name`    | VARCHAR(256)         | From `name` claim                  |
| `created_at`      | TIMESTAMPTZ          | Default `now() AT TIME ZONE 'UTC'` |
| `updated_at`      | TIMESTAMPTZ          | Default `now() AT TIME ZONE 'UTC'` |

## `document_statuses` (lookup)

| Column | Type                | Notes                                         |
| ------ | ------------------- | --------------------------------------------- |
| `id`   | INT (PK)            | Auto-increment                                |
| `name` | VARCHAR(20), UNIQUE | `Queued`, `Processing`, `Succeeded`, `Failed` |

Seeded on migration with the four known statuses.

## `documents`

| Column                 | Type                         | Notes                                        |
| ---------------------- | ---------------------------- | -------------------------------------------- |
| `id`                   | BIGINT (PK)                  | Auto-increment                               |
| `user_id`              | BIGINT (FK → users)          | ON DELETE RESTRICT                           |
| `status_id`            | INT (FK → document_statuses) | ON DELETE RESTRICT                           |
| `template_key`         | VARCHAR(100)                 |                                              |
| `template_version`     | INT                          | Snapshot of version at creation time         |
| `payload`              | JSONB                        | Input data, default `'{}'::jsonb`            |
| `pdf_storage_key`      | VARCHAR(500)                 | Blob path (nullable until Succeeded)         |
| `snapshot_storage_key` | VARCHAR(500)                 | Blob path (nullable)                         |
| `error_code`           | VARCHAR(100)                 | Nullable                                     |
| `error_message`        | TEXT                         | Nullable                                     |
| `correlation_id`       | BIGINT                       | Ties related messages together for tracing   |
| `idempotency_key`      | BIGINT, UNIQUE               | Prevents duplicate processing (unique index) |
| `created_at`           | TIMESTAMPTZ                  | UTC                                          |
| `updated_at`           | TIMESTAMPTZ                  | UTC                                          |

## `provider_usages`

| Column       | Type        | Notes                                 |
| ------------ | ----------- | ------------------------------------- |
| `id`         | BIGINT (PK) | Auto-increment (implements `IEntity`) |
| `provider`   | VARCHAR(50) | e.g., `theirstack`                    |
| `period_key` | VARCHAR(10) | `YYYY-MM`                             |
| `used`       | INT         |                                       |
| `remaining`  | INT         |                                       |
| `updated_at` | TIMESTAMPTZ |                                       |

Unique index on `(provider, period_key)`.

## `templates`

| Column         | Type          | Notes                           |
| -------------- | ------------- | ------------------------------- |
| `id`           | BIGINT (PK)   | Auto-increment                  |
| `template_key` | VARCHAR(100)  |                                 |
| `version`      | INT           | Auto-incremented per key        |
| `is_active`    | BOOLEAN       | Only one active version per key |
| `content`      | TEXT          | Template body                   |
| `updated_at`   | TIMESTAMPTZ   |                                 |

## `outbox_messages` (optional — transactional outbox)

| Column         | Type         | Notes                 |
| -------------- | ------------ | --------------------- |
| `id`           | BIGINT (PK)  | Auto-increment        |
| `type`         | VARCHAR(100) | Message type          |
| `payload_json` | JSONB        |                       |
| `created_at`   | TIMESTAMPTZ  |                       |
| `published_at` | TIMESTAMPTZ  | Null until dispatched |
