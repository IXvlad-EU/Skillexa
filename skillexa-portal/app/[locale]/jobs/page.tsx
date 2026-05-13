import type { Metadata } from "next";
import { getTranslations } from "next-intl/server";
import { Container, Stack, Text, Title } from "@mantine/core";
import { JobSearch } from "@/components/JobSearch";

export async function generateMetadata(): Promise<Metadata> {
  const t = await getTranslations("jobSearch");
  return { title: t("title") };
}

export default async function JobsPage() {
  const t = await getTranslations("jobSearch");

  return (
    <Container size="xl" py="xl">
      <Stack gap="lg">
        <Title order={1}>{t("title")}</Title>
        <Text c="dimmed">{t("subtitle")}</Text>
        <JobSearch />
      </Stack>
    </Container>
  );
}
