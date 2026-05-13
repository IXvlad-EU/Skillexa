"use client";

import { useMutation } from "@tanstack/react-query";
import type { components } from "@/lib/api-client/schema";

type SearchJobListingsRequest = components["schemas"]["SearchJobListingsRequest"];
type SearchJobListingsResult = components["schemas"]["SearchJobListingsResult"];

export function useJobSearch() {
  return useMutation<SearchJobListingsResult[], Error, SearchJobListingsRequest>(
    {
      mutationFn: async (request) => {
        const response = await fetch("/api/jobs/search", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(request),
        });
        if (!response.ok) throw new Error("Failed to search jobs");
        return response.json();
      },
    },
  );
}
