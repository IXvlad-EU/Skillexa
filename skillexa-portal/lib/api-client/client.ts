import {
  type AuthenticationProvider,
  type RequestInformation,
} from "@microsoft/kiota-abstractions";
import { FetchRequestAdapter } from "@microsoft/kiota-http-fetchlibrary";
import {
  createSkillexaCoreClient,
  type SkillexaCoreClient,
} from "./skillexaCoreClient";

/**
 * Authentication provider that attaches a Bearer token to every request.
 */
class BearerTokenAuthProvider implements AuthenticationProvider {
  constructor(private readonly accessToken: string) {}

  async authenticateRequest(request: RequestInformation): Promise<void> {
    if (this.accessToken) {
      request.headers.add("Authorization", `Bearer ${this.accessToken}`);
    }
  }
}

/**
 * Creates a server-side Kiota client instance for calling Skillexa-Core.
 *
 * This must only be used in Server Components, Server Actions, or Route Handlers.
 * The browser must never import this module.
 *
 * @param accessToken - The Entra ID access token from the current session.
 */
export function createApiClient(accessToken: string): SkillexaCoreClient {
  const authProvider = new BearerTokenAuthProvider(accessToken);
  const adapter = new FetchRequestAdapter(authProvider);

  adapter.baseUrl =
    process.env.SKILLEXA_CORE_BASE_URL ?? "http://localhost:8080";

  return createSkillexaCoreClient(adapter);
}
