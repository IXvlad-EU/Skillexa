import {
  AnonymousAuthenticationProvider,
} from "@microsoft/kiota-abstractions";
import { FetchRequestAdapter } from "@microsoft/kiota-http-fetchlibrary";
import {
  createSkillexaCoreClient,
  type SkillexaCoreClient,
} from "./skillexaCoreClient";

/**
 * Creates a server-side Kiota client instance for calling Skillexa-Core.
 *
 * This must only be used in Server Components, Server Actions, or Route Handlers.
 * The browser must never import this module.
 */
export function createApiClient(): SkillexaCoreClient {
  const authProvider = new AnonymousAuthenticationProvider();
  const adapter = new FetchRequestAdapter(authProvider);

  adapter.baseUrl =
    process.env.SKILLEXA_CORE_BASE_URL ?? "http://localhost:8080";

  return createSkillexaCoreClient(adapter);
}
