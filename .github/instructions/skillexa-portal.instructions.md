---
description: "Next.js SSR portal with Mantine UI, TanStack Query, and Kiota client"
applyTo: "skillexa-portal/**"
---

# Skillexa-Portal — Instructions

## Stack

| Concern         | Choice                                                              |
| --------------- | ------------------------------------------------------------------- |
| Framework       | Next.js 16 (App Router, SSR)                                        |
| Language        | TypeScript (strict mode)                                            |
| React           | 19.x                                                                |
| UI library      | **Mantine 7** — see `mantine-ui.instructions.md`                    |
| Styling         | Mantine CSS modules + PostCSS                                       |
| Data fetching   | TanStack Query — see `tanstack-query.instructions.md`               |
| API client      | **Kiota**-generated TypeScript client — see `kiota.instructions.md` |
| Output          | `standalone` (Docker-friendly)                                      |
| Package manager | pnpm (workspace)                                                    |

---

## Architecture: SSR + BFF

- The portal uses **Server-Side Rendering** via the Next.js App Router.
- It also acts as a **Backend-for-Frontend (BFF)**: the browser never talks directly to Skillexa-Core. All API calls go through Next.js Route Handlers or Server Actions, which forward them to Skillexa-Core internally (server-to-server).
- Benefits: no CORS issues, JWT tokens stay on the server side (httpOnly cookies), reduced client bundle.

---

## Folder Conventions (App Router)

```
app/
  get-query-client.ts # QueryClient factory (isServer singleton pattern)
  providers.tsx       # client provider (QueryClientProvider + DevTools)
  layout.tsx          # root layout (fonts, providers, global styles)
  page.tsx            # landing / home page
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

---

## Auth Flow

- Sign-in: user clicks "Sign in" → `next-auth` (v4) redirects to **Microsoft Entra ID** OIDC login → Entra ID returns auth code → `next-auth` exchanges it for tokens server-side → session stored in an **httpOnly, Secure, SameSite=Strict** cookie.
- Subsequent requests: the BFF reads the session via `getServerSession(authOptions)`, attaches the Entra ID access token as a `Bearer` header when calling Core.
- Token refresh: `next-auth` handles token refresh with Entra ID silently.
- Client components use `useSession()` from `next-auth/react` for auth state (user name, loading status). The app is wrapped in `<SessionProvider>` inside `app/providers.tsx`.
- Client components never see or handle raw access tokens.
- See `authentication.instructions.md` for full Entra ID configuration.

## Environment Variables

| Variable                            | Purpose                                                                 |
| ----------------------------------- | ----------------------------------------------------------------------- |
| `SKILLEXA_CORE_BASE_URL`            | Internal URL of Skillexa-Core (e.g., `http://skillexa-core:8080`)       |
| `NEXTAUTH_URL`                      | Public URL of the portal (e.g., `http://localhost:3000`)                |
| `NEXTAUTH_SECRET`                   | Random secret for encrypting the next-auth session cookie               |
| `AUTH_MICROSOFT_ENTRA_ID_ID`        | Skillexa-Portal-Web Entra ID Application (client) ID                    |
| `AUTH_MICROSOFT_ENTRA_ID_SECRET`    | Skillexa-Portal-Web client secret                                       |
| `AUTH_MICROSOFT_ENTRA_ID_TENANT_ID` | Entra ID Directory (tenant) ID                                          |
| `AZURE_AD_API_SCOPE`                | Skillexa-Core API scope (e.g., `api://<Core-Client-ID>/access_as_user`) |
| `NEXT_PUBLIC_APP_URL`               | Public URL of the portal itself (if needed client-side)                 |
| `NODE_ENV`                          | `development` / `production`                                            |

---

## Related Instructions

- [mantine-ui.instructions.md](mantine-ui.instructions.md) — Mantine 7 setup, components, and styling
- [tanstack-query.instructions.md](tanstack-query.instructions.md) — TanStack Query hooks and SSR prefetching
- [kiota.instructions.md](kiota.instructions.md) — Kiota API client generation and usage
- [nextjs.instructions.md](nextjs.instructions.md) — Next.js best practices and coding standards
- [authentication.instructions.md](authentication.instructions.md) — Microsoft Entra ID auth flow
