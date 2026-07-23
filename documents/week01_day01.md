# Week 1, Day 1: Establish the Baseline

## Outcome

Create an evidence-based cloud-readiness assessment of Zapas. At the end of the day, another engineer should be able to build the repository, run its tests, explain one request path, and name the most important blockers to a responsible Azure deployment.

## Problem

“Deploy it to Azure” is not a useful requirement until the team knows what the application needs at runtime. Zapas has authentication, a database, local caching, uploads, health checks, and synchronous parsing. Each creates a hosting or operational constraint.

Today is successful when the deployment plan begins from those constraints rather than from a catalog of Azure services.

## Design

### 1. Trace the real application

Read these files in order:

1. `README.md`
2. `Zapas.Api/Program.cs`
3. `Zapas.Api/Controllers/SessionsController.cs`
4. `Zapas.Api/Services/Sessions/SessionService.cs`
5. `Zapas.Api/Repositories/SessionRepository.cs`
6. `Zapas.Api/appsettings.Development.json`
7. `Zapas.Api.Tests/Infrastructure/ZapasApiFactory.cs`

Draw the upload request from client to persistence. Include:

- JWT authentication and the `Athlete` role.
- Request size and fixed-window rate limits.
- Upload option validation.
- Synchronous FIT parsing.
- EF Core persistence.
- In-memory caching.
- Structured request logging.
- Cancellation and failure paths.

Then trace `GET /sessions/{id}` and `/health`. Notice which dependencies each path exercises.

### 2. Classify dependencies

For each runtime dependency, record:

- Whether it is stateful or stateless.
- Whether it is local, external, or platform-provided.
- How it is configured.
- What happens when it is unavailable.
- Whether multiple application instances can use it safely.

Start with:

| Dependency | Current implementation | Important cloud question |
| --- | --- | --- |
| Compute | Local ASP.NET Core process | Which managed HTTP host meets current needs? |
| Data | Local SQLite file | Where is the file and what happens on replacement/scale-out? |
| Identity | Auth0 JWT authority | Can the cloud host reach it and is metadata valid? |
| Cache | `IMemoryCache` | Is per-instance state acceptable? |
| Upload processing | Synchronous Garmin FIT parser | What CPU, memory, size, and timeout bounds apply? |
| Configuration | Development JSON | Which values must be platform settings? |
| Health | EF Core check at `/health` | Does one endpoint express both liveness and readiness? |
| Logs | `ILogger` to configured providers | Where can an operator query cloud logs? |

### 3. Create a first decision filter

Use these requirements to compare hosting options tomorrow:

- Native HTTPS ingress.
- Runs the existing ASP.NET Core API with minimal application change.
- Supports runtime settings without committing production values.
- Provides deploy, restart, log, and health-probe capabilities.
- Can be provisioned through infrastructure as code.
- Supports federated automation from CI/CD.
- Has a low-cost development configuration.
- Does not require orchestration Zapas has not earned.

## Build

### Task 1: Verify local prerequisites

Record versions, not machine-specific paths:

```powershell
dotnet --info
git --version
az version
```

If Azure CLI is not installed, record it as a setup task. Do not block the code baseline on Azure access.

### Task 2: Establish the clean baseline

From the repository root:

```powershell
dotnet restore Zapas.slnx
dotnet build Zapas.slnx --no-restore
dotnet test Zapas.slnx --no-build
dotnet publish Zapas.Api/Zapas.Api.csproj -c Release -o .artifacts/publish
```

Add `.artifacts/` to `.gitignore` if it is not already ignored. Inspect the publish directory, then explain which files are needed to run the API.

The initial repository inspection found 11 passing tests and high-severity NuGet vulnerability warnings. Re-run without `--no-restore` when evaluating advisories so the result is current:

```powershell
dotnet list Zapas.Api/Zapas.Api.csproj package --vulnerable --include-transitive
dotnet list Zapas.Api.Tests/Zapas.Api.Tests.csproj package --vulnerable --include-transitive
```

Do not apply blind upgrades. Identify the top-level package that introduces each transitive dependency, find a compatible fixed version, run the complete test suite, and record one of:

- Remediated now.
- Accepted temporarily with reason, owner, and review date.
- Blocked from deployment.

### Task 3: Run the application from its published artifact

Supply development settings through environment variables to practice the production configuration model:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__ZapasDb = "Data Source=zapas-local.db"
$env:Jwt__Authority = "<your Auth0 authority>"
$env:Jwt__Audience = "zapas-api"
$env:Cors__AllowedOrigins__0 = "https://localhost.example"
dotnet .artifacts/publish/Zapas.Api.dll
```

Use non-secret placeholders when only testing startup. Remove task-specific environment variables when finished.

### Task 4: Start the residency living documents

Create or update:

- `documents/engineering_wisdom.md`
- `documents/architecture_evolution.md`
- `documents/decision_log.md`
- `documents/tech_lead_reviews.md`

Keep entries dated. Link evidence rather than pasting large command logs. Add the SQLite, per-instance cache, per-instance rate limiter, package advisory, and ownership-query concerns from the weekly risk table.

### Task 5: Create the current-state diagram

Add `learnings/architecture/week01-current-state.md` with a Mermaid diagram. It must show boundaries and data flow, not only class names. Label local process memory and the local SQLite file explicitly.

## Verify

- A clean restore, build, test, and Release publish succeeds.
- The published assembly starts with external configuration.
- `/health` behavior is observed with both a valid and invalid database setting.
- No credential value or local database was added to Git.
- The risk register distinguishes a fact, an assumption, and a decision.
- The current-state diagram agrees with `Program.cs`.

## Explain

Give a five-minute explanation:

1. What must exist for Zapas to start?
2. What must exist for it to serve a useful authenticated request?
3. Which state disappears when the process restarts?
4. Which state becomes inconsistent with two instances?
5. What does the current test suite prove, and what does it not prove?

## Defend

Respond as if a Tech Lead asks:

- “The tests pass. Why not deploy immediately?”
- “Why can’t we keep SQLite if the app is small?”
- “Is Auth0 configuration a secret?”
- “Why investigate a transitive dependency warning?”
- “What evidence would tell us Zapas is safe to scale horizontally?”

Strong answers separate current facts from future assumptions and connect risks to actual Zapas behavior.

## Reflect

Add a dated entry to `engineering_wisdom.md`:

- The most surprising runtime dependency.
- One assumption you originally treated as a fact.
- The highest-risk unknown.
- The evidence you need tomorrow.

## Definition of done

- [ ] Baseline commands are reproducible.
- [ ] All test results and dependency warnings are recorded.
- [ ] Current request and dependency flows are diagrammed.
- [ ] Cloud-readiness risks have owners or target weeks.
- [ ] The four Week 1 living documents exist.
- [ ] No application feature or Azure service was added without a stated need.

## Optional stretch

Measure the Release publish size and cold-start time locally. Treat the numbers as a baseline, not an optimization target.

