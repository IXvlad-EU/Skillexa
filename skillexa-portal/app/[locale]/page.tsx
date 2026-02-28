import { useTranslations } from "next-intl";
import { Center, Text } from "@mantine/core";

export default function HomePage() {
  const t = useTranslations("common");

  return (
    <Center mih="calc(100vh - 60px)">
      <Text size="xl">{t("helloWorld")}</Text>
    </Center>
  );
}
