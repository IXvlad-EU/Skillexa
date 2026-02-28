---
applyTo: "**/*.sql,**/Migrations/**,**/Data/**"
---

# Database — Instructions

## Engine

- **PostgreSQL** is the single relational data store for Skillexa.
- Local development: PostgreSQL container in Docker Compose.
- Production: managed service (Azure Database for PostgreSQL / AWS RDS).

## Schema (high-level)

### `users`
| Column | Type | Notes |
|---|---|---|
| `id` | BIGINT (PK) | Auto-increment |
| `email` | VARCHAR(256), UNIQUE | |
| `password_hash` | VARCHAR(512) | bcrypt or Argon2id — **never store plaintext** |
| `created_at` | TIMESTAMPTZ | Default `now() AT TIME ZONE 'UTC'` |

### `refresh_tokens` (optional)
| Column | Type | Notes |
|---|---|---|
| `id` | BIGINT (PK) | Auto-increment |
| `user_id` | BIGINT (FK → users) | ON DELETE RESTRICT |
| `token_hash` | VARCHAR(512) | |
| `expires_at` | TIMESTAMPTZ | UTC |
| `created_at` | TIMESTAMPTZ | UTC |

### `job_statuses` (lookup)
| Column | Type | Notes |
|---|---|---|
| `id` | INT (PK) | Auto-increment |
| `name` | VARCHAR(20), UNIQUE | `Queued`, `Processing`, `Succeeded`, `Failed` |

Seeded on migration with the four known statuses.

### `jobs`
| Column | Type | Notes |
|---|---|---|
| `id` | BIGINT (PK) | Auto-increment |
| `user_id` | BIGINT (FK → users) | ON DELETE RESTRICT |
| `status_id` | INT (FK → job_statuses) | ON DELETE RESTRICT |
| `template_key` | VARCHAR(100) | |
| `template_version` | INT | Snapshot of version at creation time |
| `pdf_storage_key` | VARCHAR(500) | Blob path (nullable until Succeeded) |
| `snapshot_storage_key` | VARCHAR(500) | Blob path (nullable) |
| `error_code` | VARCHAR(100) | Nullable |
| `error_message` | TEXT | Nullable |
| `created_at` | TIMESTAMPTZ | UTC |
| `updated_at` | TIMESTAMPTZ | UTC |

### `provider_usage`
| Column | Type | Notes |
|---|---|---|
| `provider` | VARCHAR(50) | e.g., `theirstack` |
| `period_key` | VARCHAR(10) | `YYYY-MM` |
| `used` | INT | |
| `remaining` | INT | |
| `updated_at` | TIMESTAMPTZ | |

### `provider_quota`
| Column | Type | Notes |
|---|---|---|
| `provider` | VARCHAR(50) | |
| `day_key` | DATE | |
| `used` | INT | Atomically incremented |
| `limit` | INT | Daily cap |
| `updated_at` | TIMESTAMPTZ | |

### `templates`
| Column | Type | Notes |
|---|---|---|
| `id` | BIGINT (PK) | Auto-increment |
| `template_key` | VARCHAR(100) | |
| `version` | INT | Auto-incremented per key |
| `is_active` | BOOLEAN | Only one active version per key |
| `content` | JSONB or TEXT | Template body |
| `updated_at` | TIMESTAMPTZ | |

### `outbox_messages` (optional — transactional outbox)
| Column | Type | Notes |
|---|---|---|
| `id` | BIGINT (PK) | Auto-increment |
| `type` | VARCHAR(100) | Message type |
| `payload_json` | JSONB | |
| `created_at` | TIMESTAMPTZ | |
| `published_at` | TIMESTAMPTZ | Null until dispatched |

## Conventions

- Use **snake_case** for all table and column names.
- All tables have a `created_at` timestamp; mutable tables also have `updated_at`.
- **All date/time columns use `TIMESTAMPTZ` and store values in UTC.** Application code must convert to UTC before writing and interpret stored values as UTC.
- Primary keys are **BIGINT with auto-increment** (except lookup tables like `job_statuses` which use INT).
- **No cascade deletion.** All foreign keys use `ON DELETE RESTRICT` (or `NO ACTION`). Deletes must be handled explicitly in application logic.
- Index `jobs(user_id, status_id)` and `jobs(created_at)` for common query patterns.
- Index `provider_quota(provider, day_key)` for atomic increment queries.

## Migrations

- Use **EF Core code-first migrations** (`dotnet ef migrations add`, `dotnet ef database update`).
- Each migration should be small and focused on a single schema change.
- Never modify a migration that has already been applied to a shared environment.
- Store migrations in `skillexa-core/Data/Migrations/`.

## Key Rules

- `template_version` is snapshotted in `jobs` at creation time to guarantee reproducibility.
- Large data (PDF content, input snapshots) is stored in **blob storage**, not in the DB — only storage keys are persisted.
- Provider quota enforcement uses atomic `UPDATE … WHERE used < limit` to prevent race conditions.
