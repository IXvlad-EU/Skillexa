---
applyTo: "mock-theirstack/**"
---

# Mock TheirStack — Instructions

## Purpose

`mock-theirstack` is a **standalone Node.js project** that emulates the TheirStack REST API for local development. It eliminates the need to call the real TheirStack API during development and testing, preventing quota consumption and removing the external dependency.

## Stack

| Concern | Choice |
|---|---|
| Runtime | Node.js 22 LTS |
| Framework | Express (minimal — only the endpoints Skillexa-Engine calls) |
| Language | TypeScript (compiled to JS in Docker build) |
| Package manager | pnpm |
| Port | `3100` |

## Project Location

```
mock-theirstack/          # repo root, sibling of skillexa-core / skillexa-engine / skillexa-portal
  package.json
  pnpm-lock.yaml
  tsconfig.json
  Dockerfile
  .dockerignore
  src/
    index.ts              # Express app entry point, starts server
    routes/
      jobs.ts             # TheirStack job-search endpoint handler(s)
    fixtures/
      jobs.json           # Static/seed response data (array of job objects)
    middleware/
      auth.ts             # Validates API key header (mirrors TheirStack auth)
      delay.ts            # Optional artificial latency to simulate real network
    utils/
      logger.ts           # Lightweight structured logging (pino or console)
```

## API Surface

Implement **only** the endpoints that Skillexa-Engine actually calls. At minimum:

| Method | Path | Purpose |
|---|---|---|
| `POST` | `/v1/jobs/search` | Returns a paginated list of job postings matching the request payload |

### Request validation

- Require an `Authorization: Bearer <api-key>` header (or `X-Api-Key` — match whatever TheirStack uses).
- Return `401 Unauthorized` if the key is missing or does not match the configured mock key.
- Accept a JSON body with fields like `job_title_pattern`, `keywords`, `page`, `limit`, etc.

### Response format

- Mirror the real TheirStack response schema as closely as possible.
- Return data from fixture files (`fixtures/jobs.json`) so responses are deterministic.
- Support pagination (`page` / `limit` in request → sliced fixture array + `total` count in response).

### Error simulation

Expose optional behaviour controlled by **environment variables** or **request headers**:

| Trigger | Behaviour |
|---|---|
| Header `X-Mock-Status: 429` | Return HTTP 429 (rate limit) — useful for testing retry logic |
| Header `X-Mock-Status: 500` | Return HTTP 500 (server error) — useful for testing transient-error handling |
| Header `X-Mock-Delay: <ms>` | Add artificial delay before responding |
| Env `MOCK_DEFAULT_DELAY_MS` | Default latency added to every response (default `0`) |
| Env `MOCK_FAIL_RATE` | Fraction `0.0–1.0` of requests that randomly return 500 (default `0`) |

## Docker

### Dockerfile

- Multi-stage build:
  1. **Build stage** — `node:22-alpine`, install deps, compile TypeScript.
  2. **Production stage** — `node:22-alpine`, copy compiled JS + `node_modules` (production only), run as non-root user.
- Expose port `3100`.
- Entry point: `node dist/index.js`.

### Docker Compose integration

Add a `mock-theirstack` service to the repo-root `docker-compose.yml`:

```yaml
mock-theirstack:
  build:
    context: ./mock-theirstack
    dockerfile: Dockerfile
  container_name: mock-theirstack
  ports:
    - "3100:3100"
  environment:
    - PORT=3100
    - MOCK_API_KEY=dev-theirstack-key
    - MOCK_DEFAULT_DELAY_MS=0
    - MOCK_FAIL_RATE=0
  networks:
    - skillexa-network
  restart: unless-stopped
  healthcheck:
    test: ["CMD", "wget", "--no-verbose", "--tries=1", "--spider", "http://localhost:3100/health"]
    interval: 15s
    timeout: 5s
    retries: 3
    start_period: 10s
```

### Engine configuration

Point Skillexa-Engine to the mock service in `docker-compose.yml`:

```yaml
skillexa-engine:
  environment:
    - TheirStack__BaseUrl=http://mock-theirstack:3100
    - TheirStack__ApiKey=dev-theirstack-key
```

In production, these variables point to the real TheirStack API — **no code changes required**.

## Environment Variables

| Variable | Default | Purpose |
|---|---|---|
| `PORT` | `3100` | HTTP listen port |
| `MOCK_API_KEY` | `dev-theirstack-key` | Expected API key for auth validation |
| `MOCK_DEFAULT_DELAY_MS` | `0` | Artificial latency (ms) added to every response |
| `MOCK_FAIL_RATE` | `0` | Random failure rate (`0.0`–`1.0`) for chaos testing |
| `LOG_LEVEL` | `info` | Log verbosity (`debug`, `info`, `warn`, `error`) |

## Fixture Data

- Store fixture JSON files under `src/fixtures/`.
- `jobs.json` contains an array of realistic-looking job posting objects matching the TheirStack schema.
- Keep fixtures committed to the repo — they serve as a shared contract between Engine developers and the mock.
- Fixtures should cover edge cases: jobs with missing optional fields, very long descriptions, special characters, etc.

## Health Endpoint

| Method | Path | Response |
|---|---|---|
| `GET` | `/health` | `200 OK` — `{ "status": "healthy" }` |

Used by Docker Compose health check and orchestrator probes.

## Coding Standards

- Enable TypeScript strict mode.
- Use ESM modules (`"type": "module"` in `package.json`).
- Keep the codebase minimal — this is a dev tool, not a production service.
- No database required — all data comes from in-memory fixtures loaded at startup.
- Log every request with method, path, response status, and latency for observability during development.

## Key Rules

- **Never deploy this service to production.** It exists solely for local development and CI.
- The mock must stay in sync with the real TheirStack API contract. If Engine changes the endpoints or request/response shapes it uses, update the mock accordingly.
- Do not add business logic beyond what is needed to return realistic responses and simulate errors.
