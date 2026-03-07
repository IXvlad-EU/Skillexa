import { NextResponse } from "next/server";
import { getServerSession } from "next-auth";
import { authOptions } from "@/auth";
import { createApiClient } from "@/lib/api-client/client";

/**
 * BFF proxy — calls Skillexa-Core's root endpoint and returns the response.
 * The browser calls this route handler; it never talks to Core directly.
 */
export async function GET() {
  try {
    const session = await getServerSession(authOptions);
    const client = createApiClient(session?.accessToken ?? "");
    const message = await client.get();

    return new NextResponse(message ?? "", {
      status: 200,
      headers: { "Content-Type": "text/plain" },
    });
  } catch {
    return new NextResponse("Service unavailable", { status: 502 });
  }
}
