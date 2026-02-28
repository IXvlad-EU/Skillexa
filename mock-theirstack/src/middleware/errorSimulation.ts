import type { Request, Response, NextFunction } from "express";

const MOCK_FAIL_RATE = Number(process.env["MOCK_FAIL_RATE"] ?? "0");

export function errorSimulation(
  req: Request,
  res: Response,
  next: NextFunction,
): void {
  const mockStatus = req.headers["x-mock-status"];

  if (mockStatus === "429") {
    res.status(429).json({
      error: "Too Many Requests",
      message: "Rate limit exceeded (simulated)",
    });
    return;
  }

  if (mockStatus === "500") {
    res.status(500).json({
      error: "Internal Server Error",
      message: "Server error (simulated)",
    });
    return;
  }

  if (MOCK_FAIL_RATE > 0 && Math.random() < MOCK_FAIL_RATE) {
    res.status(500).json({
      error: "Internal Server Error",
      message: "Random failure (simulated via MOCK_FAIL_RATE)",
    });
    return;
  }

  next();
}
