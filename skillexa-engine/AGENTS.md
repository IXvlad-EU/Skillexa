# Skillexa Engine

Async background worker for document processing.

## Stack

- .NET 10 Worker Service
- EF Core
- PostgreSQL
- Background services
- Message-driven processing planned

## Architecture

- Async processing only.
- No HTTP API surface.
- Current code includes CQRS/database scaffolding and a `ProcessGeneratePdf` handler.
- Broker consumption, real PDF rendering, blob upload, and status publishing are not implemented yet.

## Read First

Important local instructions:

- `ai/instructions/database.md`

## Project Structure

- `Commands/` — processing commands
- `Queries/` — internal read operations
- `Data/` — EF Core database layer
- `Domain/` — entities and processing logic
- `Modules/` — dependency registration and infrastructure wiring

## Rules

- Keep processing idempotent.
- Prefer async APIs everywhere.
- Isolate infrastructure concerns.
- Keep handlers focused on orchestration.
- Fail fast on invalid payloads.
- Treat external systems as unreliable.

## Avoid

- Blocking async operations.
- Long-running logic inside constructors.
- Hidden retries.
- Mixing infrastructure and domain logic.
- Tight coupling to specific cloud providers.

## Important Context

- Engine is the intended home for PDF generation.
- Processing must be retry-safe.
- Document status propagation back to Core is not implemented yet.
- Storage and broker adapters are placeholders.
