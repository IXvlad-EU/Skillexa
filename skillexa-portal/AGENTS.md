# Skillexa Portal

Next.js SSR/BFF application for job discovery and CV generation.

## Stack

- Next.js 16
- React 19
- TypeScript
- Mantine 7
- TanStack Query
- openapi-fetch
- SCSS
- next-auth

## Architecture

- SSR-first application.
- Browser never calls Skillexa-Core directly.
- All API access goes through server-side BFF layers.
- Internal JWTs remain server-side only.

## Read First

Important local instructions:

- `ai/instructions/nextjs.md`
- `ai/instructions/authentication.md`
- `ai/instructions/api-client.md`
- `ai/instructions/tanstack-query.md`
- `ai/instructions/mantine-ui.md`
- `ai/instructions/i18n.md`
- `ai/instructions/sass.md`
- `ai/instructions/prettier.md`

## Rules

- Prefer Server Components by default.
- Use Client Components only when required.
- Reuse existing query hooks before creating new ones.
- Keep business logic outside UI components.
- Use generated API types — do not manually duplicate DTOs.
- Prefer SSR and server-side data fetching when possible.
- Keep styling consistent with existing Mantine and SCSS patterns.

## Avoid

- Direct browser calls to Skillexa-Core APIs.
- Duplicating API schemas manually.
- Creating new abstractions without existing precedent.
- Mixing server-only and client-only logic.
- Introducing state management libraries without necessity.

## Important Context

- Authentication is handled via next-auth.
- TanStack Query is used for client caching only.
- API contracts come from OpenAPI-generated types.
