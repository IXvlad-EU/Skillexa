"use client";

import { useQuery } from "@tanstack/react-query";
import { queryKeys } from "./query-keys";

/**
 * Fetches the greeting message from Skillexa-Core via the BFF route handler.
 */
export function useHello() {
  return useQuery({
    queryKey: queryKeys.hello,
    queryFn: async () => {
      const res = await fetch("/api/hello");
      if (!res.ok) throw new Error("Failed to fetch greeting");
      return res.text();
    },
  });
}
