# Skillexa

Skillexa is a skill-based job discovery and CV generation platform.

The repository has three main services:

- `skillexa-portal` - Next.js SSR/BFF frontend.
- `skillexa-core` - ASP.NET Core API, CQRS, persistence, and TheirStack proxy.
- `skillexa-engine` - .NET worker scaffold for async document processing.

## Prerequisites

- Docker and Docker Compose.
- .NET SDK `10.0.100` or compatible latest feature release.
- Node.js `24.x`.
- pnpm `11.0.9`.
- OpenSSL for local secret/key generation.

## Local Configuration

Local Docker and app configuration is driven by a root `.env` file. This file is gitignored and is not created by default.

Create `.env` in the repository root and fill every value before running the full app:

```env
# --- PostgreSQL ---
POSTGRES_USER=skillexa
POSTGRES_PASSWORD=skillexa_dev
POSTGRES_DB=skillexa_core
POSTGRES_DB_ENGINE=skillexa_engine

# --- RabbitMQ ---
RABBITMQ_USER=skillexa
RABBITMQ_PASS=skillexa_dev
RABBITMQ_VHOST=skillexa

# --- TheirStack mock ---
THEIRSTACK_BASE_URL=http://mock-theirstack:3100
THEIRSTACK_API_KEY=mock-api-key

# --- Microsoft Entra ID ---
ENTRA_TENANT_ID=<CHANGE_ME>
ENTRA_PORTAL_CLIENT_ID=<CHANGE_ME>
ENTRA_PORTAL_CLIENT_SECRET=<CHANGE_ME>

# --- Google ---
AUTH_GOOGLE_ID=<CHANGE_ME>
AUTH_GOOGLE_SECRET=<CHANGE_ME>

# --- NextAuth ---
AUTH_SECRET=<CHANGE_ME>

# --- Portal-issued Core JWT signing ---
JWT_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\n<CHANGE_ME>\n-----END PRIVATE KEY-----"
JWT_PUBLIC_KEY="-----BEGIN PUBLIC KEY-----\n<CHANGE_ME>\n-----END PUBLIC KEY-----"
```

The database, RabbitMQ, and mock TheirStack values above are safe local defaults. OAuth credentials and signing secrets must be generated or copied from provider consoles.

## Generate Local Values

Generate `AUTH_SECRET`:

```bash
openssl rand -hex 32
```

Put the output into `.env`:

```env
AUTH_SECRET=<generated hex value>
```

Generate one RSA key pair for Portal-issued Core JWTs:

```bash
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out portal-jwt-private.pem
openssl rsa -pubout -in portal-jwt-private.pem -out portal-jwt-public.pem
```

Put the private key into `.env` as `JWT_PRIVATE_KEY` and the matching public key as `JWT_PUBLIC_KEY`.

For `.env`, keep each PEM on one line by replacing real newlines with literal `\n`:

```env
JWT_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----"
JWT_PUBLIC_KEY="-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----"
```

Core receives `JWT_PUBLIC_KEY` from Docker Compose as `JWT__PublicKey`. Portal receives `JWT_PRIVATE_KEY` directly.

Microsoft and Google OAuth values come from the provider consoles:

- Microsoft Entra ID: create/use the Portal web app registration and copy tenant ID, client ID, and client secret.
- Google Cloud Console: create/use an OAuth web client and copy client ID and client secret.

## Run Locally With Docker Infrastructure

For local development, run PostgreSQL and RabbitMQ in Docker. The mock TheirStack service is also easiest to run with Docker.

```bash
docker compose up -d postgres rabbitmq mock-theirstack
```

Docker Compose reads the root `.env` automatically. When running Core, Engine, or Portal directly from a terminal, export the same values in that terminal or pass them inline as shown below.

Install Portal dependencies:

```bash
cd skillexa-portal
pnpm install
cd ..
```

Build the .NET services:

```bash
dotnet build Skillexa.sln
```

Build the Portal:

```bash
cd skillexa-portal
pnpm build
cd ..
```

Run Core:

```bash
cd skillexa-core
JWT__PublicKey="-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----" \
TheirStack__BaseUrl="http://localhost:3100" \
TheirStack__ApiKey="mock-api-key" \
dotnet run
```

Run Engine in a second terminal:

```bash
cd skillexa-engine
dotnet run
```

Run Portal in a third terminal:

```bash
cd skillexa-portal
NEXTAUTH_SECRET="<AUTH_SECRET>" \
NEXTAUTH_URL="http://localhost:3000" \
AUTH_MICROSOFT_ENTRA_ID_ID="<ENTRA_PORTAL_CLIENT_ID>" \
AUTH_MICROSOFT_ENTRA_ID_SECRET="<ENTRA_PORTAL_CLIENT_SECRET>" \
AUTH_MICROSOFT_ENTRA_ID_TENANT_ID="<ENTRA_TENANT_ID>" \
AUTH_GOOGLE_ID="<AUTH_GOOGLE_ID>" \
AUTH_GOOGLE_SECRET="<AUTH_GOOGLE_SECRET>" \
JWT_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----" \
SKILLEXA_CORE_BASE_URL="http://localhost:8080" \
pnpm dev
```

Useful local URLs:

- Portal: `http://localhost:3000`
- Core OpenAPI: `http://localhost:8080/openapi/v1.json`
- RabbitMQ management UI: `http://localhost:15672`
- Mock TheirStack: `http://localhost:3100`

Stop manually started Core, Engine, or Portal processes with `Ctrl+C` in each terminal.

If a local process keeps running after its terminal was closed, find and stop it by port:

```bash
lsof -ti :3000 | xargs kill
lsof -ti :8080 | xargs kill
```

If `dotnet run` or `pnpm dev` is still running without an open port, find the process and stop it by PID:

```bash
ps aux | grep -E "dotnet run|next dev"
kill <PID>
```

Stop the Docker infrastructure:

```bash
docker compose stop postgres rabbitmq mock-theirstack
```

## Run Everything With Docker Compose

Fill `.env`, then build and start the complete local stack:

```bash
docker compose up --build
```

Run it in the background:

```bash
docker compose up --build -d
```

Stop the stack:

```bash
docker compose down
```

Remove local database and RabbitMQ volumes when you need a clean environment:

```bash
docker compose down -v
```

## Common Checks

Core:

```bash
cd skillexa-core
dotnet build
```

Portal:

```bash
cd skillexa-portal
pnpm exec tsc --noEmit
pnpm lint
pnpm build
```

Engine:

```bash
cd skillexa-engine
dotnet build
```
