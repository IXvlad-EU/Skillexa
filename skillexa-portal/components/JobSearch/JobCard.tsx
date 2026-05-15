"use client";

import { ActionIcon, Anchor, Avatar, Badge, Button, Card, Group, Stack, Text } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { notifications } from "@mantine/notifications";
import { useTranslations } from "next-intl";
import { IconChevronDown } from "@tabler/icons-react";
import ReactMarkdown from "react-markdown";
import type { components } from "@/lib/api-client/schema";
import { useCreateDocument } from "@/lib/hooks/useCreateDocument";
import classes from "./JobCard.module.scss";

type SearchJobListingsResult = components["schemas"]["SearchJobListingsResult"];

type Props = {
  job: SearchJobListingsResult;
};

export function JobCard({ job }: Props) {
  const t = useTranslations("jobSearch");
  const { mutate, isPending } = useCreateDocument();
  const [descriptionOpened, { toggle: toggleDescription }] = useDisclosure(false);

  function handleGenerateCv() {
    mutate(
      {
        payloadJson: JSON.stringify(job),
        templateKey: "default",
        templateVersion: 1,
      },
      {
        onSuccess: () => {
          notifications.show({
            color: "green",
            title: t("card.generateCv_successTitle"),
            message: t("generateCv_success"),
          });
        },
        onError: () => {
          notifications.show({
            color: "red",
            title: t("card.generateCv_errorTitle"),
            message: t("generateCv_error"),
          });
        },
      },
    );
  }

  const initials = job.companyName
    .split(" ")
    .slice(0, 2)
    .map((word) => word[0])
    .join("")
    .toUpperCase();

  return (
    <Card withBorder radius="md" className={classes.card}>
      <Stack gap="sm" style={{ flex: 1 }}>
        <Group wrap="nowrap" align="flex-start">
          <Avatar src={job.companyLogoUrl} radius="sm" size="md">
            {initials}
          </Avatar>
          <Stack gap={2} style={{ flex: 1, minWidth: 0 }}>
            <Text fw={600} lineClamp={2}>
              {job.title}
            </Text>
            <Text size="sm" c="dimmed">
              {job.companyName}
            </Text>
          </Stack>
        </Group>

        {(job.location || job.salaryString) && (
          <Group gap="xs">
            {job.location && (
              <Text size="sm" c="dimmed">
                {job.location}
              </Text>
            )}
            {job.salaryString && (
              <Text size="sm" c="dimmed">
                · {job.salaryString}
              </Text>
            )}
          </Group>
        )}

        {job.description && (
          <Stack gap={4}>
            <div className={`${classes.description} ${!descriptionOpened ? classes.clamped : ""}`}>
              <ReactMarkdown>{job.description}</ReactMarkdown>
            </div>
            <ActionIcon
              variant="subtle"
              color="gray"
              size="xs"
              onClick={toggleDescription}
              className={`${classes.chevron} ${descriptionOpened ? classes.chevronOpen : ""}`}
            >
              <IconChevronDown size={14} />
            </ActionIcon>
          </Stack>
        )}

        <Group gap="xs" wrap="wrap">
          {job.remote && (
            <Badge variant="light" color="blue" size="sm">
              {t("card.remote")}
            </Badge>
          )}
          {job.technologySlugs.slice(0, 5).map((slug) => (
            <Badge key={slug} variant="outline" size="sm">
              {slug}
            </Badge>
          ))}
        </Group>

        <Text size="xs" c="dimmed">
          {t("card.postedOn", { date: job.datePosted })}
        </Text>

        <Group justify="space-between" mt="auto">
          <Anchor
            href={job.url}
            target="_blank"
            rel="noopener noreferrer"
            size="sm"
          >
            {t("card.viewJob")}
          </Anchor>
          <Button size="xs" loading={isPending} onClick={handleGenerateCv}>
            {t("generateCv")}
          </Button>
        </Group>
      </Stack>
    </Card>
  );
}
