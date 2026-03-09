---
description: "Docker Compose setup, Dockerfile conventions, and deployment configuration"
applyTo: "**/Dockerfile,docker-compose.yml"
---

# Docker & Deployment — Instructions

## Container-First Design

All three application components run as Docker containers. The same images work locally (Docker Compose) and in production (Azure Container Apps / AKS / ECS / K8s).

## Docker Compose (Local Development)

The `docker-compose.yml` at the repo root defines the full local environment:

| Service           | Image / Build                                                    | Ports       | Purpose                                 |
| ----------------- | ---------------------------------------------------------------- | ----------- | --------------------------------------- |
| `skillexa-portal` | `./skillexa-portal/Dockerfile`                                   | 3000        | Next.js SSR + BFF                       |
| `skillexa-core`   | `./skillexa-core/Dockerfile`                                     | 8080        | ASP.NET Core API                        |
| `skillexa-engine` | `./skillexa-engine/Dockerfile`                                   | —           | .NET Worker (no HTTP)                   |
| `mock-theirstack` | `./mock-theirstack/Dockerfile`                                   | 3100        | TheirStack API mock for local dev       |
| `postgres`        | `postgres:17-alpine`                                             | 5432        | PostgreSQL database                     |
| `rabbitmq`        | `./rabbitmq/Dockerfile` (base: `rabbitmq:4.1-management-alpine`) | 5672, 15672 | Message broker (management UI on 15672) |

> **Note:** An `azurite` service (Azure Storage emulator) is not yet in the Compose file. Add it when blob storage features are implemented.

### Network

- All services join a shared bridge network (`skillexa-network`).
- Services reference each other by container name (e.g., `http://skillexa-core:8080`, `amqp://rabbitmq:5672`, `http://azurite:10000`).

### Environment Variables

- Local dev values go in a root `.env` file (gitignored) and referenced from `docker-compose.yml` via `${VAR_NAME}` syntax.
- The `.env` file defines: `RABBITMQ_USER`, `RABBITMQ_PASS`, `RABBITMQ_VHOST`, `THEIRSTACK_API_KEY`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`.
- Production values come from the deployment platform's secret/config management.

## Dockerfile Conventions

### ASP.NET Core / .NET Worker (Skillexa-Core, Skillexa-Engine)

- Multi-stage build: `sdk` stage (restore + publish) → `aspnet` / `runtime` stage (copy published output).
- Target framework: `net10.0`.
- Expose port `8080` for Skillexa-Core; no port for Engine.
- Run as non-root user.

### Next.js (Skillexa-Portal)

- Multi-stage build: `node` stage (install + build) → production stage (copy `.next/standalone`).
- Output mode: `standalone` (`next.config.ts` → `output: "standalone"`).
- Expose port `3000`.
- Run as non-root user.

## Production Deployment (Azure)

| Concern         | Azure Service                                   |
| --------------- | ----------------------------------------------- |
| Compute         | Azure Container Apps or AKS                     |
| Database        | Azure Database for PostgreSQL — Flexible Server |
| Broker          | Azure Service Bus (replaces RabbitMQ)           |
| Object storage  | Azure Blob Storage (replaces Azurite)           |
| Edge (optional) | Azure Front Door + WAF                          |

- Swap infrastructure by changing `Messaging__Provider`, `Storage__Provider`, and connection strings — **no code changes**.
- Containers are pushed to Azure Container Registry (ACR) or equivalent.

## Health Checks

- Skillexa-Core: HTTP health endpoint (e.g., `/health`) checked by Docker/orchestrator.
- Skillexa-Engine: liveness probe via TCP or a custom health check hosted service.
- Skillexa-Portal: HTTP probe on port 3000.

## Key Rules

- **No cloud-specific SDKs in Dockerfiles** — infrastructure adapters are resolved at runtime via config.
- Keep images small: use `-slim` / `-alpine` base images where possible.
- Pin base image tags to specific versions for reproducibility.
- `.dockerignore` must exclude `node_modules/`, `bin/`, `obj/`, `.next/`, and other build artefacts.
