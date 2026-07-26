# Week 1 Pre-Day 1 Prerequisites

## Purpose

Complete this preflight before starting the ten-hour Week 1 schedule in `week01_container_images_and_app_service.md`. It verifies the workstation, current Zapas baseline, Azure access, permissions, cost controls, and safe configuration. It does **not** containerize Zapas, create Week 1 compute or registry resources, or perform any Day 1 implementation.

Allow 30–90 minutes when the tools and permissions already exist. Installation, corporate approvals, or Azure RBAC changes can take longer and should remain outside the ten-hour study budget.

This guide assumes Windows and PowerShell. Run commands from the repository root:

```powershell
Set-Location 'C:\Users\froylan.vargas\source\repos\eomd\zapas'
```

Replace every `<placeholder>` yourself. Never paste credentials, tokens, client secrets, or user-secret output into residency artifacts, command logs, screenshots, or chat.

## 1. Record the repository baseline

1. Confirm the location and current changes:

   ```powershell
   Get-Location
   git status --short --untracked-files=all
   ```

2. Confirm that the expected projects are present:

   ```powershell
   dotnet sln .\Zapas.slnx list
   ```

   Expected: `Zapas.Api/Zapas.Api.csproj` and `Zapas.Api.Tests/Zapas.Api.Tests.csproj`.

3. Record any pre-existing changes. The residency Markdown files may still be untracked; that is acceptable if intentional. Do not discard or overwrite unrelated work.

**Gate:** You know which files are baseline work and which changes belong to the residency.

## 2. Verify the .NET toolchain and baseline tests

1. Inspect installed SDKs:

   ```powershell
   dotnet --info
   dotnet --list-sdks
   ```

2. Confirm that a .NET 10 SDK is listed. Installing only the runtime is insufficient because Week 1 builds and publishes the API. If it is missing, use Microsoft’s [.NET installation guide for Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows), reopen PowerShell, and repeat the checks.

3. Restore and test the current solution before container work begins:

   ```powershell
   dotnet restore .\Zapas.slnx
   dotnet test .\Zapas.slnx --no-restore
   ```

4. If restore fails, resolve NuGet, proxy, certificate, or SDK issues now. If tests fail, record the failing test and decide whether it is a genuine pre-existing defect before Day 1. Do not hide a failure by changing application code during this preflight.

**Gate:** A .NET 10 SDK is available, restore succeeds, and the current test suite passes or has an explicitly accepted baseline failure.

## 3. Install and verify Linux-container support

1. Check Windows Subsystem for Linux:

   ```powershell
   wsl --version
   wsl --status
   ```

2. If WSL 2 is missing or disabled, follow Microsoft’s [WSL installation guidance](https://learn.microsoft.com/en-us/windows/wsl/install) and complete any required reboot before continuing.

3. Install or update Docker Desktop using the official [Docker Desktop for Windows instructions](https://docs.docker.com/desktop/setup/install/windows-install/). Use the WSL 2 backend and Linux containers. Corporate-managed machines may require administrator or security approval.

4. Start Docker Desktop and verify that both client and server respond:

   ```powershell
   docker version
   docker info --format '{{.OSType}}'
   ```

   Expected operating-system value: `linux`.

5. Verify a container can start:

   ```powershell
   docker run --rm hello-world
   ```

6. Verify access to the .NET 10 image registry and optionally prewarm the two images Week 1 is likely to use:

   ```powershell
   docker pull mcr.microsoft.com/dotnet/sdk:10.0
   docker pull mcr.microsoft.com/dotnet/aspnet:10.0
   ```

   These pulls do not define the final Dockerfile; Day 1 will choose and validate exact image variants.

**Gate:** Docker reports a Linux engine, a test container exits successfully, and both .NET 10 image pulls succeed.

## 4. Install and authenticate the Azure CLI

1. Check the CLI:

   ```powershell
   az version
   ```

2. If it is missing, install the official package as described in [Install Azure CLI on Windows](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows). A supported WinGet path is:

   ```powershell
   winget install --exact --id Microsoft.AzureCLI
   ```

3. Close and reopen PowerShell after installation. If the installed CLI is old, update it:

   ```powershell
   az upgrade
   ```

4. Sign in and inspect available subscriptions:

   ```powershell
   az login
   az account list --output table
   ```

   If the required account belongs to a different tenant, sign in to that tenant explicitly using its tenant ID. Do not record access tokens.

5. Select the approved subscription and verify it:

   ```powershell
   az account set --subscription '<subscription-name-or-id>'
   az account show --query '{name:name,id:id,tenantId:tenantId,state:state}' --output table
   ```

   Expected state: `Enabled`. Read the displayed subscription and tenant carefully; all later commands depend on this selection.

6. Confirm that the current token can be issued without printing it:

   ```powershell
   az account get-access-token --query expiresOn --output tsv
   ```

**Gate:** The CLI is current enough to support ACR and App Service commands, and the intended subscription is active.

## 5. Verify Azure permissions before provisioning day

Week 1 needs two categories of permission:

- Resource management: create and delete a resource group, ACR, App Service plan, web app, and system-assigned identity.
- Access management: assign the App Service identity the correct ACR image-pull role. `Contributor` alone cannot create Azure role assignments.

1. Capture the current IDs in session-only PowerShell variables:

   ```powershell
   $ZapasSubscriptionId = az account show --query id --output tsv
   $ZapasSubscriptionScope = "/subscriptions/$ZapasSubscriptionId"
   $ZapasPrincipalId = az ad signed-in-user show --query id --output tsv
   ```

2. List direct, group, and inherited assignments:

   ```powershell
   az role assignment list `
     --assignee-object-id $ZapasPrincipalId `
     --scope $ZapasSubscriptionScope `
     --all `
     --include-groups `
     --include-inherited `
     --query '[].{Role:roleDefinitionName,Scope:scope}' `
     --output table
   ```

3. Confirm with the subscription owner that the effective permissions cover the intended disposable lab scope. Typical workable combinations are:

   - `Owner`; or
   - `Contributor` plus `Role Based Access Control Administrator` or `User Access Administrator`; or
   - custom roles that include the required resource operations and `Microsoft.Authorization/roleAssignments/write` at the planned scope.

   Prefer the narrowest approved resource-group or registry scope. Do not request subscription-wide Owner merely for convenience. See Microsoft’s [Azure built-in roles](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles) and [role-assignment CLI reference](https://learn.microsoft.com/en-us/cli/azure/role/assignment?view=azure-cli-latest).

4. If access is missing, request it now. State that Week 1 must create disposable ACR/App Service resources and assign one managed identity a pull-only ACR role. Ask the administrator to time-bound or remove elevated access after the lab where possible.

**Gate:** Resource-creation and role-assignment authority are confirmed at an agreed scope. Do not discover this blocker on Day 2 or Day 4.

## 6. Verify required Azure resource providers and region options

1. Check only the providers needed this week:

   ```powershell
   az provider show --namespace Microsoft.ContainerRegistry --query '{Provider:namespace,State:registrationState}' --output table
   az provider show --namespace Microsoft.Web --query '{Provider:namespace,State:registrationState}' --output table
   ```

2. If either provider is `NotRegistered`, first reconfirm the subscription. Then register only the missing provider:

   ```powershell
   az provider register --namespace Microsoft.ContainerRegistry --wait
   az provider register --namespace Microsoft.Web --wait
   ```

   Provider registration changes subscription state but creates no Week 1 service resource. It requires the provider `register/action`, normally available through Contributor or Owner. Microsoft recommends registering only providers you are ready to use; see [Azure resource providers and types](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/resource-providers-and-types).

3. Review available locations for the two resource types:

   ```powershell
   az provider show --namespace Microsoft.ContainerRegistry --query "resourceTypes[?resourceType=='registries'].locations | [0]" --output table
   az provider show --namespace Microsoft.Web --query "resourceTypes[?resourceType=='serverfarms'].locations | [0]" --output table
   ```

4. Choose a nearby region present in both results and allowed by organizational policy. Confirm that it offers a Linux App Service plan SKU you are permitted to use. Record only the chosen region and SKU—not a provisioning command.

**Gate:** `Microsoft.ContainerRegistry` and `Microsoft.Web` are registered, and one approved common region/SKU is identified.

## 7. Establish cost controls and naming

1. Open Azure Portal and select the intended subscription.
2. Go to **Cost Management + Billing > Cost Management > Budgets > Add**.
3. Create a monthly budget appropriate for your personal or organizational lab allowance.
4. Add actual-cost notifications at useful early thresholds, for example 50%, 80%, and 100%, and at least one forecast notification.
5. Send notifications to an address you actively monitor. A budget is an alerting mechanism; it does not automatically cap or stop spending.
6. Review the [Azure pricing calculator](https://azure.microsoft.com/pricing/calculator/) for ACR and a Linux App Service plan in the chosen region. Record the expected order of cost, not a guaranteed quote.
7. Agree on a unique naming prefix and the tags to apply during daily sessions. Suggested tags:

   | Tag | Planned value |
   |---|---|
   | `residency` | `senior-azure-ai` |
   | `week` | `01` |
   | `purpose` | `container-lab` |
   | `owner` | `<your-alias>` |
   | `expiresOn` | `<planned-cleanup-date>` |

Use Microsoft’s [budget tutorial](https://learn.microsoft.com/en-us/azure/cost-management-billing/costs/tutorial-acm-create-budgets) as the current reference.

**Gate:** A budget/alert exists, estimated cost has been reviewed, and region, SKU, naming prefix, tags, and cleanup date are recorded.

## 8. Resolve the ACR Tasks subscription risk

Microsoft currently states that ACR task runs are temporarily paused from Azure free credits. Local Docker builds are not a substitute for the C2 cloud-build/run evidence.

1. In Azure Portal, open **Subscriptions**, select the intended subscription, and identify its offer/billing arrangement.
2. If the lab depends only on free promotional credits, ask the subscription administrator or Microsoft support whether ACR Tasks can run under that offer.
3. If they cannot, arrange an approved subscription with paid consumption before Day 3. Do not silently skip C2 or spend from an unapproved account.
4. Record the result as one of: `confirmed`, `known blocked with approved alternate`, or `requires administrator confirmation`.

See the current [ACR Tasks overview](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-tasks-overview). A real task run can only be proven after the registry exists on Day 2; this preflight removes known billing blockers.

**Gate:** There is a credible, approved path to execute ACR quick builds and runs on Day 3.

## 9. Prepare safe Zapas configuration

Zapas validates JWT configuration at startup. Its current files provide upload defaults but do not provide `Jwt:Authority`, `Jwt:Audience`, or `ConnectionStrings:ZapasDb`.

1. Review `documents/local_external_auth_setup.md`. Do not copy its tenant-specific identifiers into this file or commit new tenant values to `appsettings*.json`.
2. Prefer ASP.NET Core user secrets for local host validation. The project already has a `UserSecretsId`:

   ```powershell
   dotnet user-secrets set 'Jwt:Authority' 'https://<your-identity-domain>/' --project .\Zapas.Api\Zapas.Api.csproj
   dotnet user-secrets set 'Jwt:Audience' '<your-api-audience>' --project .\Zapas.Api\Zapas.Api.csproj
   dotnet user-secrets set 'Cors:AllowedOrigins:0' '<your-local-frontend-origin>' --project .\Zapas.Api\Zapas.Api.csproj
   dotnet user-secrets set 'ConnectionStrings:ZapasDb' 'Data Source=Zapas.db' --project .\Zapas.Api\Zapas.Api.csproj
   ```

   Use real approved Auth0 values if authenticated API testing is planned. For health-only preflight, clearly labelled non-production values may satisfy startup validation but do not prove authentication.

3. Do not put a client secret in these commands. Zapas currently validates access tokens with authority and audience; it does not require an Auth0 client secret to start.
4. Do not run `dotnet user-secrets list` in captured output because it prints values. See Microsoft’s [ASP.NET Core development-secrets guidance](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0).
5. Prepare this runtime worksheet without choosing the final container values yet:

   | Purpose | Local .NET key | Container/App Service form | Decide when |
   |---|---|---|---|
   | JWT issuer | `Jwt:Authority` | `Jwt__Authority` | Before Day 1 |
   | JWT audience | `Jwt:Audience` | `Jwt__Audience` | Before Day 1 |
   | CORS origin | `Cors:AllowedOrigins:0` | `Cors__AllowedOrigins__0` | Before Day 1 |
   | SQLite path | `ConnectionStrings:ZapasDb` | `ConnectionStrings__ZapasDb` | Validate Day 1 |
   | ASP.NET environment | `ASPNETCORE_ENVIRONMENT` | Same | Validate Day 1 |
   | HTTP listening port | ASP.NET Core URL/port setting | Container setting plus `WEBSITES_PORT` | Choose Day 1; apply Day 4 |

**Gate:** Required local values exist outside Git, no secret was copied into repository files, and unresolved container-specific choices are explicitly reserved for Day 1.

## 10. Prove the existing host application still starts

This is a baseline check, not container implementation.

1. Start the current API using its HTTP launch profile:

   ```powershell
   dotnet run --project .\Zapas.Api\Zapas.Api.csproj --launch-profile http
   ```

2. In a second PowerShell window, verify the existing endpoints:

   ```powershell
   Invoke-WebRequest 'http://localhost:5222/health/live' -UseBasicParsing
   Invoke-WebRequest 'http://localhost:5222/health/ready' -UseBasicParsing
   ```

3. Expected: both return HTTP 200 with the current local SQLite setup. If liveness succeeds but readiness fails, resolve or record the pre-existing connection/path problem before containerization.
4. Stop the API with `Ctrl+C`.

**Gate:** The non-containerized API starts, liveness succeeds, and readiness behavior is understood.

## 11. Check network and corporate-policy constraints

1. Verify outbound HTTPS to the main control-plane endpoints:

   ```powershell
   Test-NetConnection management.azure.com -Port 443
   Test-NetConnection login.microsoftonline.com -Port 443
   Test-NetConnection mcr.microsoft.com -Port 443
   ```

2. Confirm that corporate proxy/firewall policy permits Azure management APIs, Microsoft Container Registry, NuGet, the configured identity provider, and later access to the chosen `<registry-name>.azurecr.io` endpoint.
3. If TLS inspection or a private certificate authority is used, make sure both the Windows host and Docker’s Linux environment trust the required certificate chain. Resolve this before Day 1 rather than weakening certificate validation.
4. Confirm that organizational Azure Policy does not prohibit ACR, Linux App Service, managed identities, required regions, or the planned SKUs. Ask the subscription owner when policy visibility is limited.

**Gate:** No known proxy, firewall, certificate, or Azure Policy blocker remains.

## 12. Final readiness checklist

Do not start Day 1 until every required item is checked or has an explicit approved exception.

- [ ] Repository baseline and expected uncommitted files recorded.
- [ ] .NET 10 SDK installed; restore and tests succeed.
- [ ] Docker uses Linux containers; test container and .NET 10 pulls succeed.
- [ ] Azure CLI is installed, authenticated, and set to the correct enabled subscription.
- [ ] Resource-creation and role-assignment permissions are confirmed.
- [ ] `Microsoft.ContainerRegistry` and `Microsoft.Web` are registered.
- [ ] Approved common region and Linux App Service SKU identified.
- [ ] Budget, alerts, pricing review, naming prefix, tags, and cleanup date recorded.
- [ ] ACR Tasks billing/offer risk resolved or an approved alternate subscription exists.
- [ ] Required Zapas values are stored outside Git; no tenant value or secret was committed.
- [ ] Current API starts; `/health/live` and `/health/ready` behavior is known.
- [ ] Network, proxy, certificate, and Azure Policy constraints are cleared.
- [ ] No ACR, App Service, container file, or other Week 1 implementation was created during preflight.

When the checklist is complete, begin Day 1 with a fresh repository reinspection as required by the weekly playbook.
