import createClient, { type Middleware } from "openapi-fetch";
import type { paths } from "./api-client/schema";

export function createApiClient(accessToken: string) {
  const client = createClient<paths>({
    baseUrl: process.env.SKILLEXA_CORE_BASE_URL ?? "http://localhost:8080",
  });

  const authMiddleware: Middleware = {
    async onRequest({ request }) {
      request.headers.set("Authorization", `Bearer ${accessToken}`);
      return request;
    },
  };

  client.use(authMiddleware);
  return client;
}
