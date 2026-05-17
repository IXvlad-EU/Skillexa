# Security

## Authentication

- **Microsoft Entra ID** is the intended identity provider. Service-specific details live in `skillexa-core/ai/instructions/authentication.md` and `skillexa-portal/ai/instructions/authentication.md`.
- Skillexa-Core validates Bearer tokens issued by Entra ID using `Microsoft.Identity.Web`.
- Skillexa-Portal (BFF) stores the encrypted session in an **httpOnly, Secure, SameSite=Strict** cookie — the browser never sees raw access tokens.
- Authorization is wired but temporarily not enforced on Core endpoints while Entra app registrations are pending; endpoints currently use `// TODO: .RequireAuthorization()` comments.
- Portal BFF routes can be made public for local development with `AUTH_REQUIRED=false`.
- There are **no local passwords, no self-issued tokens, and no login/refresh endpoints**.

## Object Storage Access

- All blob containers are **private** — no public/anonymous access.
- Users should download PDFs only via **short-lived signed URLs** once object storage is implemented:
  - Read-only.
  - Scoped to a single blob key.
  - TTL: 5–15 minutes.
- Current code verifies document ownership/status and returns a placeholder storage URL until `IObjectStorage` exists.

## Secrets Management

- All secrets (DB connection strings, broker credentials, storage keys, TheirStack API key, Entra ID client secrets) come from **environment variables or a secrets manager** — never hard-coded or committed to source control.
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
