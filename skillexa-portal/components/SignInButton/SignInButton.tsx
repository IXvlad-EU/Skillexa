"use client";

import { useSession, signIn } from "next-auth/react";
import { useTranslations } from "next-intl";
import Image from "next/image";
import { Button, Loader, Stack } from "@mantine/core";

import classes from "./SignInButton.module.scss";

const providers = [
  {
    id: "azure-ad",
    labelKey: "signInMicrosoft",
    iconSrc: "/auth/microsoft.svg",
  },
  {
    id: "google",
    labelKey: "signInGoogle",
    iconSrc: "/auth/google.svg",
  },
] as const;

export function SignInButton() {
  const t = useTranslations("home.hero");
  const { status } = useSession();

  if (status === "loading") {
    return <Loader size="sm" />;
  }

  if (status === "authenticated") {
    return null;
  }

  return (
    <Stack className={classes.stack} gap="sm" align="stretch">
      {providers.map((provider) => (
        <Button
          key={provider.id}
          className={classes.providerButton}
          classNames={{
            section: classes.iconSection,
            label: classes.label,
          }}
          variant="default"
          leftSection={
            <Image
              className={classes.icon}
              src={provider.iconSrc}
              alt=""
              width={20}
              height={20}
            />
          }
          onClick={() => signIn(provider.id)}
        >
          {t(provider.labelKey)}
        </Button>
      ))}
    </Stack>
  );
}
