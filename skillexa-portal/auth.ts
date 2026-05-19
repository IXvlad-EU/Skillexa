import type { AuthOptions, Profile } from "next-auth";
import AzureAD from "next-auth/providers/azure-ad";
import Google from "next-auth/providers/google";
import { signCoreJwt } from "@/lib/auth/core-token";

type ProviderProfile = Profile & {
  preferred_username?: string;
  email_verified?: boolean;
  picture?: string;
};

type ProvisionResponse = {
  userId?: unknown;
};

export const authOptions: AuthOptions = {
  providers: [
    AzureAD({
      clientId: process.env.AUTH_MICROSOFT_ENTRA_ID_ID!,
      clientSecret: process.env.AUTH_MICROSOFT_ENTRA_ID_SECRET!,
      tenantId: process.env.AUTH_MICROSOFT_ENTRA_ID_TENANT_ID!,
      authorization: {
        params: {
          scope: "openid profile email",
        },
      },
    }),
    Google({
      clientId: process.env.AUTH_GOOGLE_ID!,
      clientSecret: process.env.AUTH_GOOGLE_SECRET!,
      authorization: {
        params: {
          scope: "openid profile email",
        },
      },
    }),
  ],
  callbacks: {
    async signIn({ user, account, profile }) {
      if (!account) {
        return false;
      }

      if (account.provider === "google") {
        const googleProfile = profile as ProviderProfile | undefined;
        if (googleProfile?.email_verified !== true) {
          return false;
        }
      }

      return normalizeEmail(user.email ?? getProfileEmail(profile)) !== null;
    },
    async jwt({ token, account, profile, user }) {
      if (account) {
        const email = normalizeEmail(user.email ?? getProfileEmail(profile));
        if (!email) {
          throw new Error("Verified email is required to sign in.");
        }

        const providerSub = `${account.provider}:${account.providerAccountId}`;
        const name = user.name ?? getProfileName(profile) ?? email;
        const image = user.image ?? getProfileImage(profile);

        token.providerSub = providerSub;
        token.email = email;
        token.name = name;
        token.picture = image;
        token.userId = undefined;

        try {
          const bootstrapToken = await signCoreJwt(providerSub, email, name);
          const response = await fetch(new URL("/provision", getCoreBaseUrl()), {
            method: "POST",
            headers: {
              Authorization: `Bearer ${bootstrapToken}`,
            },
          });

          if (response.ok) {
            const data = (await response.json()) as ProvisionResponse;
            const userId = getProvisionUserId(data);
            if (userId !== null) {
              token.userId = userId;
            }
          }
        } catch {
          // Core still provisions lazily on later user-scoped requests.
        }
      }

      return token;
    },
    async session({ session, token }) {
      if (session.user) {
        session.user.email = token.email ?? null;
        session.user.name = token.name ?? null;
        session.user.image = token.picture ?? null;
      }

      return session;
    },
  },
  session: { strategy: "jwt" },
};

function getProfileEmail(profile: Profile | undefined): string | null {
  const providerProfile = profile as ProviderProfile | undefined;
  return providerProfile?.email ?? providerProfile?.preferred_username ?? null;
}

function getProfileName(profile: Profile | undefined): string | null {
  return profile?.name ?? null;
}

function getProfileImage(profile: Profile | undefined): string | null {
  const providerProfile = profile as ProviderProfile | undefined;
  return providerProfile?.picture ?? null;
}

function normalizeEmail(email: string | null | undefined): string | null {
  if (!email || !email.trim()) {
    return null;
  }

  return email.trim().toLowerCase();
}

function getCoreBaseUrl(): string {
  return process.env.SKILLEXA_CORE_BASE_URL ?? "http://localhost:8080";
}

function getProvisionUserId(data: ProvisionResponse): number | null {
  return typeof data.userId === "number" &&
    Number.isFinite(data.userId) &&
    data.userId > 0
    ? data.userId
    : null;
}
