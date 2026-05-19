# Skillexa-Portal — NextAuth BFF Authentication

Skillexa-Portal authenticates users with Microsoft or Google through `next-auth`, stores the user session in an encrypted httpOnly cookie, and signs short-lived Core JWTs only on the server.

## Packages

```bash
pnpm add jose
```

`next-auth` remains the OAuth/session layer. `jose` signs RS256 JWTs for Skillexa-Core.

## Environment Variables

```env
NEXTAUTH_SECRET=<random-32-byte-secret-for-session-encryption>
NEXTAUTH_URL=http://localhost:3000

AUTH_MICROSOFT_ENTRA_ID_ID=<Portal Microsoft client ID>
AUTH_MICROSOFT_ENTRA_ID_SECRET=<Portal Microsoft client secret>
AUTH_MICROSOFT_ENTRA_ID_TENANT_ID=<Directory tenant ID>

AUTH_GOOGLE_ID=<Google OAuth client ID>
AUTH_GOOGLE_SECRET=<Google OAuth client secret>

JWT_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----"
```

PEM values may contain literal `\n`; the signing helper normalizes them before importing the key.

## NextAuth Rules

- Configure Microsoft (`azure-ad`) and Google (`google`) providers.
- Request only `openid profile email`.
- Do not store provider access tokens in the NextAuth JWT.
- Do not expose Core JWTs through `session`.
- Persist only identity metadata needed for server-side Core calls:
  - `providerSub = "{provider}:{providerAccountId}"`
  - normalized `email`
  - `name`
  - `userId` returned by Core provisioning
- A provider avatar URL may be forwarded as `session.user.image` for browser UI only. It is not used for Core calls and must not require storing provider access tokens.

Google sign-in requires `email_verified === true`. Microsoft sign-in requires an email from NextAuth/profile data.

## Calling Skillexa-Core

On initial OAuth sign-in, the NextAuth `jwt` callback should:

1. Persist `providerSub`, normalized `email`, and `name` in the encrypted NextAuth JWT.
2. Sign a short-lived bootstrap Core JWT without `uid`.
3. Call Core `POST /provision`.
4. Store the returned Core `userId` in the encrypted NextAuth JWT when provisioning succeeds.

Provisioning failures must not fail sign-in. Core keeps a lazy email-based fallback for old sessions, bootstrap tokens, and failed login-time provisioning.

Route handlers call a server-only helper that:

1. Reads the encrypted NextAuth JWT with `getToken()`.
2. Rejects missing `providerSub` or email.
3. Signs a fresh RS256 JWT with `JWT_PRIVATE_KEY`.
4. Sets `iss = skillexa-portal`, `aud = skillexa-core`, and a 15-minute expiry.
5. Includes `uid` only when `userId` exists in the encrypted NextAuth JWT.

The browser calls Portal route handlers. The browser may receive display-only profile metadata such as `name`, `email`, and `image`, but must never receive provider access tokens, Core JWTs, Core `userId`, or signing keys.

## Expiry and Active Sessions

Do not implement browser-visible refresh tokens for Core JWTs. The Core JWT is intentionally short-lived and server-only.

When a user is still active on the page:

1. The browser calls a Portal BFF route.
2. The BFF reads the current encrypted NextAuth session cookie.
3. If the session is still valid, the BFF signs a brand-new 15-minute Core JWT for that request, including `uid` when available.
4. If the NextAuth session has expired, the route returns `401` and the UI should send the user through normal sign-in again.

Auth.js provider access-token refresh is only needed when the app stores and uses provider API access tokens. Skillexa does not do that; Core never receives Microsoft or Google access tokens.

## Sign-In UI

Use two explicit buttons:

- Microsoft: `signIn("azure-ad")`
- Google: `signIn("google")`

All labels must come from `messages/en.json`, `messages/ru.json`, and `messages/de.json`.
