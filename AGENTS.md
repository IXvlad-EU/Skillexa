# Skillexa

## Repository Structure

This repository contains 3 bounded contexts:

- `skillexa-core` — ASP.NET Core API, CQRS, orchestration
- `skillexa-engine` — async background worker
- `skillexa-portal` — Next.js SSR/BFF frontend

## Context Selection

Before editing files inside a service, read that service's local `AGENTS.md`.

Use root `ai/` only for shared repository-wide rules.

Do not apply service-specific rules globally.

## Shared Documentation

Shared repository-wide documentation lives in `ai/instructions/`.
Reusable templates live in `ai/templates/`.

Use shared docs only when they are relevant to the current task.

## Service-Specific Context

Each service contains its own local AI context:

- `skillexa-core/ai`
- `skillexa-engine/ai`
- `skillexa-portal/ai`

## Rules

- Prefer local context over global assumptions.
- Follow bounded-context isolation.
- Do not mix frontend, API, and worker patterns.
- Reuse existing patterns before introducing new abstractions.
- Keep AI documentation lightweight and close to the code it describes.
- Do not create new instruction files unless the rule is reusable and likely to be needed again.
