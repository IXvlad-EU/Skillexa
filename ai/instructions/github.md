## When to Use

- Create or edit a `.github/workflows/*.yml` file.
- Add CI steps (build, test, lint, publish) to a workflow.
- Configure deployment jobs targeting environments.
- Set up secret handling, OIDC cloud auth, or `GITHUB_TOKEN` permissions.
- Add caching, matrix testing, or reusable workflow calls.
- Integrate dependency review, CodeQL, or SAST scans.

---

## Workflow Structure

- Use a descriptive `name:` at the top of every workflow file.
- Name files consistently: `build-and-test.yml`, `deploy-prod.yml`, `codeql.yml`.
- Use granular `on:` triggers — prefer specific branch filters over bare `on: push`.
- Set `concurrency:` to prevent redundant runs on the same branch/PR.
- Set `permissions:` at workflow level with minimal defaults; override per job only when needed.

```yaml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

permissions:
  contents: read
```

---

## Jobs

- Each job = one distinct CI/CD phase (build, test, lint, deploy).
- Use `needs:` for explicit dependencies between jobs.
- Use `outputs:` to pass data between jobs.
- Use `if:` conditions for conditional execution (branch, event type, job status).
- Prefer `ubuntu-latest` runners; use `windows-latest` or `macos-latest` only when required.

---

## Steps & Actions

- Pin actions to a specific major version tag (e.g., `actions/checkout@v4`). For maximum security, pin to a full commit SHA.
- Never pin to `main` or `latest`.
- Give every step a descriptive `name:`.
- Use `run:` with `|` for multi-line scripts.
- Never hardcode secrets in `run:` or `env:` values.

---

## Security Best Practices

### Secrets
- Store sensitive values in **GitHub Secrets** or **environment secrets**.
- Access via `${{ secrets.MY_SECRET }}` — never construct dynamically.
- Use **environment secrets** with protection rules for production deployments.

### OIDC for Cloud Auth
- Use OIDC instead of long-lived credentials for AWS, Azure, and GCP.
- Exchange the short-lived GitHub OIDC JWT for temporary cloud credentials.

```yaml
permissions:
  id-token: write   # required for OIDC
  contents: read

steps:
  - uses: azure/login@v2
    with:
      client-id: ${{ secrets.AZURE_CLIENT_ID }}
      tenant-id: ${{ secrets.AZURE_TENANT_ID }}
      subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

### Least Privilege for `GITHUB_TOKEN`
- Start with `contents: read` and only add write permissions where strictly needed.
- Common permissions: `pull-requests: write`, `checks: write`, `packages: write`.

### Dependency & SAST Scanning
- Add `dependency-review-action` on PRs to catch vulnerable packages.
- Use CodeQL for static analysis (available via GitHub Advanced Security).

---

## Caching

Design cache keys around lock files to maximize cache hit rate:

```yaml
- uses: actions/cache@v4
  with:
    path: ~/.npm
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
    restore-keys: ${{ runner.os }}-node-
```

Common paths to cache: `~/.npm`, `~/.nuget/packages`, `~/.pnpm-store`, `~/.cache/pip`.

---

## Matrix Strategies

```yaml
strategy:
  fail-fast: false
  matrix:
    os: [ubuntu-latest, windows-latest]
    node-version: [20.x, 22.x]
```

Use `include:` / `exclude:` to fine-tune combinations without full cartesian products.

---

## Reusable Workflows

Extract shared CI logic into a reusable workflow (`workflow_call`) and call it from multiple pipelines to eliminate duplication.

---

## Template

`templates/build-and-test.yml` — starter GitHub Actions workflow for a .NET + Next.js monorepo. Copy and adapt it.

---

## References

- [GitHub Actions docs](https://docs.github.com/en/actions)
- [Workflow syntax](https://docs.github.com/en/actions/writing-workflows/workflow-syntax-for-github-actions)
- [Security hardening](https://docs.github.com/en/actions/security-for-github-actions/security-guides/security-hardening-for-github-actions)
- [Reusable workflows](https://docs.github.com/en/actions/sharing-automations/reusing-workflows)
