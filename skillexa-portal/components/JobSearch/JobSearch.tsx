"use client";

import { Stack } from "@mantine/core";
import { useState } from "react";
import { useJobSearch } from "@/lib/hooks/useJobSearch";
import { JobSearchFilters } from "./JobSearchFilters";
import { JobGrid } from "./JobGrid";

export function JobSearch() {
  const [skills, setSkills] = useState<string[]>([]);
  const [sourceDomains, setSourceDomains] = useState<string[]>([]);
  const [jobTitle, setJobTitle] = useState("");
  const [remote, setRemote] = useState<string>("all");
  const [seniorities, setSeniorities] = useState<string[]>([]);
  const [descriptionKeywords, setDescriptionKeywords] = useState<string[]>([]);
  const [employmentTypes, setEmploymentTypes] = useState<string[]>([]);
  const [countries, setCountries] = useState<string[]>([]);
  const [minSalary, setMinSalary] = useState<number | string>("");
  const [maxSalary, setMaxSalary] = useState<number | string>("");
  const [postedWithinDays, setPostedWithinDays] = useState<string>("30");
  const [companyNames, setCompanyNames] = useState<string[]>([]);
  const [hasSearched, setHasSearched] = useState(false);

  const { mutate, data, isPending, isError } = useJobSearch();

  function handleSearch() {
    const trimmedJobTitle = jobTitle.trim();
    const trimmedSkills = skills.map((skill) => skill.trim()).filter(Boolean);
    const trimmedSourceDomains = sourceDomains.map((domain) => domain.trim()).filter(Boolean);
    const trimmedDescriptionKeywords = descriptionKeywords.map((keyword) => keyword.trim()).filter(Boolean);
    const trimmedCompanyNames = companyNames.map((name) => name.trim()).filter(Boolean);

    setHasSearched(true);
    mutate({
      skills: trimmedSkills,
      sourceDomains: trimmedSourceDomains,
      page: 0,
      pageSize: 25,
      jobTitles: trimmedJobTitle ? [trimmedJobTitle] : null,
      remote: remote === "all" ? null : remote === "true",
      seniorities: seniorities.length > 0 ? seniorities : null,
      descriptionKeywords: trimmedDescriptionKeywords.length > 0 ? trimmedDescriptionKeywords : null,
      employmentTypes: employmentTypes.length > 0 ? employmentTypes : null,
      countries: countries.length > 0 ? countries : null,
      minSalaryUsd: minSalary !== "" ? Number(minSalary) : null,
      maxSalaryUsd: maxSalary !== "" ? Number(maxSalary) : null,
      postedWithinDays: Number(postedWithinDays),
      companyNames: trimmedCompanyNames.length > 0 ? trimmedCompanyNames : null,
    });
  }

  return (
    <Stack gap="lg">
      <JobSearchFilters
        skills={skills}
        sourceDomains={sourceDomains}
        jobTitle={jobTitle}
        remote={remote}
        seniorities={seniorities}
        descriptionKeywords={descriptionKeywords}
        employmentTypes={employmentTypes}
        countries={countries}
        minSalary={minSalary}
        maxSalary={maxSalary}
        postedWithinDays={postedWithinDays}
        companyNames={companyNames}
        isPending={isPending}
        onSkillsChange={setSkills}
        onSourceDomainsChange={setSourceDomains}
        onJobTitleChange={setJobTitle}
        onRemoteChange={setRemote}
        onSenioritiesChange={setSeniorities}
        onDescriptionKeywordsChange={setDescriptionKeywords}
        onEmploymentTypesChange={setEmploymentTypes}
        onCountriesChange={setCountries}
        onMinSalaryChange={setMinSalary}
        onMaxSalaryChange={setMaxSalary}
        onPostedWithinDaysChange={setPostedWithinDays}
        onCompanyNamesChange={setCompanyNames}
        onSearch={handleSearch}
      />
      <JobGrid
        jobs={data}
        isPending={isPending}
        isError={isError}
        hasSearched={hasSearched}
      />
    </Stack>
  );
}
