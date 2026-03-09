---
description: "PostgreSQL schema, EF Core migrations, and database conventions"
applyTo: "**/*.sql,**/Migrations/**,**/Data/**"
---

# Database — Instructions

## Engine

- **PostgreSQL** is the relational data store for Skillexa.
- Local development: single PostgreSQL container in Docker Compose, hosting **two databases**.
- Production: managed service (Azure Database for PostgreSQL / AWS RDS).

## Database Topology

| Database          | Owner Service   | Tables                                                                             |
| ----------------- | --------------- | ---------------------------------------------------------------------------------- |
| `skillexa` (Core) | Skillexa-Core   | `users`, `jobs`, `job_statuses`, `templates`, `provider_usages`, `outbox_messages` |
| `skillexa_engine` | Skillexa-Engine | `templates`, `provider_quotas`                                                     |

- The second database (`skillexa_engine`) must be created manually on first setup. Run `CREATE DATABASE skillexa_engine;` against the PostgreSQL instance.
- **Important:** If you already have a `postgres-data` volume and need to re-initialize, run `docker compose down -v` to destroy it.
- In development mode, each service calls `db.Database.MigrateAsync()` at startup to apply pending EF Core migrations automatically.
- `templates` exists in both databases — Core manages them (admin CRUD), Engine has a separate copy (manually synced).
- `provider_quotas` is exclusive to Engine (atomic decrement for TheirStack daily limits).

## Core Database Schema (high-level)

### `users`

| Column            | Type                 | Notes                              |
| ----------------- | -------------------- | ---------------------------------- |
| `id`              | BIGINT (PK)          | Auto-increment                     |
| `entra_object_id` | VARCHAR(36), UNIQUE  | Entra ID Object ID (`oid` claim)   |
| `email`           | VARCHAR(256), UNIQUE | From `preferred_username` claim    |
| `display_name`    | VARCHAR(256)         | From `name` claim                  |
| `created_at`      | TIMESTAMPTZ          | Default `now() AT TIME ZONE 'UTC'` |
| `updated_at`      | TIMESTAMPTZ          | Default `now() AT TIME ZONE 'UTC'` |

### `job_statuses` (lookup)

| Column | Type                | Notes                                         |
| ------ | ------------------- | --------------------------------------------- |
| `id`   | INT (PK)            | Auto-increment                                |
| `name` | VARCHAR(20), UNIQUE | `Queued`, `Processing`, `Succeeded`, `Failed` |

Seeded on migration with the four known statuses.

### `jobs`

| Column                 | Type                    | Notes                                        |
| ---------------------- | ----------------------- | -------------------------------------------- |
| `id`                   | BIGINT (PK)             | Auto-increment                               |
| `user_id`              | BIGINT (FK → users)     | ON DELETE RESTRICT                           |
| `status_id`            | INT (FK → job_statuses) | ON DELETE RESTRICT                           |
| `template_key`         | VARCHAR(100)            |                                              |
| `template_version`     | INT                     | Snapshot of version at creation time         |
| `payload`              | JSONB                   | Input data, default `'{}'::jsonb`            |
| `pdf_storage_key`      | VARCHAR(500)            | Blob path (nullable until Succeeded)         |
| `snapshot_storage_key` | VARCHAR(500)            | Blob path (nullable)                         |
| `error_code`           | VARCHAR(100)            | Nullable                                     |
| `error_message`        | TEXT                    | Nullable                                     |
| `correlation_id`       | BIGINT                  | Ties related messages together for tracing   |
| `idempotency_key`      | BIGINT, UNIQUE          | Prevents duplicate processing (unique index) |
| `created_at`           | TIMESTAMPTZ             | UTC                                          |
| `updated_at`           | TIMESTAMPTZ             | UTC                                          |

### `provider_usages`

| Column       | Type        | Notes                                 |
| ------------ | ----------- | ------------------------------------- |
| `id`         | BIGINT (PK) | Auto-increment (implements `IEntity`) |
| `provider`   | VARCHAR(50) | e.g., `theirstack`                    |
| `period_key` | VARCHAR(10) | `YYYY-MM`                             |
| `used`       | INT         |                                       |
| `remaining`  | INT         |                                       |
| `updated_at` | TIMESTAMPTZ |                                       |

Unique index on `(provider, period_key)`.

### `templates`

| Column         | Type          | Notes                           |
| -------------- | ------------- | ------------------------------- |
| `id`           | BIGINT (PK)   | Auto-increment                  |
| `template_key` | VARCHAR(100)  |                                 |
| `version`      | INT           | Auto-incremented per key        |
| `is_active`    | BOOLEAN       | Only one active version per key |
| `content`      | JSONB or TEXT | Template body                   |
| `updated_at`   | TIMESTAMPTZ   |                                 |

### `outbox_messages` (optional — transactional outbox)

| Column         | Type         | Notes                 |
| -------------- | ------------ | --------------------- |
| `id`           | BIGINT (PK)  | Auto-increment        |
| `type`         | VARCHAR(100) | Message type          |
| `payload_json` | JSONB        |                       |
| `created_at`   | TIMESTAMPTZ  |                       |
| `published_at` | TIMESTAMPTZ  | Null until dispatched |

## Engine Database Schema (`skillexa_engine`)

### `templates`

| Column         | Type          | Notes                           |
| -------------- | ------------- | ------------------------------- |
| `id`           | BIGINT (PK)   | Auto-increment                  |
| `template_key` | VARCHAR(100)  |                                 |
| `version`      | INT           | Auto-incremented per key        |
| `is_active`    | BOOLEAN       | Only one active version per key |
| `content`      | JSONB or TEXT | Template body                   |
| `updated_at`   | TIMESTAMPTZ   |                                 |

Separate copy from Core's `templates` — managed independently (manually synced).

### `provider_quotas`

| Column       | Type        | Notes                                 |
| ------------ | ----------- | ------------------------------------- |
| `id`         | BIGINT (PK) | Auto-increment (implements `IEntity`) |
| `provider`   | VARCHAR(50) |                                       |
| `day_key`    | DATE        |                                       |
| `used`       | INT         | Atomically incremented                |
| `limit`      | INT         | Daily cap                             |
| `updated_at` | TIMESTAMPTZ |                                       |

Unique index on `(provider, day_key)`. Engine-exclusive — used for atomic `UPDATE … WHERE used < limit`.

## Conventions

- Use **snake_case** for all table and column names.
- All tables have a `created_at` timestamp; mutable tables also have `updated_at`.
- **All date/time columns use `TIMESTAMPTZ` and store values in UTC.** Application code must convert to UTC before writing and interpret stored values as UTC.
- Primary keys are **BIGINT with auto-increment** (except lookup tables like `job_statuses` which use INT).
- **No cascade deletion.** All foreign keys use `ON DELETE RESTRICT` (or `NO ACTION`). Deletes must be handled explicitly in application logic.
- Index `jobs(user_id, status_id)` and `jobs(created_at)` for common query patterns.
- Unique index on `jobs(idempotency_key)` for duplicate prevention.
- Unique index on `provider_usages(provider, period_key)`.
- Index `provider_quotas(provider, day_key)` for atomic increment queries.

## Migrations

- Use **EF Core code-first migrations** (`dotnet ef migrations add`, `dotnet ef database update`).
- Each migration should be small and focused on a single schema change.
- Never modify a migration that has already been applied to a shared environment.
- **Core** migrations are stored in `skillexa-core/Data/Migrations/` (context: `ApplicationDbContext`).
- **Engine** migrations are stored in `skillexa-engine/Data/Migrations/` (context: `EngineDbContext`).

## Key Rules

- `template_version` is snapshotted in `jobs` at creation time to guarantee reproducibility.
- Large data (PDF content, input snapshots) is stored in **blob storage**, not in the DB — only storage keys are persisted.
- Provider quota enforcement uses atomic `UPDATE … WHERE used < limit` to prevent race conditions.
