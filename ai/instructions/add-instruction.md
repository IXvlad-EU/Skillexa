# When to Use This Instruction

- Create a new instruction file inside a service-local `ai/instructions/` directory.
- Improve or restructure an existing instruction document.
- Document coding standards, architectural constraints, implementation rules, or best practices.
- Convert implicit conventions into explicit AI-readable documentation.

---

# File Requirements

- Location:
  - `ai/instructions/` (for shared instructions)
  - `skillexa-core/ai/instructions/`
  - `skillexa-engine/ai/instructions/`
  - `skillexa-portal/ai/instructions/`

- Filename:
  - lowercase
  - hyphen-separated
  - concise and domain-oriented

Examples:

```txt
database.md
authentication.md
tanstack-query.md
unit-of-work.md
```

---

# Frontmatter

Frontmatter is optional.

If used, keep it lightweight and informational only.

Do not rely on frontmatter as the primary instruction-routing mechanism.

Repository structure, locality, and `AGENTS.md` files are the primary context-routing system.

---

# Document Structure

Use only sections that are relevant.

## 1. Title & Overview

- One-line explanation of the instruction purpose.
- Keep concise.

Example:

```md
# CQRS

CQRS handler structure and database access rules for Skillexa-Core.
```

---

## 2. Rules / Guidelines

- Use imperative language:
  - Use
  - Avoid
  - Always
  - Never

Prefer:
- bullet lists
- short rules
- explicit constraints

Avoid:
- long prose paragraphs
- theoretical explanations

---

## 3. Good Examples

Provide realistic project-specific examples.

Use copy-paste-ready snippets.

Example sections:

```md
### Good Example
```

```md
### Bad Example
```

---

## 4. Anti-Patterns

Document forbidden or dangerous patterns.

Example:

| Pattern                             | Why it's wrong                        |
| ----------------------------------- | ------------------------------------- |
| Direct DbContext usage in endpoints | Bypasses CQRS and unit-of-work        |
| Business logic inside controllers   | Violates application layer boundaries |

---

## 5. Related Documentation

Link related local documents.

Example:

```md
## Related

- `../AGENTS.md`
- `instructions/database.md`
```

---

# Writing Rules

- Optimize for AI retrieval and scanning.
- Keep sections compact.
- Prefer explicit constraints over vague recommendations.
- Avoid marketing language and philosophy.
- Avoid excessive abstraction.
- Keep examples realistic and project-specific.

---

# Important Constraints

- Do not create unnecessary documentation layers.
- Do not split files aggressively.
- Do not introduce workflows unless the process is genuinely complex.
- Prefer locality: documentation should live near the bounded context it describes.
- Reuse existing terminology and patterns before introducing new abstractions.

---

# Goal

The instruction system must remain:

- lightweight
- maintainable
- retrieval-friendly
- vendor-neutral
- understandable without GitHub-specific tooling
