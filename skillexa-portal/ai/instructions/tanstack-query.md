# TanStack Query — Instructions

TanStack Query (React Query) is the data-fetching and caching layer for Skillexa Portal.

---

## QueryClient Setup (App Router — `isServer` singleton pattern)

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

### Example `get-query-client.ts`

```ts
import { QueryClient, isServer } from "@tanstack/react-query";

function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 60 * 1000,
      },
      dehydrate: {
        shouldDehydrateQuery: (query) =>
          query.state.status === "success" || query.state.status === "pending",
      },
    },
  });
}

let browserQueryClient: QueryClient | undefined = undefined;

export function getQueryClient() {
  if (isServer) {
    return makeQueryClient();
  }
  if (!browserQueryClient) {
    browserQueryClient = makeQueryClient();
  }
  return browserQueryClient;
}
```

---

## Provider

- Wrap the app in a `QueryClientProvider` inside a `"use client"` provider component (`app/providers.tsx`).
- Include `ReactQueryDevtools` inside the provider (tree-shaken in production).

```tsx
"use client";

import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { getQueryClient } from "./get-query-client";

export function Providers({ children }: { children: React.ReactNode }) {
  const queryClient = getQueryClient();

  return (
    <QueryClientProvider client={queryClient}>
      {children}
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  );
}
```

---

## Query Keys

- Define query keys as constants in a central file (`lib/hooks/query-keys.ts`).
- Use a hierarchical structure (e.g., `queryKeys.documents.all`, `queryKeys.jobSearch.results(filters)`).

```ts
export const queryKeys = {
  jobSearch: {
    all: ["job-search"] as const,
    results: (filters: JobSearchFilters) =>
      [...queryKeys.jobSearch.all, filters] as const,
  },
  documents: {
    all: ["documents"] as const,
  },
};
```

---

## Hooks

- Each domain entity gets its own hook file (e.g., `useJobs.ts`, `useDocuments.ts`) exporting `useQuery` / `useMutation` hooks.
- Hooks are `"use client"` and call BFF route handlers (`/api/*`) — never call Skillexa-Core directly.
- All mutations that create or modify data should use `useMutation` with proper `onSuccess` invalidation.

```tsx
"use client";

import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { queryKeys } from "./query-keys";

export function useJobSearch(filters: JobSearchFilters) {
  return useQuery({
    queryKey: queryKeys.jobSearch.results(filters),
    queryFn: () =>
      fetch("/api/jobs/search", {
        method: "POST",
        body: JSON.stringify(filters),
      }).then((res) => res.json()),
  });
}

export function useCreateDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateDocumentInput) =>
      fetch("/api/documents", {
        method: "POST",
        body: JSON.stringify(data),
      }).then((res) => res.json()),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.documents.all });
    },
  });
}
```

---

## Server-Side Prefetching (SSR)

- In **Server Components** (e.g., `page.tsx`), call `getQueryClient()` and use `queryClient.prefetchQuery(...)` to prefetch data on the server.
- Wrap the client subtree with `<HydrationBoundary state={dehydrate(queryClient)}>` to pass prefetched data to the client cache.
- Prefetch `queryFn` should call the openapi-fetch Core client directly (server-to-server), **not** the BFF `/api/*` routes.
- Use `await` on `prefetchQuery` for critical content; omit `await` for non-critical data to enable streaming.
- Each page/layout that prefetches needs its own `<HydrationBoundary>` — this cannot be hoisted to a shared layout.

### Example Server Component with Prefetching

```tsx
import { dehydrate, HydrationBoundary } from "@tanstack/react-query";
import { getQueryClient } from "@/app/get-query-client";
import { createApiClient } from "@/lib/core-client";
import { queryKeys } from "@/lib/hooks/query-keys";
import { DocumentsList } from "./DocumentsList";

export default async function DocumentsPage() {
  const queryClient = getQueryClient();
  const client = createApiClient(/* session access token */);

  await queryClient.prefetchQuery({
    queryKey: queryKeys.documents.all,
    queryFn: () => client.GET("/documents").then(({ data }) => data ?? []),
  });

  return (
    <HydrationBoundary state={dehydrate(queryClient)}>
      <DocumentsList />
    </HydrationBoundary>
  );
}
```
