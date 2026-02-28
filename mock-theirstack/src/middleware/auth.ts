import type { Request, Response, NextFunction } from "express";

const MOCK_API_KEY = process.env["MOCK_API_KEY"] ?? "dev-theirstack-key";

export function auth(req: Request, res: Response, next: NextFunction): void {
  const header = req.headers["authorization"];

  if (!header || !header.startsWith("Bearer ")) {
    res.status(401).json({
      error: "Unauthorized",
      message:
        "Missing or malformed Authorization header. Expected: Bearer <api-key>",
    });
    return;
  }

  const token = header.slice("Bearer ".length);

  if (token !== MOCK_API_KEY) {
    res.status(401).json({
      error: "Unauthorized",
      message: "Invalid API key",
    });
    return;
  }

  next();
}
