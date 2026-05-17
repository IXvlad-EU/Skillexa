# Engine Database Schema (`skillexa_engine`)

## `templates`

| Column         | Type          | Notes                           |
| -------------- | ------------- | ------------------------------- |
| `id`           | BIGINT (PK)   | Auto-increment                  |
| `template_key` | VARCHAR(100)  |                                 |
| `version`      | INT           | Auto-incremented per key        |
| `is_active`    | BOOLEAN       | Only one active version per key |
| `content`      | TEXT          | Template body                   |
| `updated_at`   | TIMESTAMPTZ   |                                 |

Separate copy from Core's `templates` — managed independently (manually synced).

## `provider_quotas`

| Column       | Type        | Notes                                 |
| ------------ | ----------- | ------------------------------------- |
| `id`         | BIGINT (PK) | Auto-increment (implements `IEntity`) |
| `provider`   | VARCHAR(50) |                                       |
| `day_key`    | DATE        |                                       |
| `used`       | INT         | Atomically incremented                |
| `limit`      | INT         | Daily cap                             |
| `updated_at` | TIMESTAMPTZ |                                       |

Unique index on `(provider, day_key)`. Engine-exclusive — used for atomic `UPDATE … WHERE used < limit`.
