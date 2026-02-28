"use client";

import { useLocale, useTranslations } from "next-intl";
import { usePathname, useRouter } from "@/i18n/navigation";
import { routing } from "@/i18n/routing";
import {
  ActionIcon,
  Menu,
  MenuDropdown,
  MenuItem,
  MenuTarget,
} from "@mantine/core";
import { IconLanguage } from "@tabler/icons-react";
import classes from "./LocaleSwitcher.module.scss";

const localeFlags: Record<string, string> = {
  en: "\uD83C\uDDEC\uD83C\uDDE7",
  ru: "\uD83C\uDDF7\uD83C\uDDFA",
  de: "\uD83C\uDDE9\uD83C\uDDEA",
};

export function LocaleSwitcher() {
  const t = useTranslations("navbar");
  const locale = useLocale();
  const router = useRouter();
  const pathname = usePathname();

  function handleLocaleChange(newLocale: string) {
    router.replace(pathname, { locale: newLocale });
  }

  return (
    <Menu shadow="md" width={180}>
      <MenuTarget>
        <ActionIcon
          variant="subtle"
          size="lg"
          aria-label={t("language")}
          title={t("language")}
        >
          <IconLanguage size={20} />
        </ActionIcon>
      </MenuTarget>
      <MenuDropdown>
        {routing.locales.map((loc) => (
          <MenuItem
            key={loc}
            onClick={() => handleLocaleChange(loc)}
            className={loc === locale ? classes.active : undefined}
            leftSection={<span>{localeFlags[loc]}</span>}
          >
            {t(`locale_${loc}`)}
          </MenuItem>
        ))}
      </MenuDropdown>
    </Menu>
  );
}
