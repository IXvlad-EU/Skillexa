---
applyTo: "skillexa-portal/**"
---

# Skillexa-Portal — Instructions

## Stack

| Concern         | Choice                                                                                       |
| --------------- | -------------------------------------------------------------------------------------------- |
| Framework       | Next.js 16 (App Router, SSR)                                                                 |
| Language        | TypeScript (strict mode)                                                                     |
| React           | 19.x                                                                                         |
| UI library      | **Mantine 7** (`@mantine/core`, `@mantine/hooks`, `@mantine/form`, `@mantine/notifications`) |
| Styling         | Mantine CSS modules + PostCSS with `postcss-preset-mantine` and `postcss-simple-vars`        |
| Data fetching   | TanStack Query (React Query)                                                                 |
| API client      | **Kiota**-generated TypeScript client from Skillexa-Core's OpenAPI spec                      |
| Output          | `standalone` (Docker-friendly)                                                               |
| Package manager | pnpm (workspace)                                                                             |

## Mantine UI Library

Mantine is the **primary UI library** for all portal components. Follow these rules:

### Setup

- Wrap the application in `MantineProvider` inside `app/layout.tsx`.
- Import `@mantine/core/styles.css` (and any other package styles like `@mantine/notifications/styles.css`) in the root layout **before** any other CSS.
- Add `ColorSchemeScript` inside `<head>` to prevent color-scheme flash.
- Spread `mantineHtmlProps` on the `<html>` element to avoid hydration warnings:
  ```tsx
  import {
    ColorSchemeScript,
    MantineProvider,
    mantineHtmlProps,
  } from "@mantine/core";
  // ...
  <html lang="en" {...mantineHtmlProps}>
    <head>
      <ColorSchemeScript defaultColorScheme="auto" />
    </head>
    <body>
      <MantineProvider>{children}</MantineProvider>
    </body>
  </html>;
  ```
- Create `postcss.config.cjs` at the project root with `postcss-preset-mantine` and `postcss-simple-vars` plugins:
  ```js
  module.exports = {
    plugins: {
      "postcss-preset-mantine": {},
      "postcss-simple-vars": {
        variables: {
          "mantine-breakpoint-xs": "36em",
          "mantine-breakpoint-sm": "48em",
          "mantine-breakpoint-md": "62em",
          "mantine-breakpoint-lg": "75em",
          "mantine-breakpoint-xl": "88em",
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

### Icons

- Use **`@tabler/icons-react`** as the default icon library — it is Mantine's recommended icon set and is already installed.
- Always prefer Tabler icons before reaching for any other icon package. They integrate seamlessly with Mantine components (`ActionIcon`, `Button` `leftSection`/`rightSection`, `ThemeIcon`, etc.).
- Import icons individually for tree shaking: `import { IconLanguage } from '@tabler/icons-react';`
- Use a consistent icon `size` prop (e.g., `16` for inline, `20` for buttons, `24` for headers) to maintain visual harmony.
- Ensure `@tabler/icons-react` is listed in `optimizePackageImports` in `next.config.ts` for efficient bundling.
- Do **not** add alternative icon libraries (Lucide, Heroicons, Font Awesome, Material Icons, etc.) unless Tabler icons genuinely lack a required glyph — and document the reason in a code comment if so.

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
      optimizePackageImports: ["@mantine/core", "@mantine/hooks"],
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
  get-query-client.ts # QueryClient factory (isServer singleton pattern)
  providers.tsx       # client provider (QueryClientProvider + DevTools)
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
i18n/
  navigation.ts       # locale-aware navigation helpers (Link, redirect, useRouter, etc.)
  request.ts          # next-intl request config
  routing.ts          # locale routing config
lib/
  api-client/         # Kiota-generated client
  hooks/              # custom React hooks (TanStack Query wrappers)
  utils/
components/           # shared UI components
proxy.ts              # locale detection & redirect (replaces deprecated middleware.ts in Next.js 16)
next.config.ts
postcss.config.cjs
```

## TanStack Query Guidelines

### QueryClient Setup (App Router — `isServer` singleton pattern)

- **Do NOT** use `useState` to create the `QueryClient` — React will throw away the client on the initial render if it suspends and there is no Suspense boundary above.
- Create a dedicated `app/get-query-client.ts` file that exports a `getQueryClient()` function:
  - On the **server** (`isServer === true`): always return a **new** `QueryClient` (prevents data sharing between requests).
  - On the **browser**: lazily create and return a **singleton** `QueryClient` (prevents re-creation during Suspense).
- Import `getQueryClient` in both:
  - `app/providers.tsx` (client component with `QueryClientProvider`).
  - Any Server Component that does prefetching.
- Configure `defaultOptions.queries.staleTime` to `60 * 1000` (60 s) to avoid refetching immediately on the client after SSR.
- Configure `defaultOptions.dehydrate.shouldDehydrateQuery` to also include `pending` queries for streaming SSR support.
- Do **not** override `refetchOnWindowFocus` globally — leave the TanStack default (`true`). Disable per-query only when justified.

### Provider

- Wrap the app in a `QueryClientProvider` inside a `"use client"` provider component (`app/providers.tsx`).
- Include `ReactQueryDevtools` inside the provider (tree-shaken in production).

### Query Keys

- Define query keys as constants in a central file (`lib/hooks/query-keys.ts`).
- Use a hierarchical structure (e.g., `queryKeys.jobs.all`, `queryKeys.jobs.detail(id)`).

### Hooks

- Each domain entity gets its own hook file (e.g., `useJobs.ts`, `useDocuments.ts`) exporting `useQuery` / `useMutation` hooks.
- Hooks are `"use client"` and call BFF route handlers (`/api/*`) — never call Skillexa-Core directly.
- All mutations that create or modify data should use `useMutation` with proper `onSuccess` invalidation.

### Server-Side Prefetching (SSR)

- In **Server Components** (e.g., `page.tsx`), call `getQueryClient()` and use `queryClient.prefetchQuery(...)` to prefetch data on the server.
- Wrap the client subtree with `<HydrationBoundary state={dehydrate(queryClient)}>` to pass prefetched data to the client cache.
- Prefetch `queryFn` should call the Kiota API client directly (server-to-server), **not** the BFF `/api/*` routes.
- Use `await` on `prefetchQuery` for critical content; omit `await` for non-critical data to enable streaming.
- Each page/layout that prefetches needs its own `<HydrationBoundary>` — this cannot be hoisted to a shared layout.

## Auth Flow

- Login: user submits credentials → BFF Route Handler calls `POST /auth/login` on Core → receives JWT → sets an **httpOnly, Secure, SameSite=Strict** cookie.
- Subsequent requests: the BFF reads the cookie, attaches the JWT as a `Bearer` header when calling Core.
- Token refresh: BFF calls `POST /auth/refresh` transparently when the token is near expiry.
- Client components never see or handle raw JWTs.

## Environment Variables

| Variable                 | Purpose                                                           |
| ------------------------ | ----------------------------------------------------------------- |
| `SKILLEXA_CORE_BASE_URL` | Internal URL of Skillexa-Core (e.g., `http://skillexa-core:8080`) |
| `NEXT_PUBLIC_APP_URL`    | Public URL of the portal itself                                   |
| `NODE_ENV`               | `development` / `production`                                      |

## Coding Standards

- Use **Server Components** by default; add `"use client"` only when interactivity is needed.
- Prefer `async` Server Components for data loading.
- Keep client components small and focused on interaction / state.
- All pages under authenticated routes must verify the session server-side before rendering.
- Use TypeScript `strict` — no `any` types without justification.
- Set `"target": "ES2022"` (or later) in `tsconfig.json` to enable modern type-checking (e.g., `Array.at()`, `structuredClone`, `Error.cause`).
- ESLint config uses the **flat config** format (`eslint.config.mjs`) with `eslint-config-next/core-web-vitals` and `eslint-config-next/typescript`.

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
