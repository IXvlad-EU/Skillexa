"use client";

import { useTranslations } from "next-intl";
import { Group, Text } from "@mantine/core";
import { LocaleSwitcher } from "@/components/LocaleSwitcher";
import classes from "./Navbar.module.scss";

export function Navbar() {
  const t = useTranslations("navbar");

  return (
    <Group h="100%" px="md" justify="space-between" className={classes.header}>
      <Text fw={700} size="lg">
        {t("appName")}
      </Text>
      <LocaleSwitcher />
    </Group>
  );
}
