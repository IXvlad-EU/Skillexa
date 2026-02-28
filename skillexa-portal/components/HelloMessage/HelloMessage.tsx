"use client";

import { Center, Loader, Text } from "@mantine/core";
import { useTranslations } from "next-intl";
import { useHello } from "@/lib/hooks/useHello";

export function HelloMessage() {
  const t = useTranslations("common");
  const { data, isLoading, error } = useHello();

  if (isLoading) {
    return (
      <Center mih="calc(100vh - 60px)">
        <Loader />
      </Center>
    );
  }

  if (error) {
    return (
      <Center mih="calc(100vh - 60px)">
        <Text size="xl" c="red">
          {t("failedToLoad")}
        </Text>
      </Center>
    );
  }

  return (
    <Center mih="calc(100vh - 60px)">
      <Text size="xl">{data}</Text>
      <Text size="xl">{t("helloWorld")}</Text>
    </Center>
  );
}
