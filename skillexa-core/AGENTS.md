# Skillexa Core

ASP.NET Core API and orchestration layer for document generation workflows.

## Stack

- ASP.NET Core (.NET 10)
- EF Core
- PostgreSQL
- Autofac
- CQRS

## Architecture

- CQRS-based application structure.
- Commands handle writes.
- Queries handle reads.
- Database access flows through application patterns and abstractions.
- Core creates document records and stages async work in `outbox_messages`; the broker dispatcher is not implemented yet.
- Core does not generate PDFs itself.

## Read First

Important local instructions:

- `ai/instructions/authentication.md`
- `ai/instructions/database.md`

## Project Structure

- `Commands/` — write operations
- `Queries/` — read operations
- `Requests/` — HTTP request contracts
- `Data/` — EF Core database layer
- `Domain/` — entities and domain logic
- `Infrastructure/` — external integrations
- `Modules/` — Autofac modules and registrations

## Rules

- Use Commands for state changes.
- Use Queries for read operations.
- Keep business logic out of controllers/endpoints.
- Prefer explicit DTOs and contracts.
- Use dependency injection consistently.
- Keep infrastructure concerns isolated.
- Keep `Program.cs` in top-level program style; do not add namespace declarations there.

## Avoid

- Direct DbContext usage from endpoints.
- Mixing reads and writes inside handlers.
- Business logic inside Infrastructure.
- Service locator patterns.

## Important Context

- OpenAPI contracts are consumed by Skillexa-Portal.
- Core is responsible for orchestration and persistence.
- Async processing is intended to be delegated to Skillexa-Engine once broker dispatch is implemented.
