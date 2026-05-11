---
name: azure
description: 'Design, write, and review Azure DevOps Pipeline YAML files. USE FOR: creating or fixing azure-pipelines.yml; CI/CD pipeline stages, jobs, steps; caching NuGet/npm dependencies; deployment strategies (blue-green, canary); Key Vault secrets; service connections; approval gates; environment promotion (dev→staging→prod). DO NOT USE FOR: GitHub Actions workflows (use the github skill); local Docker Compose. Keywords: azure devops, azure pipelines, pipeline yaml, stages, deployment, service connection, variable group, artifact, dotnet build, bicep, ARM.'
---

## When to Use This Skill

Activate when the user asks to:
- Create or edit an `azure-pipelines.yml` file.
- Add stages, jobs, steps, templates, or conditions to an Azure DevOps pipeline.
- Configure caching, artifacts, or deployment jobs.
- Set up secrets via Azure Key Vault or variable groups.
- Implement approval gates, environments, or rollback mechanisms.
- Integrate testing, code coverage, or security scans into a pipeline.

---

## General Guidelines

- Use YAML with **2-space indentation** consistently.
- Always include meaningful `name` / `displayName` on pipelines, stages, jobs, and steps.
- Use `variables` and `parameters` to keep pipelines reusable and environment-independent.
- Follow **least privilege**: service connections and permissions should be scoped as narrowly as possible.
- Keep pipeline files **focused and modular** — split large pipelines into template files.

---

## Pipeline Structure

- Use **stages** for clear visualization and control flow (e.g., `Build`, `Test`, `Deploy`).
- Use **jobs** within stages to group related steps and enable parallel execution.
- Use **deployment jobs** (`deployment:`) for environment-targeted releases.
- Define explicit `dependsOn` between stages/jobs.
- Extract repeated logic into **template files** (`extends` / `template` references).

---

## Build Best Practices

- Pin agent pool images for reproducibility (e.g., `vmImage: 'ubuntu-24.04'`).
- Cache package manager dependencies to speed up builds:
  - NuGet: `$(NUGET_PACKAGES)` keyed on `**/packages.lock.json`
  - npm/pnpm: `$(Pipeline.Workspace)/.pnpm-store` keyed on `**/pnpm-lock.yaml`
- Publish build artifacts with meaningful names and retention policies.
- Use `$(Build.BuildId)` or `$(Build.SourceVersion)` for version metadata.
- Include quality gates: linting, unit tests, and security scans.

---

## Deployment Strategies

- Promote through environments: `dev → staging → production`.
- Use manual approval gates on `production` environments.
- Prefer **blue-green** or **canary** for zero-downtime deployments.
- Use **Infrastructure as Code** (Bicep / ARM / Terraform) for consistent environment provisioning.
- Include health checks and rollback steps in deployment jobs.

---

## Security Considerations

- Store all secrets in **Azure Key Vault** and surface them via variable groups linked to Key Vault.
- Never hard-code credentials in YAML files.
- Use **managed identities** over service principals where possible.
- Enable dependency vulnerability scans (e.g., OWASP Dependency-Check, Mend).
- Require approvals for production deployments.

---

## Template

`templates/pipeline.yml` — starter Azure Pipelines YAML. Copy and adapt it for a new pipeline.

---

## References

- [Azure Pipelines YAML schema](https://learn.microsoft.com/en-us/azure/devops/pipelines/yaml-schema/)
- [Pipeline caching](https://learn.microsoft.com/en-us/azure/devops/pipelines/release/caching)
- [Key Vault variable groups](https://learn.microsoft.com/en-us/azure/devops/pipelines/library/variable-groups?view=azure-devops&tabs=yaml#link-secrets-from-an-azure-key-vault)
