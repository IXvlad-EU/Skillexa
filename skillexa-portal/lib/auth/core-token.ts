import type { NextRequest } from "next/server";
import { getToken } from "next-auth/jwt";
import { SignJWT, importPKCS8 } from "jose";

const issuer = "skillexa-portal";
const audience = "skillexa-core";
const expirationTime = "15m";

let privateKeyPromise: ReturnType<typeof importPKCS8> | undefined;

export async function createCoreAccessToken(
  request: NextRequest,
): Promise<string | null> {
  const token = await getToken({
    req: request,
    secret: process.env.NEXTAUTH_SECRET,
  });

  if (!token?.providerSub || !token.email) {
    return null;
  }

  const email = normalizeEmail(token.email);
  const name = normalizeName(token.name) ?? email;
  const uid =
    typeof token.userId === "number" && Number.isFinite(token.userId)
      ? token.userId
      : undefined;

  return signCoreJwt(token.providerSub, email, name, uid);
}

export async function signCoreJwt(
  sub: string,
  email: string,
  name: string,
  uid?: number,
): Promise<string> {
  const normalizedEmail = normalizeEmail(email);
  const normalizedName = normalizeName(name) ?? normalizedEmail;
  const payload: Record<string, string | number> = {
    email: normalizedEmail,
    name: normalizedName,
  };

  if (uid !== undefined && Number.isFinite(uid) && uid > 0) {
    payload.uid = uid;
  }

  return new SignJWT(payload)
    .setProtectedHeader({
      alg: "RS256",
      typ: "JWT",
    })
    .setIssuer(issuer)
    .setAudience(audience)
    .setSubject(sub)
    .setIssuedAt()
    .setExpirationTime(expirationTime)
    .sign(await getPrivateKey());
}

async function getPrivateKey() {
  privateKeyPromise ??= importPKCS8(getRequiredPrivateKey(), "RS256");
  return privateKeyPromise;
}

function getRequiredPrivateKey() {
  const privateKey = process.env.JWT_PRIVATE_KEY;
  if (!privateKey?.trim()) {
    throw new Error("JWT_PRIVATE_KEY is required.");
  }

  return normalizePem(privateKey);
}

function normalizePem(value: string) {
  return value.replaceAll("\\n", "\n").trim();
}

function normalizeEmail(email: string) {
  return email.trim().toLowerCase();
}

function normalizeName(name: unknown) {
  return typeof name === "string" && name.trim() ? name.trim() : null;
}
