import { Router, type IRouter } from "express";
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import { logger } from "../utils/logger.js";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

interface Job {
  id: number;
  company: string;
  company_object?: { id: string };
  [key: string]: unknown;
}

const fixturesPath = join(__dirname, "..", "fixtures", "jobs.json");
const jobs: Job[] = JSON.parse(readFileSync(fixturesPath, "utf-8")) as Job[];

logger.info({ count: jobs.length }, "Loaded job fixtures");

const distinctCompanies = new Set(jobs.map((j) => j.company));

export const jobsRouter: IRouter = Router();

jobsRouter.post("/v1/jobs/search", (req, res) => {
  const body = req.body as Record<string, unknown>;
  const page = typeof body["page"] === "number" ? body["page"] : 0;
  const limit = typeof body["limit"] === "number" ? body["limit"] : 25;

  const start = page * limit;
  const end = start + limit;
  const sliced = jobs.slice(start, end);

  res.json({
    metadata: {
      total_results: jobs.length,
      truncated_results: 0,
      truncated_companies: 0,
      total_companies: distinctCompanies.size,
    },
    data: sliced,
  });
});
