"use client";

import { Alert, SimpleGrid, Skeleton, Stack, Text } from "@mantine/core";
import { IconSearch } from "@tabler/icons-react";
import { useTranslations } from "next-intl";
import type { components } from "@/lib/api-client/schema";
import { JobCard } from "./JobCard";

type SearchJobListingsResult = components["schemas"]["SearchJobListingsResult"];

type Props = {
  jobs: SearchJobListingsResult[] | undefined;
  isPending: boolean;
  isError: boolean;
  hasSearched: boolean;
};

export function JobGrid({ jobs, isPending, isError, hasSearched }: Props) {
  const t = useTranslations("jobSearch");

  if (isPending) {
    return (
      <SimpleGrid cols={{ base: 1, sm: 2, lg: 3 }} spacing="md">
        {Array.from({ length: 6 }).map((_, index) => (
          <Skeleton key={index} height={200} radius="md" />
        ))}
      </SimpleGrid>
    );
  }

  if (isError) {
    return (
      <Alert variant="light" color="red">
        {t("error")}
      </Alert>
    );
  }

  if (hasSearched && jobs && jobs.length === 0) {
    return (
      <Stack align="center" py="xl">
        <IconSearch size={48} color="var(--mantine-color-dimmed)" />
        <Text fw={500}>{t("noResults.title")}</Text>
        <Text c="dimmed" size="sm">
          {t("noResults.description")}
        </Text>
      </Stack>
    );
  }

  if (!jobs || jobs.length === 0) {
    return null;
  }

  return (
    <SimpleGrid cols={{ base: 1, sm: 2, lg: 3 }} spacing="md">
      {jobs.map((job) => (
        <JobCard key={job.id} job={job} />
      ))}
    </SimpleGrid>
  );
}
