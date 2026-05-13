"use client";

import { useMutation } from "@tanstack/react-query";
import type { components } from "@/lib/api-client/schema";

type CreateDocumentRequest = components["schemas"]["CreateDocumentRequest"];
type CreateDocumentResult = components["schemas"]["CreateDocumentResult"];

export function useCreateDocument() {
  return useMutation<CreateDocumentResult, Error, CreateDocumentRequest>({
    mutationFn: async (request) => {
      const response = await fetch("/api/documents", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
      });
      if (!response.ok) throw new Error("Failed to create document");
      return response.json();
    },
  });
}
