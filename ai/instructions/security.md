# Security

## Authentication

- Skillexa-Portal authenticates users with Microsoft or Google through `next-auth`. Service-specific details live in `skillexa-core/ai/instructions/authentication.md` and `skillexa-portal/ai/instructions/authentication.md`.
- Skillexa-Core validates only short-lived RS256 Bearer tokens issued by Skillexa-Portal.
- Skillexa-Portal stores the encrypted NextAuth session in an **httpOnly** cookie; production deployments must use HTTPS and Secure cookies.
- The browser never receives provider access tokens, Core JWTs, private signing keys, or raw bearer tokens.
- Core endpoints are protected with `.RequireAuthorization()` except OpenAPI metadata and health checks.
- User identity is provisioned by normalized verified email; provider object IDs are not persisted in Core.
- Portal calls Core `POST /provision` once during sign-in, stores the returned Core `userId` only in the encrypted NextAuth JWT, and includes it as an optional signed `uid` claim in later short-lived Core JWTs.
- There are **no local passwords, no login/refresh endpoints, and no direct browser calls to Core**.

## Object Storage Access

- All blob containers are **private** — no public/anonymous access.
- Users should download PDFs only via **short-lived signed URLs** once object storage is implemented:
  - Read-only.
  - Scoped to a single blob key.
  - TTL: 5–15 minutes.
- Current code verifies document ownership/status and returns a placeholder storage URL until `IObjectStorage` exists.

## Secrets Management

- All secrets (DB connection strings, broker credentials, storage keys, TheirStack API key, OAuth client secrets, and JWT private keys) come from **environment variables or a secrets manager** — never hard-coded or committed to source control.
- `.env` files for local development are listed in `.gitignore`.

## Idempotency

- `documentId` or a dedicated `idempotencyKey` should prevent duplicate processing when a message is redelivered.
- Core currently stores an `idempotency_key` on `documents`; Engine terminal-status checks are not implemented yet.

## Rate Limiting

| Layer                 | Scope              | Mechanism                                     |
| --------------------- | ------------------ | --------------------------------------------- |
| Edge / Ingress        | IP, burst          | Nginx/Traefik rate-limit module or cloud WAF  |
| API (Skillexa-Core)   | Per-user, per-plan | Not implemented yet                           |
| Provider quotas       | Daily call limit   | Engine handler has atomic quota decrement scaffolding |

## Retry & Failure

- For new broker/provider integrations, handle **transient errors** (HTTP 429/5xx, network timeouts) with exponential backoff + jitter and a bounded retry count.
- **Permanent failures** (4xx, validation errors): fail immediately and record/publish failure once status propagation exists.
- Exhausted retries should route messages to a Dead-Letter Queue once broker consumers are implemented.

## General Rules

- Validate and sanitize all user inputs at the API boundary.
- Use parameterized queries / ORM — no raw string concatenation in SQL.
- Enable HTTPS everywhere in production; HTTP allowed only for local container-to-container traffic.
- Log security-relevant events (login attempts, auth failures, quota violations) with structured logging.
- Never log sensitive data (passwords, tokens, full connection strings).
