import { NextResponse } from "next/server";
import { getServerSession } from "next-auth";
import { authOptions } from "@/auth";
import { createApiClient } from "@/lib/core-client";
import type { components } from "@/lib/api-client/schema";

type SearchJobListingsRequest = components["schemas"]["SearchJobListingsRequest"];

export async function POST(request: Request) {
  const session = await getServerSession(authOptions);
  // TODO: Re-enable auth check once Entra ID app registrations are configured
  if (!session && process.env.AUTH_REQUIRED !== "false") {
    return new NextResponse("Unauthorized", { status: 401 });
  }

  const body: SearchJobListingsRequest = await request.json();
  const client = createApiClient(session?.accessToken ?? "");
  const { data, error } = await client.POST("/job-listings/search", {
    body,
  });

  if (error) {
    return new NextResponse("Service unavailable", { status: 502 });
  }

  return NextResponse.json(data ?? []);
}
