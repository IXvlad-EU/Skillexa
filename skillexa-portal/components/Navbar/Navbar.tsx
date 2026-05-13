"use client";

import { useTranslations } from "next-intl";
import { Anchor, Group, Text } from "@mantine/core";
import { Link } from "@/i18n/navigation";
import { LocaleSwitcher } from "@/components/LocaleSwitcher";
import classes from "./Navbar.module.scss";

export function Navbar() {
  const t = useTranslations("navbar");

  return (
    <Group h="100%" px="md" justify="space-between" className={classes.header}>
      <Text fw={700} size="lg">
        {t("appName")}
      </Text>
      <Group gap="sm">
        <Anchor component={Link} href="/jobs" size="sm">
          {t("jobs")}
        </Anchor>
        <LocaleSwitcher />
      </Group>
    </Group>
  );
}
