import { NextRequest, NextResponse } from "next/server";
import { createApiClient } from "@/lib/core-client";
import { createCoreAccessToken } from "@/lib/auth/core-token";
import type { components } from "@/lib/api-client/schema";

type CreateDocumentRequest = components["schemas"]["CreateDocumentRequest"];

export async function POST(request: NextRequest) {
  const accessToken = await createCoreAccessToken(request);
  if (!accessToken) {
    return new NextResponse("Unauthorized", { status: 401 });
  }

  const body: CreateDocumentRequest = await request.json();
  const client = createApiClient(accessToken);
  const { data, error } = await client.POST("/documents", {
    body,
  });

  if (error) {
    return new NextResponse("Service unavailable", { status: 502 });
  }

  return NextResponse.json(data, { status: 201 });
}
