import type { Metadata } from "next";
import { getServerSession } from "next-auth";
import { getTranslations } from "next-intl/server";
import { Container, Stack, Text, Title } from "@mantine/core";
import { authOptions } from "@/auth";
import { JobSearch } from "@/components/JobSearch";
import { redirect } from "@/i18n/navigation";

type Props = {
  params: Promise<{ locale: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "jobSearch" });

  return { title: t("title") };
}

export default async function JobsPage({ params }: Props) {
  const { locale } = await params;
  const session = await getServerSession(authOptions);

  if (!session) {
    redirect({ href: "/", locale });
  }

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
