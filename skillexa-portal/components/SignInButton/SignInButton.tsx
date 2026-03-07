"use client";

import { useSession, signIn } from "next-auth/react";
import { Button, Loader } from "@mantine/core";
import { IconBrandWindows } from "@tabler/icons-react";

type SignInButtonProps = {
  label: string;
};

export function SignInButton({ label }: SignInButtonProps) {
  const { status } = useSession();

  if (status === "loading") {
    return <Loader size="sm" />;
  }

  if (status === "authenticated") {
    return null;
  }

  return (
    <Button
      size="lg"
      leftSection={<IconBrandWindows size={20} />}
      onClick={() => signIn("azure-ad")}
    >
      {label}
    </Button>
  );
}
