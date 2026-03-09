---
description: "Prettier code formatting configuration and ESLint integration"
applyTo: "skillexa-portal/**"
---

# Prettier — Instructions

## Overview

**Prettier** is the sole code formatter for the portal codebase. It handles all formatting concerns — indentation, line breaks, quotes, semicolons, trailing commas — so developers never argue about style or manually format code.

## Golden Rule

**Never disable or override Prettier formatting.** If generated code looks odd, it's probably correct. Do not add `// prettier-ignore` comments unless there is a compelling readability reason — and add a comment explaining why.

## Configuration

### `.prettierrc.json` (portal root)

```jsonc
{
  "semi": true,
  "singleQuote": false, // double quotes everywhere
  "trailingComma": "all", // trailing commas in all valid positions
  "tabWidth": 2,
  "printWidth": 80,
  "arrowParens": "always", // (x) => ... not x => ...
  "endOfLine": "lf", // Unix line endings
}
```

Do **not** add options beyond those listed above unless the team explicitly agrees. Fewer options = fewer debates.

### `.prettierignore` (portal root)

Build artifacts, dependencies, lock files, and **Kiota-generated API client** (`lib/api-client/`) are excluded from formatting. Never format auto-generated code.

## Format-on-Save (VS Code)

Format-on-save is configured in `.vscode/settings.json` at the workspace root:

- **`editor.formatOnSave: true`** — every save triggers Prettier automatically.
- **`editor.defaultFormatter: "esbenp.prettier-vscode"`** — Prettier is the default formatter for TypeScript, JavaScript, JSON, SCSS, CSS, HTML, Markdown, and YAML.
- The **Prettier - Code formatter** VS Code extension (`esbenp.prettier-vscode`) is listed in `.vscode/extensions.json` — install it when prompted.

If format-on-save is not working, check:

1. The Prettier extension is installed and enabled.
2. No other formatter extension is overriding the default (e.g., Beautify).
3. The file type is covered in `.vscode/settings.json`.

## ESLint Integration

**`eslint-config-prettier`** is added to the ESLint flat config to **disable all ESLint rules that conflict with Prettier**. This means:

- ESLint handles **code quality** (unused variables, hook rules, Next.js best practices).
- Prettier handles **code style** (formatting).
- There is **zero overlap** between the two.

```js
// eslint.config.mjs
import prettierConfig from "eslint-config-prettier";

const eslintConfig = defineConfig([
  ...nextVitals,
  ...nextTs,
  prettierConfig,   // ← must come AFTER other configs to override their style rules
  globalIgnores([...]),
]);
```

Do **not** use `eslint-plugin-prettier` (which runs Prettier as an ESLint rule) — it is slower and produces noisy lint output. The recommended approach is `eslint-config-prettier` only.

## NPM Scripts

| Script              | Command              | Purpose                                 |
| ------------------- | -------------------- | --------------------------------------- |
| `pnpm format`       | `prettier --write .` | Format all files in-place               |
| `pnpm format:check` | `prettier --check .` | Check formatting without modifying (CI) |
| `pnpm lint`         | `eslint`             | Lint for code quality issues only       |

### CI Usage

Run `pnpm format:check` in CI pipelines to fail the build if unformatted code is committed. This ensures the entire team stays consistent.

## `.editorconfig` (workspace root)

A workspace-level `.editorconfig` provides baseline settings (UTF-8, LF, indent size) for editors that do not use Prettier. It is a safety net — Prettier overrides it for all file types it supports.

## Rules for Generated Code

- **Kiota API client** (`lib/api-client/`): excluded via `.prettierignore`. Never manually format it.
- If new code generators are added, add their output directories to `.prettierignore`.

## Adding New File Types

If a new file type needs Prettier formatting:

1. Check [Prettier's language support](https://prettier.io/docs/en/index.html) — it must have a built-in parser or a plugin.
2. Add the language identifier to `.vscode/settings.json` with `"editor.defaultFormatter": "esbenp.prettier-vscode"`.
3. If a plugin is needed, install it (`pnpm add -D prettier-plugin-*`) and add it to `.prettierrc.json` under `"plugins"`.
