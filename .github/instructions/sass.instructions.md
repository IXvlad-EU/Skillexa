---
description: "SCSS styling conventions and Mantine CSS module integration"
applyTo: "skillexa-portal/**"
---

# SASS — Instructions

## Overview

Skillexa-Portal uses **SASS (SCSS syntax)** instead of plain CSS. All stylesheets use the `.scss` extension. This applies to global styles, Mantine CSS modules, and any shared style utilities.

> Sass supports two syntaxes: `.scss` (superset of CSS) and `.sass` (indented syntax). This project uses **`.scss` exclusively** because it is a superset of CSS and requires no new indentation rules.

## Setup

### Dependency

Next.js has built-in Sass support — install the `sass` package and Next.js handles the rest (no Webpack/Turbopack config required):

```bash
pnpm add -D sass
```

For faster compilation, `sass-embedded` can be used as a drop-in replacement. To switch, install the package and set the `implementation` option:

```bash
pnpm add -D sass-embedded
```

```ts
// next.config.ts
import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  sassOptions: {
    implementation: "sass-embedded",
  },
};

export default nextConfig;
```

### `sassOptions` in `next.config.ts`

Use the `sassOptions` key for any Sass compiler configuration. Common options:

```ts
// next.config.ts
import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  sassOptions: {
    // Inject shared partials into every SCSS file automatically:
    additionalData: `@use 'styles/variables' as vars;`,
    // Or switch implementation:
    // implementation: 'sass-embedded',
  },
};

export default nextConfig;
```

- **`additionalData`** — prepends SCSS code to every file before compilation. Useful for auto-importing shared variables or mixins so you don't need `@use` in every module.
- **`implementation`** — `'sass'` (default) or `'sass-embedded'`.
- All standard [Sass JS API options](https://sass-lang.com/documentation/js-api/interfaces/options/) are supported.

### PostCSS

The PostCSS config (`postcss.config.cjs`) continues to use `postcss-preset-mantine` and `postcss-simple-vars`. PostCSS runs **after** SASS compilation, so Mantine-specific PostCSS features (light-dark functions, breakpoint variables) work seamlessly with SCSS source files.

## File Naming Conventions

| File Type            | Extension                     | Example                                         |
| -------------------- | ----------------------------- | ----------------------------------------------- |
| Global styles        | `.scss`                       | `app/globals.scss`                              |
| Mantine CSS module   | `.module.scss`                | `components/JobCard/JobCard.module.scss`        |
| Shared SCSS partials | `_*.scss` (underscore prefix) | `styles/_variables.scss`, `styles/_mixins.scss` |

## Global Styles

- The root layout imports `app/globals.scss` (not `.css`).
- Mantine package styles (`@mantine/core/styles.css`, etc.) are still imported as plain CSS — do **not** rename or wrap them.

```tsx
// app/layout.tsx
import "@mantine/core/styles.css";
import "./globals.scss";
```

## Mantine CSS Modules with SCSS

Use `.module.scss` for component-scoped Mantine styles. Access Mantine theme tokens via CSS custom properties (`var(--mantine-*)`), not via SCSS variables:

```scss
// components/JobCard/JobCard.module.scss
.card {
  padding: var(--mantine-spacing-md);
  border-radius: var(--mantine-radius-md);

  &:hover {
    background-color: var(--mantine-color-gray-0);
  }
}
```

Import in the component:

```tsx
import classes from "./JobCard.module.scss";

<Card className={classes.card}>...</Card>;
```

## Exporting SCSS Variables to JavaScript

Next.js supports the Sass `:export` block in CSS Module files, making SCSS values available in JS/TS:

```scss
// app/variables.module.scss
$primary-color: #64ff00;

:export {
  primaryColor: $primary-color;
}
```

```tsx
import variables from "./variables.module.scss";

export default function Page() {
  return <h1 style={{ color: variables.primaryColor }}>Hello</h1>;
}
```

Use this sparingly — prefer Mantine theme tokens for theming. Reserve `:export` for cases where a SCSS value must be read in JS logic (e.g., chart colors, canvas drawing).

## Shared Partials & Imports

Place reusable SCSS partials in `styles/`:

```
styles/
  _variables.scss   # app-level SCSS variables (non-Mantine)
  _mixins.scss      # reusable mixins
  _typography.scss  # font-face declarations, type scale helpers
```

Import partials with `@use`:

```scss
// components/Header/Header.module.scss
@use "../../styles/variables" as vars;
@use "../../styles/mixins" as mix;

.header {
  height: vars.$header-height;
  @include mix.flex-center;
}
```

> **Tip:** If most files import the same partials, use `sassOptions.additionalData` in `next.config.ts` to auto-inject them instead of repeating `@use` in every file.

### Rules for `@use` / `@forward`

- **Always use `@use`** — never `@import` (deprecated in Dart Sass).
- Use **namespaced access** (`vars.$name`, `mix.mixin-name`) to keep origin clear.
- Use `@forward` only in barrel files (`styles/_index.scss`) to re-export partials.

## SCSS Variables vs Mantine CSS Custom Properties

| Use Case                                                                         | Mechanism                                   |
| -------------------------------------------------------------------------------- | ------------------------------------------- |
| Mantine theme tokens (colors, spacing, radii, font sizes)                        | `var(--mantine-*)` CSS custom properties    |
| App-specific constants not in Mantine theme (e.g., sidebar width, header height) | SCSS `$variables` in `_variables.scss`      |
| Responsive breakpoints (Mantine-aware)                                           | PostCSS `$mantine-breakpoint-*` simple-vars |

Do **not** duplicate Mantine tokens as SCSS variables. Use `var(--mantine-*)` directly.

## Nesting

SCSS nesting is allowed and encouraged for:

- Pseudo-classes and pseudo-elements (`&:hover`, `&::before`).
- BEM-like children (`&__title`, `&--active`) when not using CSS modules.
- Media queries via Mantine PostCSS `@mixin` or standard `@media`.

Keep nesting **≤ 3 levels deep** to avoid specificity bloat.

## Mixins & Functions

Define reusable patterns as mixins:

```scss
// styles/_mixins.scss
@mixin flex-center {
  display: flex;
  align-items: center;
  justify-content: center;
}

@mixin truncate($lines: 1) {
  @if $lines == 1 {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  } @else {
    display: -webkit-box;
    -webkit-line-clamp: $lines;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
}
```

## Forbidden Patterns

- **No Tailwind CSS** — Mantine + SCSS replaces it entirely.
- **No `.sass` indented syntax** — use `.scss` only for consistency.
- **No `@import`** — use `@use` / `@forward` exclusively (Dart Sass deprecation).
- **No inline `style={{}}` objects** for anything beyond truly dynamic values (e.g., computed positions). Use Mantine style props or SCSS modules instead.
- **No global class names** in module files — CSS modules auto-scope; keep class names short and descriptive (`.card`, `.title`, not `.job-card-wrapper-title`).

## VS Code Setup

- Install [SCSS IntelliSense](https://marketplace.visualstudio.com/items?itemName=mrmlnc.vscode-scss) for autocompletion of SCSS variables, mixins, and partials.
- The PostCSS and CSS Variable extensions from the portal instructions remain relevant for Mantine token autocompletion.
