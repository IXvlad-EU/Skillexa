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
  source_url?: string | null;
  technology_slugs?: string[];
  company_object?: { id: string };
  [key: string]: unknown;
}

const REQUIRED_FILTERS = [
  "posted_at_max_age_days",
  "posted_at_gte",
  "posted_at_lte",
  "company_domain_or",
  "company_linkedin_url_or",
  "company_name_or",
] as const;

const fixturesPath = join(__dirname, "..", "fixtures", "jobs.json");
const jobs: Job[] = JSON.parse(readFileSync(fixturesPath, "utf-8")) as Job[];

logger.info({ count: jobs.length }, "Loaded job fixtures");

const distinctCompanies = new Set(jobs.map((j) => j.company));

export const jobsRouter: IRouter = Router();

jobsRouter.post("/v1/jobs/search", (req, res) => {
  const body = req.body as Record<string, unknown>;

  // 422 — at least one required filter must be present
  const hasRequiredFilter = REQUIRED_FILTERS.some(
    (field) => body[field] !== undefined && body[field] !== null,
  );
  if (!hasRequiredFilter) {
    res.status(422).json({
      detail: [
        {
          msg: "At least one of posted_at_max_age_days, posted_at_gte, posted_at_lte, company_domain_or, company_linkedin_url_or, company_name_or is required",
        },
      ],
    });
    return;
  }

  const page = typeof body["page"] === "number" ? body["page"] : 0;
  const limit = typeof body["limit"] === "number" ? body["limit"] : 25;

  // Skill filter — case-sensitive, any-match
  const slugFilter = Array.isArray(body["job_technology_slug_or"])
    ? (body["job_technology_slug_or"] as string[])
    : [];

  // Source domain filter — case-insensitive substring match
  const domainFilter = Array.isArray(body["url_domain_or"])
    ? (body["url_domain_or"] as string[])
    : [];

  let filtered = jobs;

  if (slugFilter.length > 0) {
    filtered = filtered.filter((job) =>
      slugFilter.some((slug) => job.technology_slugs?.includes(slug)),
    );
  }

  if (domainFilter.length > 0) {
    filtered = filtered.filter((job) => {
      const url = job.source_url;
      if (!url) return false;
      return domainFilter.some((domain) =>
        url.toLowerCase().includes(domain.toLowerCase()),
      );
    });
  }

  const totalResults = filtered.length;
  const start = page * limit;
  const end = start + limit;
  const sliced = filtered.slice(start, end);

  res.json({
    metadata: {
      total_results: totalResults,
      truncated_results: 0,
      truncated_companies: 0,
      total_companies: distinctCompanies.size,
    },
    data: sliced,
  });
});
