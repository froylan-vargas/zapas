# Week 1, Day 3: Provision a Repeatable Azure Environment

## Outcome

Define the Azure development environment as code and prove that Zapas can be published and deployed without Visual Studio or portal-only steps.

## Problem

A manually created cloud resource is difficult to review, reproduce, and remove. Zapas needs a small, explicit environment whose purpose, cost, region, ownership, and constraints are visible in source control.

The infrastructure must reflect the current design: one web API instance and disposable SQLite data. It must not imply durable production storage.

## Design

### 1. Define the resource boundary

For the recommended App Service path, the minimum development environment is:

- One resource group, created by a bootstrap command because a resource-group-scoped Bicep deployment needs it to exist.
- One Linux App Service plan on a low-cost development SKU selected after checking regional/runtime support.
- One Web App configured for the API runtime.
- App settings for environment, identity metadata, CORS, uploads, and the temporary SQLite connection string.
- Tags such as `application=zapas`, `environment=dev`, `managed-by=bicep`, `owner=<team-or-user>`, and an expiry/review date.

Do not add Application Insights, Key Vault, a container registry, a database server, private networking, or API Management today. Put future candidates in the decision log with the requirement that would justify each one.

### 2. Separate code, configuration, and secrets

Use Bicep for stable resource structure and non-secret defaults. Pass environment-specific values through parameters. Pass secret values at deployment time from an approved secret source; do not commit them.

Although the Week 1 SQLite connection string has no database password, model connection strings as sensitive because Week 2 will replace it.

### 3. Make names predictable but globally unique

Azure Web App names are globally unique. Generate the suffix deterministically from subscription/resource-group inputs in Bicep when possible. Do not hardcode someone’s personal suffix in the reusable template.

Outputs should include only safe values such as:

- Web App name.
- Default hostname.
- Resource group or region.

Never output credential material.

### 4. Decide the deployment unit

The deployment unit is the Release output of:

```powershell
dotnet publish Zapas.Api/Zapas.Api.csproj -c Release
```

Infrastructure deployment and application deployment are separate steps. This distinction makes it possible to determine whether a failure is in resource provisioning, configuration, artifact creation, or artifact rollout.

## Build

### Task 1: Verify Azure context safely

```powershell
az login
az account show --query "{subscription:name, tenant:tenantId, user:user.name}" --output table
az account list-locations --query "[].{name:name, displayName:displayName}" --output table
```

Select the intended subscription explicitly:

```powershell
az account set --subscription "<subscription-id-or-name>"
```

Do not paste tokens or the full CLI profile into documentation.

### Task 2: Verify runtime and SKU availability

Use Azure CLI or the portal to verify:

- The chosen region is permitted by the subscription.
- The chosen plan SKU is available and affordable.
- The selected host supports the project’s .NET runtime.

Capture the query and conclusion in the architecture document. Service availability changes; do not encode an old tutorial’s assumptions as facts.

### Task 3: Create Bicep

Add:

```text
infrastructure/
|-- main.bicep
`-- parameters/
    `-- dev.bicepparam
```

The template should:

- Accept location, environment, tags/owner, and required non-secret settings.
- Provision the plan and Web App.
- Configure HTTPS-only behavior and the health-check path supported by the platform.
- Configure one instance for the temporary SQLite design.
- Set the production environment name.
- Avoid embedding subscription IDs, tenant IDs, credentials, or personal paths.
- Return safe deployment outputs.

Use `/health/ready` for routing readiness if the selected platform feature supports it. Keep `/health/live` available for diagnosis.

### Task 4: Lint and preview

```powershell
az bicep build --file infrastructure/main.bicep
az group create `
  --name "<zapas-dev-resource-group>" `
  --location "<region>" `
  --tags application=zapas environment=dev managed-by=bicep
az deployment group what-if `
  --resource-group "<zapas-dev-resource-group>" `
  --template-file infrastructure/main.bicep `
  --parameters infrastructure/parameters/dev.bicepparam
```

Read the what-if output. Explain every resource to be created or changed. Unexpected deletion or replacement is a stop condition.

### Task 5: Deploy infrastructure

```powershell
az deployment group create `
  --name "zapas-dev-infrastructure" `
  --resource-group "<zapas-dev-resource-group>" `
  --template-file infrastructure/main.bicep `
  --parameters infrastructure/parameters/dev.bicepparam
```

Run the same command again and confirm it is idempotent: no unintended resources or configuration drift should appear.

### Task 6: Publish and deploy manually once

Before automating, understand the primitive:

```powershell
dotnet publish Zapas.Api/Zapas.Api.csproj `
  -c Release `
  -o .artifacts/publish

Compress-Archive `
  -Path .artifacts/publish/* `
  -DestinationPath .artifacts/zapas-api.zip `
  -Force

az webapp deploy `
  --resource-group "<zapas-dev-resource-group>" `
  --name "<web-app-name>" `
  --src-path .artifacts/zapas-api.zip `
  --type zip
```

If the platform recommends a different current deployment command, record the exact command and reason in the runbook. The archive must contain the publish output at its root, not an extra parent directory.

### Task 7: Write the runbook

Create or update `documents/architecture/week01-azure-development.md` with:

- Prerequisites.
- Subscription and region selection.
- Provision, what-if, deploy, verify, rollback, and teardown commands.
- Expected health behavior.
- Where logs can be read.
- The explicit SQLite data-loss warning.
- Common failure modes and safe recovery steps.

## Verify

- Bicep compiles and what-if contains only expected changes.
- A second infrastructure deployment is idempotent.
- Resource names and tags identify purpose and ownership.
- Public traffic is HTTPS-only.
- Required settings exist without values appearing in Git or command evidence.
- The Release artifact deploys and the process starts.
- No assumption of durable SQLite data is made.

## Explain

Explain:

- Why the resource group is a useful lifecycle boundary.
- What Bicep controls and what the application deployment controls.
- Why a parameter file is not a safe place for secrets merely because it is “infrastructure.”
- What idempotence means in this deployment.
- Which Azure billable resources now exist.

## Defend

Tech Lead challenges:

- “Why not create everything with portal clicks once?”
- “Why does the app need a globally unique name?”
- “Why not add Key Vault now?”
- “If Bicep owns configuration, can an operator change an app setting manually?”
- “How do we know tearing down the resource group will not delete shared resources?”

## Reflect

Record:

- One difference between application and infrastructure deployment.
- One surprising item in what-if output.
- One resource you chose not to add and the trigger that would justify it.
- The most likely operational error in the current runbook.

## Definition of done

- [ ] Infrastructure is source controlled and compiles.
- [ ] What-if was reviewed before deployment.
- [ ] Reapplying Bicep is safe.
- [ ] Zapas can be deployed from a Release artifact without an IDE.
- [ ] Runtime configuration is external and no secrets are committed.
- [ ] The deployment and teardown runbook is reproducible.

## Optional stretch

Run Azure Resource Graph or `az resource list` for the resource group and compare actual resources to Bicep. Record drift without “fixing” it manually.

