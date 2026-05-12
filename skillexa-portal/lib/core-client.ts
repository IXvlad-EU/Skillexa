import { createSkillexaCoreClient } from "./api-client/skillexaCoreClient";
import { FetchRequestAdapter } from "@microsoft/kiota-http-fetchlibrary";
import {
  AllowedHostsValidator,
  BaseBearerTokenAuthenticationProvider,
} from "@microsoft/kiota-abstractions";

export function createApiClient(accessToken: string) {
  const authProvider = new BaseBearerTokenAuthenticationProvider({
    getAuthorizationToken: async () => accessToken,
    getAllowedHostsValidator: () => new AllowedHostsValidator(),
  });

  const adapter = new FetchRequestAdapter(authProvider);

  adapter.baseUrl =
    process.env.SKILLEXA_CORE_BASE_URL ?? "http://localhost:8080";
    
  return createSkillexaCoreClient(adapter);
}
