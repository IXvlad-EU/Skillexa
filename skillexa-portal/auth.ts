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
