"use client";

import { signOut, useSession } from "next-auth/react";
import { useLocale, useTranslations } from "next-intl";
import {
  ActionIcon,
  Avatar,
  Group,
  Menu,
  MenuDivider,
  MenuDropdown,
  MenuItem,
  MenuLabel,
  MenuTarget,
  Stack,
  Switch,
  Text,
  useComputedColorScheme,
  useMantineColorScheme,
} from "@mantine/core";
import { IconInfoCircle, IconLogout, IconMoon, IconUser } from "@tabler/icons-react";
import classes from "./UserMenu.module.scss";

export function UserMenu() {
  const t = useTranslations("navbar");
  const locale = useLocale();
  const { data: session, status } = useSession();
  const { setColorScheme } = useMantineColorScheme();
  const computedColorScheme = useComputedColorScheme("light", {
    getInitialValueInEffect: true,
  });

  if (status !== "authenticated") {
    return null;
  }

  const user = session.user;
  const displayName = user?.name ?? user?.email ?? "";
  const initials = getInitials(displayName);
  const isDark = computedColorScheme === "dark";

  function handleThemeChange() {
    setColorScheme(isDark ? "light" : "dark");
  }

  function handleSignOut() {
    void signOut({ callbackUrl: `/${locale}` });
  }

  return (
    <Menu shadow="md" width={260} position="bottom-end">
      <MenuTarget>
        <ActionIcon
          variant="subtle"
          size="lg"
          radius="xl"
          aria-label={t("profileMenu")}
          title={t("profileMenu")}
        >
          <Avatar src={user?.image} radius="xl" size={30}>
            {initials || <IconUser size={18} />}
          </Avatar>
        </ActionIcon>
      </MenuTarget>
      <MenuDropdown>
        <MenuLabel>
          <Group gap="sm" wrap="nowrap" className={classes.profileHeader}>
            <Avatar src={user?.image} radius="xl" size="md">
              {initials || <IconUser size={20} />}
            </Avatar>
            <Stack gap={0} className={classes.profileText}>
              <Text fw={600} size="sm" truncate>
                {displayName}
              </Text>
            </Stack>
          </Group>
        </MenuLabel>
        <MenuItem leftSection={<IconInfoCircle size={16} />}>
          {t("information")}
        </MenuItem>
        <MenuItem
          leftSection={<IconMoon size={16} />}
          rightSection={
            <Switch
              className={classes.themeSwitch}
              size="xs"
              checked={isDark}
              readOnly
              tabIndex={-1}
              aria-label={t("themeToggle")}
            />
          }
          closeMenuOnClick={false}
          onClick={handleThemeChange}
        >
          {t("themeToggle")}
        </MenuItem>
        <MenuDivider />
        <MenuItem
          color="red"
          leftSection={<IconLogout size={16} />}
          onClick={handleSignOut}
        >
          {t("logout")}
        </MenuItem>
      </MenuDropdown>
    </Menu>
  );
}

function getInitials(value: string): string {
  return value
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();
}
