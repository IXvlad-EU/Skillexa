---
description: "TheirStack API mock service for local development and testing"
applyTo: "mock-theirstack/**"
---

# Mock TheirStack — Instructions

## API Specification

The authoritative contract for the TheirStack API is the OpenAPI 3.1 specification committed at the repo root:

```
theirstack-api.specification.json
```

Use this file as the reference when adding or updating mock endpoints, verifying request/response shapes, or checking error schemas. The mock must stay in sync with this spec — specifically the endpoints that Skillexa-Core and Skillexa-Engine actually call.

## Purpose

`mock-theirstack` is a **standalone Node.js project** that emulates the TheirStack REST API for local development. It eliminates the need to call the real TheirStack API during development and testing, preventing quota consumption and removing the external dependency.

## Stack

| Concern         | Choice                                                               |
| --------------- | -------------------------------------------------------------------- |
| Runtime         | Node.js 24 LTS                                                       |
| Framework       | Express 5 (minimal — only the endpoints Skillexa-Engine calls)       |
| Language        | TypeScript (strict mode, ESM, compiled to JS in Docker build)        |
| Package manager | pnpm                                                                 |
| Logging         | pino + pino-http (structured JSON, request-level logging middleware) |
| Port            | `3100`                                                               |

## Project Location

```
mock-theirstack/          # repo root, sibling of skillexa-core / skillexa-engine / skillexa-portal
  package.json
  pnpm-lock.yaml
  tsconfig.json
  Dockerfile
  src/
    index.ts              # Express app entry point, starts server
    routes/
      jobs.ts             # TheirStack job-search endpoint handler(s)
    fixtures/
      jobs.json           # Static/seed response data (array of job objects)
    middleware/
      auth.ts             # Validates Authorization Bearer header
      delay.ts            # Optional artificial latency to simulate real network
      errorSimulation.ts  # Simulates 429/500 errors via headers or random failure
    utils/
      logger.ts           # pino logger instance (level controlled by LOG_LEVEL env)
```

## Middleware Pipeline

Global middleware is applied in this order:

1. **pino-http** — structured request logging (method, path, status, latency).
2. **express.json()** — JSON body parsing.
3. **Health endpoint** — `/health` is registered **before** auth so it stays unauthenticated.
4. **auth** — validates `Authorization: Bearer <api-key>` header.
5. **delay** — adds artificial latency (per-request header or global default).
6. **errorSimulation** — returns simulated error responses (header-triggered or random).
7. **Route handlers** — the actual API endpoints.

## API Surface

Implement **only** the endpoints that Skillexa-Engine actually calls.

### `POST /v1/jobs/search`

Returns a paginated list of job postings from fixture data.

**Request body** (JSON):

| Field   | Type   | Default | Description                |
| ------- | ------ | ------- | -------------------------- |
| `page`  | number | `0`     | Zero-based page index      |
| `limit` | number | `25`    | Number of results per page |

Other fields in the body (e.g., `job_title_pattern`, `keywords`) are accepted but currently ignored — the full fixture array is always the source.

**Response** (JSON):

```jsonc
{
  "metadata": {
    "total_results": 50, // total jobs in fixture
    "truncated_results": 0,
    "truncated_companies": 0,
    "total_companies": 12, // distinct companies in fixture
  },
  "data": [
    /* sliced job objects for the requested page */
  ],
}
```

### Authentication

- Require an `Authorization: Bearer <api-key>` header on all routes except `/health`.
- Return `401 Unauthorized` with a JSON error body if the header is missing, malformed, or the key doesn't match `MOCK_API_KEY`.

### Error simulation

Expose optional behaviour controlled by **request headers** and **environment variables**:

| Trigger                       | Behaviour                                                                              |
| ----------------------------- | -------------------------------------------------------------------------------------- |
| Header `X-Mock-Status: 429`   | Return HTTP 429 with JSON error body — useful for testing retry logic                  |
| Header `X-Mock-Status: 500`   | Return HTTP 500 with JSON error body — useful for testing transient-error handling     |
| Header `X-Mock-Delay: <ms>`   | Add artificial delay (ms) before responding (overrides default)                        |
| Env `MOCK_DEFAULT_DELAY_MS`   | Default latency (ms) added to every response when no header override (default `0`)     |
| Env `MOCK_FAIL_RATE`          | Fraction `0.0–1.0` of requests that randomly return 500 (default `0`)                  |

Header-triggered errors (`X-Mock-Status`) take precedence over random failure (`MOCK_FAIL_RATE`).

## Docker

### Dockerfile

Multi-stage build with a pinned Node version via build argument:

1. **Build stage** — `node:24-alpine`, `corepack enable`, `pnpm install --frozen-lockfile`, compile TypeScript.
2. **Production stage** — same `node:24-alpine` base, production-only deps (`pnpm install --frozen-lockfile --prod`), compiled JS from build stage, fixture files copied separately (`COPY src/fixtures ./dist/fixtures`), non-root user (`appuser`).

- Expose port `3100`.
- Entry point: `node dist/index.js`.

### Docker Compose integration

The `mock-theirstack` service in the repo-root `docker-compose.yml`:

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
    test:
      [
        "CMD",
        "wget",
        "--no-verbose",
        "--tries=1",
        "--spider",
        "http://localhost:3100/health",
      ]
    interval: 15s
    timeout: 5s
    retries: 3
    start_period: 10s
```

### Engine configuration

Skillexa-Engine points to the mock service via environment variables in `docker-compose.yml`:

```yaml
skillexa-engine:
  environment:
    - TheirStack__BaseUrl=http://mock-theirstack:3100
    - TheirStack__ApiKey=dev-theirstack-key
  depends_on:
    mock-theirstack:
      condition: service_healthy
```

In production, these variables point to the real TheirStack API — **no code changes required**.

## Environment Variables

| Variable                | Default              | Purpose                                                                |
| ----------------------- | -------------------- | ---------------------------------------------------------------------- |
| `PORT`                  | `3100`               | HTTP listen port                                                       |
| `MOCK_API_KEY`          | `dev-theirstack-key` | Expected API key for auth validation                                   |
| `MOCK_DEFAULT_DELAY_MS` | `0`                  | Artificial latency (ms) added to every response                        |
| `MOCK_FAIL_RATE`        | `0`                  | Random failure rate (`0.0`–`1.0`) for chaos testing                    |
| `LOG_LEVEL`             | `info`               | pino log level (`debug`, `info`, `warn`, `error`)                      |
| `NODE_ENV`              | —                    | When not `production`, pino writes to stdout via `pino/file` transport |

## Fixture Data

- Stored under `src/fixtures/`.
- `jobs.json` contains an array of ~50 job posting objects matching the TheirStack schema (1800+ lines).
- Loaded once at startup via `readFileSync` and held in memory.
- Each fixture object includes: `id`, `job_title`, `company`, `company_object`, `description`, `location`, `salary_*`, `technology_slugs`, `hiring_team`, `employment_statuses`, `remote`/`hybrid`, `seniority`, and many more TheirStack fields.
- Keep fixtures committed to the repo — they serve as a shared contract between Engine developers and the mock.
- Fixtures should cover edge cases: jobs with missing optional fields, very long descriptions, special characters, etc.

## Health Endpoint

| Method | Path      | Response                               |
| ------ | --------- | -------------------------------------- |
| `GET`  | `/health` | `200 OK` — `{ "status": "healthy" }` |

Registered **before** the auth middleware — no API key required. Used by Docker Compose health check and orchestrator probes.

## npm Scripts

| Script  | Command                  | Purpose                                   |
| ------- | ------------------------ | ----------------------------------------- |
| `build` | `tsc`                    | Compile TypeScript to `dist/`             |
| `start` | `node dist/index.js`     | Run the compiled application              |
| `dev`   | `tsx watch src/index.ts` | Run with hot-reload for local development |

## Coding Standards

- TypeScript strict mode enabled.
- ESM modules (`"type": "module"` in `package.json`, `"module": "NodeNext"` in tsconfig).
- Keep the codebase minimal — this is a dev tool, not a production service.
- No database required — all data comes from in-memory fixtures loaded at startup.
- Use `pino-http` middleware for automatic per-request structured logging.
- Access environment variables via bracket notation: `process.env["VAR_NAME"]`.

## Key Rules

- **Never deploy this service to production.** It exists solely for local development and CI.
- The mock must stay in sync with the real TheirStack API contract defined in `theirstack-api.specification.json` (repo root). If the Engine changes the endpoints or request/response shapes it uses, update the mock and verify against the spec accordingly.
- Do not add business logic beyond what is needed to return realistic responses and simulate errors.
