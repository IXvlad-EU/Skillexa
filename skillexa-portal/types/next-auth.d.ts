import "next-auth";
import "next-auth/jwt";

declare module "next-auth/jwt" {
  interface JWT {
    /** Provider-scoped subject, formatted as provider:providerAccountId. */
    providerSub?: string;
    /** Core user ID returned by Skillexa-Core provisioning. */
    userId?: number;
  }
}
