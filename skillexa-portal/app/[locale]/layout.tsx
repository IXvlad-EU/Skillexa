import type { Metadata } from "next";
import { NextIntlClientProvider, hasLocale } from "next-intl";
import { notFound } from "next/navigation";
import {
  AppShell,
  AppShellHeader,
  AppShellMain,
  ColorSchemeScript,
  MantineProvider,
  mantineHtmlProps,
} from "@mantine/core";
import { routing } from "@/i18n/routing";
import { Navbar } from "@/components/Navbar";
import { Providers } from "@/app/providers";

import "@mantine/core/styles.css";
import "../globals.scss";

type Props = {
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
};

export const metadata: Metadata = {
  title: "Skillexa",
  description: "Skillexa Portal",
};

export default async function LocaleLayout({ children, params }: Props) {
  const { locale } = await params;

  if (!hasLocale(routing.locales, locale)) {
    notFound();
  }

  return (
    <html lang={locale} {...mantineHtmlProps}>
      <head>
        <ColorSchemeScript defaultColorScheme="auto" />
      </head>
      <body>
        <MantineProvider defaultColorScheme="auto">
          <NextIntlClientProvider>
            <Providers>
              <AppShell header={{ height: 60 }}>
                <AppShellHeader>
                  <Navbar />
                </AppShellHeader>
                <AppShellMain>{children}</AppShellMain>
              </AppShell>
            </Providers>
          </NextIntlClientProvider>
        </MantineProvider>
      </body>
    </html>
  );
}
