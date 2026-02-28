import type { Request, Response, NextFunction } from "express";

const MOCK_DEFAULT_DELAY_MS = Number(
  process.env["MOCK_DEFAULT_DELAY_MS"] ?? "0",
);

export async function delay(
  req: Request,
  _res: Response,
  next: NextFunction,
): Promise<void> {
  const headerDelay = req.headers["x-mock-delay"];
  const ms = headerDelay ? Number(headerDelay) : MOCK_DEFAULT_DELAY_MS;

  if (ms > 0) {
    await new Promise<void>((resolve) => setTimeout(resolve, ms));
  }

  next();
}
