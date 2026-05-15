"use client";

import {
  Button,
  Collapse,
  MultiSelect,
  NumberInput,
  Select,
  SegmentedControl,
  TagsInput,
  Text,
  TextInput,
} from "@mantine/core";
import { useTranslations } from "next-intl";
import { useState } from "react";
import classes from "./JobSearchFilters.module.scss";

const SOURCE_DOMAIN_OPTIONS = [
  "indeed.com",
  "linkedin.com",
  "myworkdayjobs.com",
  "seek.com.au",
  "icims.com",
  "infojobs.net",
  "adp.com",
  "smartrecruiters.com",
  "appcast.io",
  "naukri.com",
  "oraclecloud.com",
  "startup.jobs",
  "join.com",
  "ultipro.com",
  "workable.com",
  "gupy.io",
  "grnh.se",
  "recruitics.com",
  "dayforcehcm.com",
  "paycomonline.net",
  "bamboohr.com",
  "paylocity.com",
];

type Props = {
  skills: string[];
  sourceDomains: string[];
  jobTitle: string;
  remote: string;
  seniorities: string[];
  descriptionKeywords: string[];
  employmentTypes: string[];
  countries: string[];
  minSalary: number | string;
  maxSalary: number | string;
  postedWithinDays: string;
  companyNames: string[];
  isPending: boolean;
  onSkillsChange: (value: string[]) => void;
  onSourceDomainsChange: (value: string[]) => void;
  onJobTitleChange: (value: string) => void;
  onRemoteChange: (value: string) => void;
  onSenioritiesChange: (value: string[]) => void;
  onDescriptionKeywordsChange: (value: string[]) => void;
  onEmploymentTypesChange: (value: string[]) => void;
  onCountriesChange: (value: string[]) => void;
  onMinSalaryChange: (value: number | string) => void;
  onMaxSalaryChange: (value: number | string) => void;
  onPostedWithinDaysChange: (value: string) => void;
  onCompanyNamesChange: (value: string[]) => void;
  onClearFilters: () => void;
  onSearch: () => void;
};

export function JobSearchFilters({
  skills,
  sourceDomains,
  jobTitle,
  remote,
  seniorities,
  descriptionKeywords,
  employmentTypes,
  countries,
  minSalary,
  maxSalary,
  postedWithinDays,
  companyNames,
  isPending,
  onSkillsChange,
  onSourceDomainsChange,
  onJobTitleChange,
  onRemoteChange,
  onSenioritiesChange,
  onDescriptionKeywordsChange,
  onEmploymentTypesChange,
  onCountriesChange,
  onMinSalaryChange,
  onMaxSalaryChange,
  onPostedWithinDaysChange,
  onCompanyNamesChange,
  onClearFilters,
  onSearch,
}: Props) {
  const t = useTranslations("jobSearch");
  const [showMore, setShowMore] = useState(false);

  const SENIORITY_OPTIONS = [
    { value: "junior", label: t("filters.seniority_options.junior") },
    { value: "mid_level", label: t("filters.seniority_options.mid_level") },
    { value: "senior", label: t("filters.seniority_options.senior") },
    { value: "staff", label: t("filters.seniority_options.staff") },
    { value: "c_level", label: t("filters.seniority_options.c_level") },
  ];

  const EMPLOYMENT_TYPE_OPTIONS = [
    { value: "full_time", label: t("filters.employment_type_options.full_time") },
    { value: "part_time", label: t("filters.employment_type_options.part_time") },
    { value: "contract", label: t("filters.employment_type_options.contract") },
    { value: "temporary", label: t("filters.employment_type_options.temporary") },
    { value: "internship", label: t("filters.employment_type_options.internship") },
  ];

  const POSTED_WITHIN_OPTIONS = [
    { value: "7", label: t("filters.posted_within_options.days_7") },
    { value: "14", label: t("filters.posted_within_options.days_14") },
    { value: "30", label: t("filters.posted_within_options.days_30") },
    { value: "60", label: t("filters.posted_within_options.days_60") },
    { value: "90", label: t("filters.posted_within_options.days_90") },
  ];

  return (
    <div className={classes.root}>
      {/* ── Primary row ─────────────────────────── */}
      <div className={classes.primaryRow}>
        <TextInput
          label={t("filters.jobTitle")}
          placeholder={t("filters.jobTitlePlaceholder")}
          value={jobTitle}
          onChange={(event) => onJobTitleChange(event.currentTarget.value)}
        />
        <TagsInput
          label={t("filters.skills")}
          placeholder={t("filters.skillsPlaceholder")}
          value={skills}
          onChange={onSkillsChange}
        />
        <div className={classes.field}>
          <Text size="sm" fw={500} mb={4}>
            {t("filters.workType")}
          </Text>
          <SegmentedControl
            data={[
              { value: "all", label: t("filters.workTypeAll") },
              { value: "true", label: t("filters.workTypeRemote") },
              { value: "false", label: t("filters.workTypeOnSite") },
            ]}
            value={remote}
            onChange={onRemoteChange}
            fullWidth
          />
        </div>
        <MultiSelect
          label={t("filters.seniority")}
          placeholder={t("filters.seniorityPlaceholder")}
          data={SENIORITY_OPTIONS}
          value={seniorities}
          onChange={onSenioritiesChange}
        />
        <div className={classes.actions}>
          <Button
            variant="subtle"
            size="sm"
            onClick={() => setShowMore((prev) => !prev)}
          >
            {showMore ? t("filters.fewerFilters") : t("filters.moreFilters")}
          </Button>
          <Button variant="subtle" size="sm" onClick={onClearFilters}>
            {t("filters.clearFilters")}
          </Button>
          <Button loading={isPending} onClick={onSearch}>
            {t("filters.search")}
          </Button>
        </div>
      </div>

      {/* ── Advanced panel ──────────────────────── */}
      <Collapse in={showMore}>
        <div className={classes.advancedGrid}>
          <TagsInput
            label={t("filters.descriptionKeywords")}
            placeholder={t("filters.descriptionKeywordsPlaceholder")}
            value={descriptionKeywords}
            onChange={onDescriptionKeywordsChange}
          />
          <MultiSelect
            label={t("filters.employmentType")}
            placeholder={t("filters.employmentTypePlaceholder")}
            data={EMPLOYMENT_TYPE_OPTIONS}
            value={employmentTypes}
            onChange={onEmploymentTypesChange}
          />
          <TagsInput
            label={t("filters.country")}
            placeholder={t("filters.countryPlaceholder")}
            value={countries}
            onChange={onCountriesChange}
          />
          <TagsInput
            label={t("filters.companyName")}
            placeholder={t("filters.companyNamePlaceholder")}
            value={companyNames}
            onChange={onCompanyNamesChange}
          />
          <NumberInput
            label={t("filters.minSalary")}
            placeholder={t("filters.minSalaryPlaceholder")}
            prefix="$"
            value={minSalary}
            onChange={onMinSalaryChange}
            min={0}
          />
          <NumberInput
            label={t("filters.maxSalary")}
            placeholder={t("filters.maxSalaryPlaceholder")}
            prefix="$"
            value={maxSalary}
            onChange={onMaxSalaryChange}
            min={0}
          />
          <Select
            label={t("filters.postedWithin")}
            data={POSTED_WITHIN_OPTIONS}
            value={postedWithinDays}
            onChange={(value) => onPostedWithinDaysChange(value ?? "30")}
            allowDeselect={false}
          />
          <MultiSelect
            label={t("filters.sources")}
            placeholder={t("filters.sourcesPlaceholder")}
            data={SOURCE_DOMAIN_OPTIONS}
            value={sourceDomains}
            onChange={onSourceDomainsChange}
          />
        </div>
      </Collapse>
    </div>
  );
}

