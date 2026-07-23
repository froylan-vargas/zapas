# Week 1, Day 2: Define the Runtime Contract

## Outcome

Make Zapas explicit about the conditions under which it is alive, ready, and correctly configured. Decide where the first Azure deployment should run and document why.

## Problem

The current API reads configuration dynamically, registers one database health check, and maps it to `/health`. That is a useful start, but a cloud platform needs clearer signals:

- Liveness: is the process responsive enough to keep running?
- Readiness: can this instance serve requests that require its dependencies?
- Startup validity: are required settings present and structurally valid?

If those meanings are mixed, an invalid database setting can cause unnecessary restart loops, while an apparently healthy process can receive traffic it cannot serve.

## Design

### 1. Write the configuration contract

Create a table in `documents/architecture/week01-azure-development.md`:

| Key | Required | Secret | Source in Azure | Failure behavior |
| --- | --- | --- | --- | --- |
| `ConnectionStrings__ZapasDb` | Yes | Treat as sensitive | App setting for Week 1 | Readiness fails; data routes unavailable |
| `Jwt__Authority` | Yes | No, but environment-specific | App setting | Authentication cannot validate tokens |
| `Jwt__Audience` | Yes | No, but environment-specific | App setting | Valid tokens may be rejected |
| `Cors__AllowedOrigins__0` | When browser client exists | No | App setting | Browser client blocked or overly broad |
| `Uploads__MaxFitFileSizeBytes` | Yes | No | App setting or safe code default | Upload bound ambiguous |
| `Uploads__AllowedExtensions__0` | Yes | No | App setting or safe code default | Upload validation ambiguous |

Validate required options at startup where a missing or malformed value always makes the deployment unusable. Do not mark an external dependency permanently unavailable merely because it had a brief network failure during startup.

### 2. Separate health semantics

Recommended API contract:

- `GET /health/live`: process liveness only; no database or internet dependency.
- `GET /health/ready`: readiness checks, including `ZapasDbContext`.

Keep responses small and avoid exposing connection details or exception text publicly. A `200` means the relevant contract is satisfied; a `503` means it is not.

Tag dependency checks as `ready` and filter the two endpoints with predicates. Add automated tests for status behavior.

### 3. Decide the compute platform

Compare:

| Option | Fit for current Zapas | Cost/complexity introduced | Trigger to reconsider |
| --- | --- | --- | --- |
| Azure App Service | Strong fit for one continuously available HTTP API and direct .NET publish | App Service plan and platform conventions | Container-specific runtime needs or more workload types |
| Azure Container Apps | Good if Zapas standardizes on containers or needs revision/scale-to-zero behavior | Image build, registry, container configuration | Choose now only if those capabilities solve a current need |
| Azure Functions | Weak fit for the existing controller-based API and synchronous FIT request path | Programming and hosting model changes | Discrete event-triggered workloads may use it later |

Write `learnings/adr/0001-host-zapas-on-azure-app-service.md` with:

- Status and date.
- Context grounded in the current code.
- Decision.
- Options considered.
- Positive and negative consequences.
- Revisit triggers.

An ADR records the best decision with current evidence; it is not a permanent promise.

## Build

### Task 1: Add typed runtime options

Use the existing `UploadOptions` pattern as a starting point. Decide which settings require typed option classes and startup validation. Preserve ASP.NET Core configuration precedence:

1. Base JSON for safe defaults.
2. Environment-specific JSON for local development.
3. Environment variables/platform settings for deployment.

Do not create a production JSON file containing environment values. Do not place secrets in Bicep parameter files, workflow YAML, or the repository.

### Task 2: Implement liveness and readiness

Modify health registration and endpoint mapping in `Program.cs`. Keep the database check in readiness only. Use official ASP.NET Core health-check primitives; avoid a custom controller unless a documented need exists.

Expected behavior:

| Scenario | `/health/live` | `/health/ready` |
| --- | --- | --- |
| Normal startup and database available | 200 | 200 |
| Process running, database invalid/unavailable | 200 | 503 |
| Process stopped | No response | No response |

### Task 3: Add integration tests

Extend the integration-test factory or create focused health tests that prove:

- Liveness returns `200`.
- Readiness returns `200` with the in-memory test database.
- Readiness returns `503` when its database dependency cannot be reached.
- Health responses do not expose a connection string or exception stack.

Keep tests deterministic. Do not require an Azure subscription.

### Task 4: Review proxy behavior

App Service terminates public TLS before forwarding traffic to the application. Verify how ASP.NET Core receives forwarded scheme information on the chosen host and whether platform integration handles it. Record the conclusion before adding forwarded-header middleware. Avoid cargo-cult middleware changes.

Verify that:

- Public traffic uses HTTPS.
- HTTP redirects correctly.
- JWT metadata requires HTTPS outside Development.
- Swagger remains intentionally disabled outside Development unless an ADR changes that choice.

### Task 5: Complete ADR-0001

The ADR should make one recommendation and admit its cost. Include these constraints:

- One instance for Week 1 due to SQLite and in-memory state.
- No deployment slot assumption if the selected development tier does not provide it.
- A runtime-support check for .NET 10 before provisioning.
- A container fallback only if native runtime availability blocks the chosen region.

## Verify

```powershell
dotnet build Zapas.slnx
dotnet test Zapas.slnx --no-build
dotnet publish Zapas.Api/Zapas.Api.csproj -c Release -o .artifacts/publish
```

Then run the published artifact using production-style environment settings and probe both endpoints.

Check that logs contain useful request properties without credentials, tokens, or uploaded file contents.

## Explain

Explain:

- Why liveness must not depend on SQLite.
- Why readiness should depend on SQLite today.
- Which configuration errors should stop startup versus fail readiness.
- Why environment variables are configuration transport, not automatically secret storage.
- Why App Service is a smaller justified step than Container Apps for current Zapas.

## Defend

Tech Lead challenges:

- “One `/health` endpoint was simpler. What problem did two endpoints solve?”
- “Why not put all settings in `appsettings.Production.json`?”
- “Container Apps is more cloud-native. Why aren’t we using it?”
- “Can the App Service scale to three instances right now?”
- “What observation would cause you to reverse ADR-0001?”

## Reflect

Record:

- A configuration value you misclassified and why.
- The difference between process health and business capability.
- One negative consequence of the hosting decision.
- One design question deferred deliberately to Week 2.

## Definition of done

- [ ] Required runtime settings and their sources are documented.
- [ ] Invalid mandatory configuration has intentional behavior.
- [ ] Liveness and readiness have distinct, tested meanings.
- [ ] HTTPS/proxy behavior is verified or captured as an experiment.
- [ ] ADR-0001 is accepted or explicitly proposed for review.
- [ ] The full test suite passes.

## Optional stretch

Add a build/version value to a non-sensitive diagnostic response or startup log so a deployment can be tied to a commit. Do not expose environment variables or assembly internals indiscriminately.

