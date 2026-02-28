---
applyTo: "skillexa-portal/**"
---

# Skillexa-Portal — Instructions

## Stack

| Concern | Choice |
|---|---|
| Framework | Next.js 16 (App Router, SSR) |
| Language | TypeScript (strict mode) |
| React | 19.x |
| UI library | **Mantine 7** (`@mantine/core`, `@mantine/hooks`, `@mantine/form`, `@mantine/notifications`) |
| Styling | Mantine CSS modules + PostCSS with `postcss-preset-mantine` and `postcss-simple-vars` |
| Data fetching | TanStack Query (React Query) |
| API client | **Kiota**-generated TypeScript client from Skillexa-Core's OpenAPI spec |
| Output | `standalone` (Docker-friendly) |
| Package manager | pnpm (workspace) |

## Mantine UI Library

Mantine is the **primary UI library** for all portal components. Follow these rules:

### Setup

- Wrap the application in `MantineProvider` inside `app/layout.tsx`.
- Import `@mantine/core/styles.css` (and any other package styles like `@mantine/notifications/styles.css`) in the root layout **before** any other CSS.
- Add `ColorSchemeScript` inside `<head>` to prevent color-scheme flash.
- Spread `mantineHtmlProps` on the `<html>` element to avoid hydration warnings:
  ```tsx
  import { ColorSchemeScript, MantineProvider, mantineHtmlProps } from '@mantine/core';
  // ...
  <html lang="en" {...mantineHtmlProps}>
    <head><ColorSchemeScript defaultColorScheme="auto" /></head>
    <body><MantineProvider>{children}</MantineProvider></body>
  </html>
  ```
- Create `postcss.config.cjs` at the project root with `postcss-preset-mantine` and `postcss-simple-vars` plugins:
  ```js
  module.exports = {
    plugins: {
      'postcss-preset-mantine': {},
      'postcss-simple-vars': {
        variables: {
          'mantine-breakpoint-xs': '36em',
          'mantine-breakpoint-sm': '48em',
          'mantine-breakpoint-md': '62em',
          'mantine-breakpoint-lg': '75em',
          'mantine-breakpoint-xl': '88em',
        },
      },
    },
  };
  ```

### Component Usage

- **Always prefer Mantine components** over raw HTML elements or custom implementations. Use `Button`, `TextInput`, `Select`, `Modal`, `AppShell`, `Stack`, `Group`, `Grid`, `Card`, `Text`, `Title`, etc.
- For layout, use Mantine's `AppShell` for the authenticated shell (header, navbar, main content).
- Use `@mantine/form` for all form state management (validation, dirty tracking, submit handling).
- Use `@mantine/notifications` for toast/notification feedback (success, error, info messages).
- Use `@mantine/hooks` for common UI utilities (`useDisclosure`, `useMediaQuery`, `useClipboard`, `useDebouncedValue`, etc.).

### Styling Rules

- Use **Mantine CSS modules** (`.module.css` files) for component-specific styles. Access Mantine theme variables via `var(--mantine-*)` CSS custom properties.
- Use Mantine **style props** (`p`, `m`, `fw`, `fz`, `c`, `bg`, etc.) for quick inline-style adjustments.
- Do **not** use Tailwind CSS — Mantine replaces it entirely.
- For theming, extend the Mantine default theme in a central `theme.ts` file and pass it to `MantineProvider`.
- Override component default props via `theme.components` when you need consistent styling across the app.

### Color Scheme

- Support **light and dark** color schemes via Mantine's built-in `colorScheme` management.
- Store the user's preference in a cookie (`ColorSchemeScript` + server-side detection).

### SSR / Next.js App Router Integration

- All `@mantine/*` package entry points already include `'use client';` — you do **not** need to add it to files that import Mantine components.
- Mantine components render on both server and client (they cannot be true Server Components due to `useContext` usage for theming/Styles API).
- **Compound components** (e.g., `Popover.Target`, `Popover.Dropdown`) cannot be used in Server Components. Use the flat import syntax instead: `PopoverTarget`, `PopoverDropdown`.
- Enable tree shaking in `next.config.ts`:
  ```ts
  export default {
    experimental: {
      optimizePackageImports: ['@mantine/core', '@mantine/hooks'],
    },
  };
  ```
- Use Next.js `Link` with Mantine polymorphic components via the `component` prop: `<Button component={Link} href="/path">`.

## Architecture: SSR + BFF

- The portal uses **Server-Side Rendering** via the Next.js App Router.
- It also acts as a **Backend-for-Frontend (BFF)**: the browser never talks directly to Skillexa-Core. All API calls go through Next.js Route Handlers or Server Actions, which forward them to Skillexa-Core internally (server-to-server).
- Benefits: no CORS issues, JWT tokens stay on the server side (httpOnly cookies), reduced client bundle.

## Kiota OpenAPI Client

- Skillexa-Core exposes an **OpenAPI specification**.
- Use **Microsoft Kiota** to generate a typed TypeScript HTTP client from that spec.
- Place generated client code in `skillexa-portal/lib/api-client/` (or similar).
- Regenerate the client whenever the Core OpenAPI spec changes (`kiota generate` or equivalent npm script).
- The Kiota client is used exclusively on the **server side** (Route Handlers, Server Components, Server Actions) — never import it in client components.

## Folder Conventions (App Router)

```
app/
  layout.tsx          # root layout (fonts, providers, global styles)
  page.tsx            # landing / home page
  (auth)/
    login/page.tsx
  (dashboard)/
    layout.tsx        # authenticated layout shell
    documents/
      page.tsx        # create document form
    jobs/
      page.tsx        # jobs grid
      [jobId]/
        page.tsx      # single job detail + download
  api/                # BFF Route Handlers (proxy to Core)
lib/
  api-client/         # Kiota-generated client
  hooks/              # custom React hooks (TanStack Query wrappers)
  utils/
components/           # shared UI components
```

## TanStack Query Guidelines

- Wrap the app in a `QueryClientProvider` inside a **client component** provider (`app/providers.tsx`).
- Define query keys as constants in a central file (`lib/hooks/query-keys.ts`).
- Each domain entity gets its own hook file (e.g., `useJobs.ts`, `useDocuments.ts`) exporting `useQuery` / `useMutation` hooks.
- Server-side prefetching: use `dehydrate` / `HydrationBoundary` to prefetch data in Server Components and hydrate on the client.
- All mutations that create or modify data should use `useMutation` with proper `onSuccess` invalidation.

## Auth Flow

- Login: user submits credentials → BFF Route Handler calls `POST /auth/login` on Core → receives JWT → sets an **httpOnly, Secure, SameSite=Strict** cookie.
- Subsequent requests: the BFF reads the cookie, attaches the JWT as a `Bearer` header when calling Core.
- Token refresh: BFF calls `POST /auth/refresh` transparently when the token is near expiry.
- Client components never see or handle raw JWTs.

## Environment Variables

| Variable | Purpose |
|---|---|
| `SKILLEXA_CORE_BASE_URL` | Internal URL of Skillexa-Core (e.g., `http://skillexa-core:8080`) |
| `NEXT_PUBLIC_APP_URL` | Public URL of the portal itself |
| `NODE_ENV` | `development` / `production` |

## Coding Standards

- Use **Server Components** by default; add `"use client"` only when interactivity is needed.
- Prefer `async` Server Components for data loading.
- Keep client components small and focused on interaction / state.
- All pages under authenticated routes must verify the session server-side before rendering.
- Use TypeScript `strict` — no `any` types without justification.
- Set `"target": "ES2022"` (or later) in `tsconfig.json` to enable modern type-checking (e.g., `Array.at()`, `structuredClone`, `Error.cause`).
- ESLint config extends `eslint-config-next`.

## VS Code Setup

- Install [PostCSS Intellisense and Highlighting](https://marketplace.visualstudio.com/items?itemName=vunguyentuan.vscode-postcss) to enable syntax highlighting for Mantine postcss syntax (e.g., `$variable` references).
- Install [CSS Variable Autocomplete](https://marketplace.visualstudio.com/items?itemName=vunguyentuan.vscode-css-variables) and add to `.vscode/settings.json`:
  ```json
  {
    "cssVariables.lookupFiles": [
      "**/*.css",
      "node_modules/@mantine/core/styles.css"
    ]
  }
  ```
