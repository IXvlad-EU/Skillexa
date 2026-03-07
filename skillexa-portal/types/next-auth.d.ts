import "next-auth";
import "next-auth/jwt";

declare module "next-auth" {
  interface Session {
    /** Entra ID access token for calling Skillexa-Core API */
    accessToken: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    /** Entra ID access token */
    accessToken?: string;
    /** Token expiry (epoch seconds) */
    expiresAt?: number;
  }
}
