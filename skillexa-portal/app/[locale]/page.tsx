import { getServerSession } from "next-auth";
import { getTranslations } from "next-intl/server";
import type { ComponentType } from "react";
import {
  Box,
  Container,
  Divider,
  SimpleGrid,
  Stack,
  Text,
  ThemeIcon,
  Title,
} from "@mantine/core";
import { IconDownload, IconFileText, IconSearch } from "@tabler/icons-react";
import { authOptions } from "@/auth";
import { SignInButton } from "@/components/SignInButton";
import { redirect } from "@/i18n/navigation";

import classes from "./page.module.scss";

type Props = {
  params: Promise<{ locale: string }>;
};

type FeatureItem = {
  icon: ComponentType<{ size?: number | string }>;
  title: string;
  description: string;
};

type StepItem = {
  number: string;
  title: string;
  description: string;
};

export default async function HomePage({ params }: Props) {
  const { locale } = await params;
  const session = await getServerSession(authOptions);

  if (session) {
    redirect({ href: "/jobs", locale });
  }

  const t = await getTranslations("home");

  const features: FeatureItem[] = [
    {
      icon: IconSearch,
      title: t("features.search.title"),
      description: t("features.search.description"),
    },
    {
      icon: IconFileText,
      title: t("features.pdf.title"),
      description: t("features.pdf.description"),
    },
    {
      icon: IconDownload,
      title: t("features.download.title"),
      description: t("features.download.description"),
    },
  ];

  const steps: StepItem[] = [
    {
      number: "01",
      title: t("steps.signIn.title"),
      description: t("steps.signIn.description"),
    },
    {
      number: "02",
      title: t("steps.browse.title"),
      description: t("steps.browse.description"),
    },
    {
      number: "03",
      title: t("steps.generate.title"),
      description: t("steps.generate.description"),
    },
    {
      number: "04",
      title: t("steps.download.title"),
      description: t("steps.download.description"),
    },
  ];

  return (
    <>
      {/* ── Hero ──────────────────────────────────────────────── */}
      <Box className={classes.hero}>
        <Container size="sm" py="xl" w="100%">
          <Stack align="center" gap="xl">
            <Title order={1} ta="center" size="h1" lh={1.2}>
              {t("hero.title")}
            </Title>
            <Text size="xl" c="dimmed" ta="center" maw={560}>
              {t("hero.tagline")}
            </Text>
            <SignInButton />
          </Stack>
        </Container>
      </Box>

      <Divider />

      {/* ── Features ──────────────────────────────────────────── */}
      <Box className={classes.sectionAlt}>
        <Container size="lg" py={80}>
          <Title order={2} ta="center" mb={48}>
            {t("features.sectionTitle")}
          </Title>
          <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="xl">
            {features.map((feature) => (
              <Stack key={feature.title} align="center" ta="center" gap="sm">
                <ThemeIcon size={56} radius="md" variant="light">
                  <feature.icon size={28} />
                </ThemeIcon>
                <Text fw={600} size="lg">
                  {feature.title}
                </Text>
                <Text size="sm" c="dimmed" maw={280}>
                  {feature.description}
                </Text>
              </Stack>
            ))}
          </SimpleGrid>
        </Container>
      </Box>

      <Divider />

      {/* ── How it works ──────────────────────────────────────── */}
      <Container size="lg" py={80}>
        <Title order={2} ta="center" mb={48}>
          {t("steps.sectionTitle")}
        </Title>
        <SimpleGrid cols={{ base: 1, sm: 2, md: 4 }} spacing="xl">
          {steps.map((step) => (
            <Stack key={step.number} align="center" ta="center" gap="sm">
              <ThemeIcon size={56} radius="xl">
                <Text fw={700} size="lg" c="white">
                  {step.number}
                </Text>
              </ThemeIcon>
              <Text fw={600} size="lg">
                {step.title}
              </Text>
              <Text size="sm" c="dimmed" maw={200}>
                {step.description}
              </Text>
            </Stack>
          ))}
        </SimpleGrid>
      </Container>
    </>
  );
}
