import type { Request, Response, NextFunction } from "express";
import { errorResponse } from "../utils/errors.js";

const MOCK_API_KEY = process.env["MOCK_API_KEY"] ?? "dev-theirstack-key";

export function auth(req: Request, res: Response, next: NextFunction): void {
  const header = req.headers["authorization"];

  if (!header || !header.startsWith("Bearer ")) {
    res.status(401).json(
      errorResponse(
        "Not allowed exception",
        "Missing or malformed Authorization header. Expected: Bearer <api-key>",
        "E-001",
      ),
    );
    return;
  }

  const token = header.slice("Bearer ".length);

  if (token !== MOCK_API_KEY) {
    res.status(401).json(
      errorResponse("Not allowed exception", "Invalid API key", "E-001"),
    );
    return;
  }

  next();
}
