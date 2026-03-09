---
description: "Kiota-generated TypeScript API client for consuming Skillexa-Core OpenAPI spec"
applyTo: "skillexa-portal/**"
---

# Kiota API Client — Instructions

Skillexa Portal uses **Microsoft Kiota** to generate a typed TypeScript HTTP client from the Skillexa-Core OpenAPI specification.

---

## Overview

- Skillexa-Core exposes an **OpenAPI specification** (e.g., `/openapi.json`).
- Kiota generates a fully typed, tree-shakeable TypeScript client from that spec.
- The generated client provides type-safe methods for all API endpoints with request/response models.

---

## Generated Client Location

- Place generated client code in `skillexa-portal/lib/api-client/`.
- Structure:
  ```
  lib/
    api-client/
      client.ts          # Factory function to create the client instance
      models/            # Generated request/response models
      api/               # Generated API endpoint methods
      kiota-lock.json    # Kiota lock file (tracks generation settings)
  ```

---

## Regenerating the Client

- Regenerate the client whenever the Core OpenAPI spec changes.
- Add an npm script to `package.json`:

```json
{
  "scripts": {
    "generate:api-client": "kiota generate -l typescript -d http://localhost:8080/openapi.json -c ApiClient -o ./lib/api-client"
  }
}
```

- Run: `pnpm generate:api-client`

---

## Client Factory

Create a factory function that accepts the access token and returns a configured client instance:

```ts
// lib/api-client/client.ts
import { ApiClient } from "./apiClient";
import {
  FetchRequestAdapter,
  HttpClient,
} from "@microsoft/kiota-http-fetchlibrary";
import {
  AnonymousAuthenticationProvider,
  BaseBearerTokenAuthenticationProvider,
} from "@microsoft/kiota-abstractions";

export function createApiClient(accessToken: string): ApiClient {
  const authProvider = {
    getAuthorizationToken: async () => accessToken,
  };

  const adapter = new FetchRequestAdapter(
    new BaseBearerTokenAuthenticationProvider(authProvider),
  );

  adapter.baseUrl = process.env.SKILLEXA_CORE_BASE_URL!;

  return new ApiClient(adapter);
}
```

---

## Usage Rules

### Server-Side Only

The Kiota client is used **exclusively on the server side**:

- ✅ Route Handlers (`app/api/*/route.ts`)
- ✅ Server Components (`page.tsx`, `layout.tsx`)
- ✅ Server Actions

**Never import the Kiota client in client components.** Client components should call BFF route handlers (`/api/*`) instead.

### Authentication

Pass the Entra ID access token from the session to the client factory:

```ts
import { getServerSession } from "next-auth";
import { authOptions } from "@/auth";
import { createApiClient } from "@/lib/api-client/client";

const session = await getServerSession(authOptions);
const client = createApiClient(session?.accessToken ?? "");

// Use the client
const jobs = await client.jobs.get();
```

### In Route Handlers

```ts
// app/api/jobs/route.ts
import { NextResponse } from "next/server";
import { getServerSession } from "next-auth";
import { authOptions } from "@/auth";
import { createApiClient } from "@/lib/api-client/client";

export async function GET() {
  const session = await getServerSession(authOptions);
  if (!session?.accessToken) {
    return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
  }

  const client = createApiClient(session.accessToken);
  const jobs = await client.jobs.get();

  return NextResponse.json(jobs);
}
```

### In Server Components (Prefetching)

```ts
// app/(dashboard)/jobs/page.tsx
import { getServerSession } from "next-auth";
import { authOptions } from "@/auth";
import { createApiClient } from "@/lib/api-client/client";
import { getQueryClient } from "@/app/get-query-client";
import { queryKeys } from "@/lib/hooks/query-keys";

export default async function JobsPage() {
  const session = await getServerSession(authOptions);
  const client = createApiClient(session?.accessToken ?? "");
  const queryClient = getQueryClient();

  await queryClient.prefetchQuery({
    queryKey: queryKeys.jobs.lists(),
    queryFn: () => client.jobs.get(),
  });

  // ... render with HydrationBoundary
}
```

---

## Error Handling

The Kiota client throws typed errors. Wrap calls in try/catch and handle appropriately:

```ts
try {
  const job = await client.jobs.byJobId(jobId).get();
  return job;
} catch (error) {
  if (error instanceof ApiError) {
    // Handle specific API errors
    console.error("API error:", error.message, error.responseStatusCode);
  }
  throw error;
}
```

---

## Best Practices

- **Keep the client up to date**: Regenerate after any Core API changes to maintain type safety.
- **Use the lock file**: Commit `kiota-lock.json` to source control to ensure consistent generation.
- **Don't modify generated code**: Any customizations should go in wrapper functions, not in the generated files.
- **Type safety**: Leverage the generated types in your application code for full end-to-end type safety.
