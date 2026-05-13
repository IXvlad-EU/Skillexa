import { Router, type IRouter } from "express";
import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";
import { logger } from "../utils/logger.js";
import { errorResponse } from "../utils/errors.js";

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
    res.status(422).json(
      errorResponse(
        "Validation error",
        "At least one of posted_at_max_age_days, posted_at_gte, posted_at_lte, company_domain_or, company_linkedin_url_or, company_name_or is required",
        "E-007",
      ),
    );
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

  // Job title — substring match (case-insensitive)
  const titleFilter = Array.isArray(body["job_title_or"])
    ? (body["job_title_or"] as string[])
    : [];

  if (titleFilter.length > 0) {
    filtered = filtered.filter((job) =>
      titleFilter.some((t) =>
        (job["job_title"] as string | undefined)
          ?.toLowerCase()
          .includes(t.toLowerCase()),
      ),
    );
  }

  // Description keywords — whole-word, case-insensitive
  const descFilter = Array.isArray(body["job_description_contains_or"])
    ? (body["job_description_contains_or"] as string[])
    : [];

  if (descFilter.length > 0) {
    filtered = filtered.filter((job) => {
      const desc = (job["description"] as string | undefined) ?? "";
      return descFilter.some((word) =>
        new RegExp(`\\b${word}\\b`, "i").test(desc),
      );
    });
  }

  // Remote
  if (body["remote"] !== undefined && body["remote"] !== null) {
    const wantRemote = body["remote"] as boolean;
    filtered = filtered.filter((job) => job["remote"] === wantRemote);
  }

  // Seniority
  const seniorityFilter = Array.isArray(body["job_seniority_or"])
    ? (body["job_seniority_or"] as string[])
    : [];

  if (seniorityFilter.length > 0) {
    filtered = filtered.filter((job) =>
      seniorityFilter.includes(job["seniority"] as string),
    );
  }

  // Employment type
  const employmentFilter = Array.isArray(body["employment_statuses_or"])
    ? (body["employment_statuses_or"] as string[])
    : [];

  if (employmentFilter.length > 0) {
    filtered = filtered.filter((job) => {
      const statuses = job["employment_statuses"] as string[] | undefined;
      return employmentFilter.some((s) => statuses?.includes(s));
    });
  }

  // Country (ISO2)
  const countryFilter = Array.isArray(body["job_country_code_or"])
    ? (body["job_country_code_or"] as string[])
    : [];

  if (countryFilter.length > 0) {
    filtered = filtered.filter((job) => {
      const codes = job["country_codes"] as string[] | undefined;
      const primary = job["country_code"] as string | undefined;
      return countryFilter.some(
        (code) => codes?.includes(code) || primary === code,
      );
    });
  }

  // Salary
  const minSalary =
    typeof body["min_salary_usd"] === "number"
      ? (body["min_salary_usd"] as number)
      : null;
  const maxSalary =
    typeof body["max_salary_usd"] === "number"
      ? (body["max_salary_usd"] as number)
      : null;

  if (minSalary !== null) {
    filtered = filtered.filter((job) => {
      const val = job["min_annual_salary_usd"] as number | undefined;
      return val !== undefined && val >= minSalary;
    });
  }
  if (maxSalary !== null) {
    filtered = filtered.filter((job) => {
      const val = job["max_annual_salary_usd"] as number | undefined;
      return val !== undefined && val <= maxSalary;
    });
  }

  // Posted within N days
  const maxAgeDays =
    typeof body["posted_at_max_age_days"] === "number"
      ? (body["posted_at_max_age_days"] as number)
      : null;

  if (maxAgeDays !== null) {
    const cutoff = new Date();
    cutoff.setDate(cutoff.getDate() - maxAgeDays);
    filtered = filtered.filter((job) => {
      const posted = job["date_posted"] as string | undefined;
      if (!posted) return false;
      return new Date(posted) >= cutoff;
    });
  }

  // Company name — partial match (case-insensitive)
  const companyFilter = Array.isArray(body["company_name_partial_match_or"])
    ? (body["company_name_partial_match_or"] as string[])
    : [];

  if (companyFilter.length > 0) {
    filtered = filtered.filter((job) =>
      companyFilter.some((name) =>
        (job["company"] as string | undefined)
          ?.toLowerCase()
          .includes(name.toLowerCase()),
      ),
    );
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
