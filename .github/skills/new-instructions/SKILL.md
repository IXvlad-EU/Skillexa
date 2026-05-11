---
name: new-instructions
description: 'Generate, review, or fix .instructions.md files in .github/instructions/. USE FOR: creating a new [name].instructions.md from scratch; adding frontmatter (description, applyTo); structuring content sections (overview, patterns, examples, anti-patterns); updating stale instructions. DO NOT USE FOR: creating SKILL.md files (use skill-creator mode); creating prompt files. Keywords: instructions file, copilot instructions, applyTo, frontmatter, coding standards, conventions, .instructions.md, github/instructions.'
---

## When to Use This Skill

Activate when the user asks to:
- Create a new `.github/instructions/[name].instructions.md` file.
- Add or fix the YAML frontmatter (`description`, `applyTo`) of an existing instructions file.
- Structure or rewrite the body of an instructions file for clarity and completeness.
- Generate domain-specific guidance from a description of a pattern or standard.

---

## File Requirements

- **Location:** `.github/instructions/` directory.
- **Filename:** lowercase with hyphens — `nextjs.instructions.md`, `unit-of-work.instructions.md`.
- **Format:** Markdown with YAML frontmatter block at the top.

---

## Required Frontmatter

```yaml
---
description: "Brief description of the instruction purpose and scope"
applyTo: "glob pattern for target files"
---
```

### Frontmatter Rules

- `description` — double-quoted string, ≤500 chars, clearly states purpose and domain.
- `applyTo` — glob pattern(s) controlling which files trigger these instructions:
  - Single: `"**/*.ts"`
  - Multiple: `"**/*.ts, **/*.tsx"`
  - Scoped: `"skillexa-portal/**"`
  - All files: `"**"`
- Use `"**"` only when the instructions apply universally (e.g., security rules). Prefer narrow globs to avoid polluting context.

---

## Body Structure

A well-formed instructions file uses the following sections (include only what is relevant):

### 1. Title & Overview (`#`)
- One-line purpose statement.
- Optionally a brief context table (stack, version, location).

### 2. Core Principles / Guidelines
- High-level rules written in imperative mood: "Use", "Avoid", "Always", "Never".
- Bullet lists or numbered rules — not prose paragraphs.

### 3. Patterns & Code Examples
- Concrete, copy-paste-ready code snippets.
- Label each with `### Good Example` / `### Bad Example` when showing contrast.

### 4. Anti-Patterns (What NOT To Do)
- A table or bullet list of forbidden patterns with a one-line reason.

### 5. Configuration / Setup (if applicable)
- Config file snippets, environment variables, registration code.

### 6. References (optional)
- Links to related instruction files or external documentation.

---

## Writing Style

- Write in **imperative mood**: "Use", "Implement", "Avoid", "Never".
- Be specific and actionable — no vague guidance like "should" or "might".
- Use tables for rule sets; use code blocks for all code.
- Keep each section focused and scannable — no long prose paragraphs.
- Cross-link related instruction files by name (e.g., `see unit-of-work.instructions.md`).

---

## Template

`templates/template.instructions.md` — copy this scaffold and fill in each section.
