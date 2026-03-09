---
description: "Mantine 7 UI library setup, components, styling, and Next.js App Router integration"
applyTo: "skillexa-portal/**"
---

# Mantine UI — Instructions

Mantine is the **primary UI library** for all Skillexa Portal components. Do **not** use Tailwind CSS — Mantine replaces it entirely.

---

## Setup

- Wrap the application in `MantineProvider` inside `app/layout.tsx`.
- Import `@mantine/core/styles.css` (and any other package styles like `@mantine/notifications/styles.css`) in the root layout **before** any other CSS.
- Add `ColorSchemeScript` inside `<head>` to prevent color-scheme flash.
- Spread `mantineHtmlProps` on the `<html>` element to avoid hydration warnings:

```tsx
import {
  ColorSchemeScript,
  MantineProvider,
  mantineHtmlProps,
} from "@mantine/core";

<html lang="en" {...mantineHtmlProps}>
  <head>
    <ColorSchemeScript defaultColorScheme="auto" />
  </head>
  <body>
    <MantineProvider>{children}</MantineProvider>
  </body>
</html>;
```

- Create `postcss.config.cjs` at the project root with `postcss-preset-mantine` and `postcss-simple-vars` plugins:

```js
module.exports = {
  plugins: {
    "postcss-preset-mantine": {},
    "postcss-simple-vars": {
      variables: {
        "mantine-breakpoint-xs": "36em",
        "mantine-breakpoint-sm": "48em",
        "mantine-breakpoint-md": "62em",
        "mantine-breakpoint-lg": "75em",
        "mantine-breakpoint-xl": "88em",
      },
    },
  },
};
```

---

## Component Usage

- **Always prefer Mantine components** over raw HTML elements or custom implementations. Use `Button`, `TextInput`, `Select`, `Modal`, `AppShell`, `Stack`, `Group`, `Grid`, `Card`, `Text`, `Title`, etc.
- For layout, use Mantine's `AppShell` for the authenticated shell (header, navbar, main content).
- Use `@mantine/form` for all form state management (validation, dirty tracking, submit handling).
- Use `@mantine/notifications` for toast/notification feedback (success, error, info messages).
- Use `@mantine/hooks` for common UI utilities (`useDisclosure`, `useMediaQuery`, `useClipboard`, `useDebouncedValue`, etc.).

---

## Icons

- Use **`@tabler/icons-react`** as the default icon library — it is Mantine's recommended icon set and is already installed.
- Always prefer Tabler icons before reaching for any other icon package. They integrate seamlessly with Mantine components (`ActionIcon`, `Button` `leftSection`/`rightSection`, `ThemeIcon`, etc.).
- Import icons individually for tree shaking: `import { IconLanguage } from '@tabler/icons-react';`
- Use a consistent icon `size` prop (e.g., `16` for inline, `20` for buttons, `24` for headers) to maintain visual harmony.
- Ensure `@tabler/icons-react` is listed in `optimizePackageImports` in `next.config.ts` for efficient bundling.
- Do **not** add alternative icon libraries (Lucide, Heroicons, Font Awesome, Material Icons, etc.) unless Tabler icons genuinely lack a required glyph — and document the reason in a code comment if so.

---

## Styling Rules

- Use **Mantine CSS modules** (`.module.css` files) for component-specific styles. Access Mantine theme variables via `var(--mantine-*)` CSS custom properties.
- Use Mantine **style props** (`p`, `m`, `fw`, `fz`, `c`, `bg`, etc.) for quick inline-style adjustments.
- For theming, extend the Mantine default theme in a central `theme.ts` file and pass it to `MantineProvider`.
- Override component default props via `theme.components` when you need consistent styling across the app.

---

## Color Scheme

- Support **light and dark** color schemes via Mantine's built-in `colorScheme` management.
- Store the user's preference in a cookie (`ColorSchemeScript` + server-side detection).

---

## SSR / Next.js App Router Integration

- All `@mantine/*` package entry points already include `'use client';` — you do **not** need to add it to files that import Mantine components.
- Mantine components render on both server and client (they cannot be true Server Components due to `useContext` usage for theming/Styles API).
- **Compound components** (e.g., `Popover.Target`, `Popover.Dropdown`) cannot be used in Server Components. Use the flat import syntax instead: `PopoverTarget`, `PopoverDropdown`.
- Enable tree shaking in `next.config.ts`:

```ts
export default {
  experimental: {
    optimizePackageImports: ["@mantine/core", "@mantine/hooks"],
  },
};
```

- Use Next.js `Link` with Mantine polymorphic components via the `component` prop: `<Button component={Link} href="/path">`.

---

## VS Code Setup

- Install [PostCSS Intellisense and Highlighting](https://marketplace.visualstudio.com/items?itemName=vunguyentuan.vscode-postcss) to enable syntax highlighting for Mantine postcss syntax (e.g., `$variable` references).
- Install [CSS Variable Autocomplete](https://marketplace.visualstudio.com/items?itemName=vunguyentuan.vscode-css-variables) and add to `.vscode/settings.json`:

```json
{
  "cssVariables.lookupFiles": [
    "**/*.css",
    "node_modules/@mantine/core/styles.css"
  ]
}
```
