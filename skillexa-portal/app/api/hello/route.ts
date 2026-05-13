import { NextResponse } from "next/server";
import { getServerSession } from "next-auth";
import { authOptions } from "@/auth";
import { createApiClient } from "@/lib/core-client";

/**
 * BFF proxy — calls Skillexa-Core's usage endpoint and returns the response.
 * The browser calls this route handler; it never talks to Core directly.
 */
export async function GET() {
  try {
    const session = await getServerSession(authOptions);
    const client = createApiClient(session?.accessToken ?? "");
    const { data, error } = await client.GET("/app/usage");
    if (error) return new NextResponse("Service unavailable", { status: 502 });
    return NextResponse.json(data ?? {});
  } catch {
    return new NextResponse("Service unavailable", { status: 502 });
  }
}
