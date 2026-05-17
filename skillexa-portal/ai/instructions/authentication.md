# Skillexa-Portal — Confidential Client (Next.js BFF)

## Package

```
next-auth   (v4, as installed in `package.json`)
```

Use the **Azure AD** provider built into `next-auth`.

## Environment Variables

```env
# .env (development — gitignored)
NEXTAUTH_SECRET=<random-32-byte-secret-for-session-encryption>
NEXTAUTH_URL=http://localhost:3000
AUTH_MICROSOFT_ENTRA_ID_ID=<Skillexa-Portal-Web Application (client) ID>
AUTH_MICROSOFT_ENTRA_ID_SECRET=<Skillexa-Portal-Web client secret>
AUTH_MICROSOFT_ENTRA_ID_TENANT_ID=<Directory (tenant) ID>

# Scope to request access token for Skillexa-Core API
AZURE_AD_API_SCOPE=api://<Skillexa-Core-API-Client-ID>/access_as_user
```

## NextAuth Configuration — `auth.ts`

```ts
import type { AuthOptions } from "next-auth";
import AzureAD from "next-auth/providers/azure-ad";

export const authOptions: AuthOptions = {
  providers: [
    AzureAD({
      clientId: process.env.AUTH_MICROSOFT_ENTRA_ID_ID!,
      clientSecret: process.env.AUTH_MICROSOFT_ENTRA_ID_SECRET!,
      tenantId: process.env.AUTH_MICROSOFT_ENTRA_ID_TENANT_ID!,
      authorization: {
        params: {
          scope: `openid profile email ${process.env.AZURE_AD_API_SCOPE}`,
        },
      },
    }),
  ],
  callbacks: {
    async jwt({ token, account }) {
      // Persist the access token from the initial sign-in
      if (account) {
        token.accessToken = account.access_token;
        token.expiresAt = account.expires_at;
      }
      return token;
    },
    async session({ session, token }) {
      // Make the access token available in the session (server-side only)
      session.accessToken = token.accessToken as string;
      return session;
    },
  },
  session: { strategy: "jwt" },
};
```

## Route Handler — `app/api/auth/[...nextauth]/route.ts`

```ts
import NextAuth from "next-auth";
import { authOptions } from "@/auth";

const handler = NextAuth(authOptions);

export { handler as GET, handler as POST };
```

## Calling Skillexa-Core from Server Components / Route Handlers

```ts
import { getServerSession } from "next-auth";
import { authOptions } from "@/auth";

const session = await getServerSession(authOptions);
const response = await fetch(`${process.env.SKILLEXA_CORE_BASE_URL}/documents`, {
  headers: {
    Authorization: `Bearer ${session?.accessToken}`,
  },
});
```

When using the **openapi-fetch client**, pass the access token from the session to the `createApiClient(accessToken)` factory:

```ts
import { getServerSession } from "next-auth";
import { authOptions } from "@/auth";
import { createApiClient } from "@/lib/core-client";

const session = await getServerSession(authOptions);
const client = createApiClient(session?.accessToken ?? "");
```

## Sign-In / Sign-Out UI

```tsx
import { signIn, signOut } from "next-auth/react";

// Sign in — redirects to Entra ID login page
<Button onClick={() => signIn("azure-ad")}>Sign in</Button>

// Sign out — clears session + redirects to Entra ID logout
<Button onClick={() => signOut()}>Sign out</Button>
```

Use the `useSession()` hook from `next-auth/react` in client components to access session state (user name, auth status). Wrap the app in `<SessionProvider>` (inside `app/providers.tsx`).

---

## Token Validation Rules (Skillexa-Core)

`Microsoft.Identity.Web` handles these automatically, but be aware:

1. **Signature** — validated against Entra ID's public signing keys (fetched from the OIDC discovery endpoint).
2. **Issuer** — must match `https://login.microsoftonline.com/{tenant-id}/v2.0`.
3. **Audience** — must match `api://<Core-API client ID>` (the `aud` claim).
4. **Expiry** — tokens past `exp` are rejected.
5. **Token version** — v2.0 tokens (configured via `accessTokenAcceptedVersion: 2` in the app manifest).

Do **not** manually validate tokens or parse JWTs in application code. Rely on `Microsoft.Identity.Web` middleware.
