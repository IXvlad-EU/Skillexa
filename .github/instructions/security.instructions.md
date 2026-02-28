---
applyTo: "**"
---

# Security — Instructions

## Authentication

- **JWT Bearer tokens** issued by Skillexa-Core (`POST /auth/login`).
- Access tokens are short-lived (15–60 min); optional refresh tokens are longer-lived and stored hashed in the DB.
- Skillexa-Portal (BFF) stores the JWT in an **httpOnly, Secure, SameSite=Strict** cookie — the browser JavaScript never accesses the token.
- All API endpoints (except `/auth/login`, `/auth/refresh`) require a valid `Authorization: Bearer <token>` header.

## Password Storage

- Passwords are hashed with **bcrypt** or **Argon2id** before storage.
- **Never** store plaintext passwords.
- Use a work factor / memory cost appropriate for current hardware (bcrypt cost ≥ 12).

## Object Storage Access

- All blob containers are **private** — no public/anonymous access.
- Users download PDFs only via **short-lived signed URLs** (SAS tokens):
  - Read-only.
  - Scoped to a single blob key.
  - TTL: 5–15 minutes.
- Signed URLs are generated server-side by Skillexa-Core after verifying:
  1. The requesting user owns the job (`job.userId == currentUser`).
  2. The job status is `Succeeded`.

## Secrets Management

- All secrets (DB connection strings, broker credentials, storage keys, TheirStack API key, JWT signing key) come from **environment variables or a secrets manager** — never hard-coded or committed to source control.
- `.env` files for local development are listed in `.gitignore`.

## Idempotency

- `jobId` (or a dedicated `idempotencyKey`) prevents duplicate processing when a message is redelivered.
- The Engine checks for terminal job status before processing.

## Rate Limiting

| Layer | Scope | Mechanism |
|---|---|---|
| Edge / Ingress | IP, burst | Nginx/Traefik rate-limit module or cloud WAF |
| API (Skillexa-Core) | Per-user, per-plan | Middleware + DB check (quota tables) |
| Provider (TheirStack) | Daily call limit | Atomic DB decrement in Engine before API call |

## Retry & Failure

- **Transient errors** (HTTP 429/5xx, network timeouts): exponential backoff + jitter, max 5 retries.
- **Permanent failures** (4xx, validation errors): fail immediately, emit `Failed` event, no retry.
- Exhausted retries → message routed to **Dead-Letter Queue (DLQ)**.

## General Rules

- Validate and sanitize all user inputs at the API boundary.
- Use parameterized queries / ORM — no raw string concatenation in SQL.
- Enable HTTPS everywhere in production; HTTP allowed only for local container-to-container traffic.
- Log security-relevant events (login attempts, auth failures, quota violations) with structured logging.
- Never log sensitive data (passwords, tokens, full connection strings).
