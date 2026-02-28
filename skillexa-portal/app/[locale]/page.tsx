import { dehydrate, HydrationBoundary } from "@tanstack/react-query";
import { getQueryClient } from "@/app/get-query-client";
import { queryKeys } from "@/lib/hooks/query-keys";
import { HelloMessage } from "@/components/HelloMessage";
import { createApiClient } from "@/lib/api-client/client";

export default async function HomePage() {
  const queryClient = getQueryClient();

  await queryClient.prefetchQuery({
    queryKey: queryKeys.hello,
    queryFn: async () => {
      const client = createApiClient();
      const message = await client.get();
      return message ?? "";
    },
  });

  return (
    <HydrationBoundary state={dehydrate(queryClient)}>
      <HelloMessage />
    </HydrationBoundary>
  );
}
