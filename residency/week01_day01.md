# Week 1, Day 1 — Container Contract and Local Proof

**Date:** July 24, 2026  
**Time box:** 120 minutes after prerequisites pass  
**Exam domain:** Develop containerized solutions on Azure  
**Objectives introduced:** C1 and C3  
**Daily goal:** Define, build, and locally prove the Zapas container contract without confusing local evidence with complete ACR or App Service competency.

## Readiness gate

Day 1 is currently blocked by workstation prerequisites:

- .NET SDK `10.0.302` is installed.
- Azure CLI `2.88.0` is installed, but authentication and subscription permissions were not revalidated.
- All 14 Zapas tests pass.
- Docker is not installed or is not on `PATH`.
- WSL is not installed.
- No `Dockerfile`, `.dockerignore`, Azure deployment asset, ADR, exam log, or competency matrix exists.
- `residency/` is currently untracked.

Installing WSL and Docker Desktop, rebooting, and resolving corporate policy are preflight work, not part of the 120-minute session. Complete `residency/prerequisites.md` before starting the timed lab.

Required remediation from an elevated PowerShell:

```powershell
wsl --install
```

After any required reboot, install Docker Desktop through the approved corporate process or:

```powershell
winget install --exact --id Docker.DockerDesktop
```

Start Docker Desktop with its WSL 2 backend and Linux containers, then validate in a new ordinary PowerShell:

```powershell
wsl --status
docker version
docker info --format '{{.OSType}}'
docker run --rm hello-world
docker pull mcr.microsoft.com/dotnet/sdk:10.0
docker pull mcr.microsoft.com/dotnet/aspnet:10.0
```

Expected result: WSL 2 is available, Docker client and server respond, the engine reports `linux`, the test container succeeds, and both .NET 10 images pull.

Common failures:

- `docker` is not recognized: reopen the shell and verify Docker Desktop installation.
- The client works but the server does not: start Docker Desktop and wait for engine readiness.
- `OSType` is `windows`: switch Docker Desktop to Linux containers.
- Pulls fail with timeouts or certificate errors: investigate proxy, firewall, TLS inspection, and the Docker Linux trust store. Do not disable certificate validation.

Do not claim this day complete until the Linux-container gate passes and the local image is actually validated.

## 1. Current repository state

### Relevant implementation

- `Zapas.slnx` contains the .NET 10 `Zapas.Api` and `Zapas.Api.Tests` projects.
- `Program.cs` composes options, application services, security, persistence, documentation, and middleware through extension methods.
- Options validation requires a valid HTTPS `Jwt:Authority` and non-empty `Jwt:Audience` at startup.
- EF Core uses SQLite through `ConnectionStrings:ZapasDb`.
- `/health/live` has no dependency checks.
- `/health/ready` includes the tagged EF Core SQLite check.
- `UseHttpsRedirection()` is enabled. The local container will listen on internal HTTP; a future App Service deployment will terminate public TLS.
- `appsettings*.json` contains logging only. Runtime values can be supplied with ASP.NET Core environment-variable names such as `Jwt__Authority`.
- SQLite, `IMemoryCache`, and rate-limit counters are process-local. SQLite is only disposable, single-instance Week 1 lab storage.

### Already complete

- The host solution restores, builds, and passes 14 tests.
- Liveness and readiness are separated and covered by tests.
- Required runtime values are externalizable.
- Local database and common secret/configuration files are ignored by Git.
- Restart and multi-instance limitations are documented in `learnings/architecture/week01-current-state.md`.

### Missing

- Linux container packaging and a bounded build context.
- A chosen container port, runtime-user, and writable-state contract.
- Local image build/run/health evidence.
- Container-specific automated evidence.
- ACR storage, versioning, task, authentication, and authorization evidence.
- App Service image pull, settings, health, logging, update, and rollback evidence.
- ADR, exam-error-log, and competency-matrix entries.

### Assumptions and risks

- The runtime port will be `8080`.
- The .NET runtime image's `$APP_UID` will run the application as non-root.
- `/data/zapas.db` will be writable but disposable.
- The current readiness check proves database connectivity, not schema correctness or migration completion.
- Missing JWT values stop the process before health validation.
- `EXPOSE` documents a port but does not publish it.
- Host tests do not prove a Linux image builds or starts.
- App Service configuration cannot be validated locally; local work only establishes the contract it will consume.

## 2. Exact objective alignment and coverage boundary

### C1

**Official competency:** Build, store, version, and manage container images by using Azure Container Registry.

Today teaches the complete C1 service model, but practical work proves only the local build foundation. ACR storage, tags/digests, access, inventory, lifecycle management, and Azure-side evidence are scheduled for Days 2 and 3.

### C3

**Official competency:** Deploy containers to Azure App Service, including configuring environment variables and secrets.

Today teaches the complete C3 service model, but practical work proves only the local port, configuration, health, and writable-state contract. App Service deployment and troubleshooting are scheduled for Day 4.

Neither C1 nor C3 is marked covered today. Every verb and Azure service in the official objective must later be implemented, troubleshot, and assessed.

## 3. Two-hour plan

| Track | Purpose | Time |
|---|---|---:|
| Exam concepts | Learn ACR and App Service container concepts independently of Zapas | 35 minutes |
| Exam scenarios | Practice Azure selection, configuration, and diagnosis | 20 minutes |
| Zapas implementation | Apply the relevant container-contract concepts | 50 minutes |
| Retrieval and notes | Answer generic questions and record evidence | 15 minutes |
| **Total** |  | **120 minutes** |

### Required

- Complete the independent C1/C3 concept map and scenarios.
- Make and explain the container-contract decisions.
- Add `Dockerfile` and `.dockerignore` in separate checkpoints.
- Build, inspect, run, and validate the image.
- Diagnose and repair one controlled readiness failure.
- Record observed evidence and an honest competency level.

### Optional stretch

- Run the image with a named volume and explain persistence across container replacement.
- Add a narrow automated container smoke test.
- Inspect final filesystem contents and layer sizes.

### Deferred

- ACR resource creation, push, tags/digests, Tasks, and registry lifecycle.
- App Service deployment, managed-identity pull, secrets/settings, update, rollback, and Azure logs.
- Container Apps, KEDA, AKS, Helm, Dapr, CI/CD, and IaC.
- Key Vault, App Configuration, OpenTelemetry, KQL, and durable database replacement.

## 4. Track 1 — Exam concepts independent of Zapas (35 minutes)

### 4.1 C1: ACR image lifecycle (18 minutes)

Learn these concepts before opening Zapas:

| Concept | Exam-ready model |
|---|---|
| Registry | Azure resource and private endpoint namespace that contains repositories and controls access. |
| Repository | Logical collection of image manifests with the same name. It is not a source-code repository. |
| Image | Immutable content represented by a manifest and referenced by a digest. |
| Layer | Content-addressed filesystem change reused across images when its content matches. |
| Tag | Human-friendly, mutable reference to a manifest, such as `api:1.2.3`. |
| Digest | Immutable content identifier, such as `sha256:...`; strongest artifact identity for audit and rollback. |
| Build | Produce an image locally or through ACR Tasks from a Docker build context. |
| Store | Push/import a manifest and its layers into an ACR repository. |
| Version | Use unique tags plus digests; do not rely on mutable `latest` for rollback. |
| Manage | List, inspect, lock, delete, retain, import, replicate, and control access to images according to policy. |
| Authentication | Proves caller identity: Azure identity, service principal, managed identity, or interactive CLI identity. |
| Authorization | Grants operations through appropriate Azure roles and scope; authentication alone does not permit push/pull. |

Lifecycle:

```text
source/context -> build -> local tag -> authenticate -> push/import
-> repository manifest -> immutable digest -> deploy -> inventory/retention/delete
```

Important distinctions:

- Local Docker builds and ACR Tasks can use the same Dockerfile but execute in different environments and have different logs/identity.
- A push uploads missing layers and a manifest; a tag does not copy the layer content.
- Retagging can move a mutable tag. A digest continues to identify the same manifest.
- `AcrPull` is narrower than push-capable access. Select the least privilege appropriate to the operation and the registry's permission mode.
- Registry credentials and application secrets must not be Docker build arguments, Dockerfile `ENV` values, or committed files.

Closest alternatives:

- Docker Hub or another registry can store OCI images, but C1 explicitly assesses ACR.
- `latest` is convenient for informal local use but inferior to unique tags/digests for traceability and rollback.
- A local build is useful evidence but cannot substitute for ACR storage and management evidence.

Prediction prompts:

1. If `api:prod` moves from digest A to digest B, can digest A still identify the old manifest?
2. If a managed identity authenticates successfully but has no pull role, which phase fails?
3. Does deleting one tag necessarily delete shared layers immediately?

### 4.2 C3: App Service custom containers (17 minutes)

Learn the platform model independently of the application:

```text
ACR image + App Service image selection + pull identity
        + app settings/secrets + container port + health path
        -> platform pulls -> process starts -> health succeeds -> traffic routes
```

Required concepts:

- **App Service plan:** defines compute, operating system, region, and scale boundary.
- **Web app:** deployment/configuration resource hosted by the plan.
- **Image selection:** registry, repository, tag or digest determine the artifact to pull.
- **Private registry access:** prefer a managed identity with least-privilege ACR pull access over stored registry passwords.
- **App settings:** encrypted-at-rest platform settings injected into the process as environment variables; changing them can restart the app.
- **Secrets:** remain outside source and image. App settings can carry them for the exam objective; Key Vault integration is a supporting enhancement, not a replacement for understanding App Service settings.
- **Port:** App Service must forward to the port where the process actually listens. `EXPOSE` alone does not configure every platform.
- **TLS:** App Service terminates public HTTPS; the container commonly listens on HTTP internally.
- **Health Check:** the chosen anonymous path should represent readiness and return healthy status without redirect/authentication problems.
- **Logs:** distinguish image-pull, container-startup, application, health, and connectivity evidence.
- **Update and rollback:** select a new unique artifact, restart/validate it, and restore the prior tag/digest without rebuilding.

Failure classification:

| Symptom | First hypothesis | Evidence |
|---|---|---|
| Image cannot be pulled | Registry/repository/tag, authentication, authorization, or networking | Deployment/container logs and ACR role/image inventory |
| Container exits | Entry point or required runtime configuration | Container startup/application logs |
| Process runs but platform cannot connect | Listening-port mismatch | App setting, platform logs, and process listening log |
| Health fails but basic process is alive | Dependency/readiness, redirect, auth, or wrong path | Direct path response and application health logs |
| Old behavior after update | Mutable tag, cached selection, or wrong artifact | Configured tag/digest and ACR manifest inventory |

Closest alternatives:

- Code deployment is simpler for some applications but does not exercise the custom-container C3 objective.
- Container Apps provides revisions and event-driven scaling, but C3 explicitly names App Service; it cannot substitute for this objective.
- Baking settings into the image appears easy but destroys artifact portability and exposes values in image metadata/layers.

## 5. Track 2 — Generic Azure scenarios (20 minutes)

Answer before reading the discussion prompts. These do not require Zapas.

### Scenario 1: rollback identity

A team publishes `orders:latest` twice. Production fails after the second push, and no unique tag was retained.

Questions:

- Which identifier would have made exact rollback reliable?
- Why is `latest` insufficient?
- What ACR inventory should the team record for every release?

Expected reasoning: use a unique version tag correlated with the immutable digest; record repository, tag, digest, build/source, and deployment.

### Scenario 2: private pull failure

An App Service managed identity exists, but startup logs report unauthorized access to a private ACR repository.

Questions:

- Is identity existence proof of authorization?
- Which scope and permission direction are required?
- Which surfaces distinguish a wrong image name from missing pull access?

Expected reasoning: identity and role are separate; grant least-privilege pull access at an appropriate scope and compare App Service pull logs with ACR repository/tag inventory and role assignments.

### Scenario 3: wrong port

The container log says the web server listens on port 8080. App Service repeatedly reports that the container did not respond on the expected port.

Questions:

- Which contract is inconsistent?
- Does `EXPOSE 8080` by itself guarantee platform routing?
- What should be corrected and revalidated?

Expected reasoning: align the platform's container-port setting with the process listener, restart, and revalidate startup, health, and connectivity.

### Scenario 4: secret handling

A Dockerfile contains `ENV API_KEY=...`. A later layer deletes the key and App Service supplies a replacement setting.

Questions:

- Is the original secret safely removed?
- Where should the runtime value live?
- What remediation is required for the exposed value?

Expected reasoning: layers/history can retain it; rotate the secret, remove it from source/build inputs, rebuild cleanly, and inject the replacement through runtime configuration.

### Scenario 5: health semantics

A web process accepts TCP connections, but its database is unavailable. The configured Health Check uses a process-only liveness path.

Questions:

- What false signal can the platform receive?
- When should readiness include the dependency?
- Why must liveness usually avoid transient dependencies?

Expected reasoning: traffic may reach an unusable instance; readiness should represent ability to serve, while liveness should avoid causing restart loops for recoverable dependency outages.

## 6. Track 3 — Zapas implementation (50 minutes)

Work in order. Stop after every checkpoint for review.

### Checkpoint 0: revalidate baseline (5 minutes)

Why: keep host defects, build defects, startup defects, and platform defects separate.

```powershell
git status --short --untracked-files=all
dotnet test .\Zapas.slnx --nologo --verbosity minimal
docker version
docker info --format '{{.OSType}}'
Get-ChildItem -Force -Name Dockerfile,.dockerignore -ErrorAction SilentlyContinue
```

Expected:

- All 14 tests pass.
- Docker client/server respond and report Linux.
- Expected untracked files are understood.
- No container files exist.

Predict before continuing:

- Name one failure that can occur during image build but not container startup.
- If live is 200 and ready is 503, which failure class is likely?

### Checkpoint 1: decide the contract (5 minutes)

Explain and record:

| Decision | Choice | Why |
|---|---|---|
| Build context | Repository root | Explicit project path and reusable context for later ACR Tasks |
| Build strategy | .NET 10 multi-stage publish | Keep SDK/build tools out of final image |
| Listener | HTTP 8080 | Clear internal platform contract |
| TLS | Future hosting platform terminates TLS | No development certificate in image |
| User | `$APP_UID` | Non-root least privilege |
| Writable state | `/data/zapas.db` | Explicit disposable lab state outside binaries |
| Settings | Runtime environment variables | Same image can move across environments |
| Health | Preserve live/ready split | Separate process from dependency readiness |

Stop and explain why an image is immutable while a container's writable layer is not.

### Checkpoint 2: create `Dockerfile` (10 minutes)

Why: create a reproducible runtime artifact with an explicit port, user, and writable path.

File: repository-root `Dockerfile`.

Type this yourself:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Zapas.Api/Zapas.Api.csproj", "Zapas.Api/"]
RUN dotnet restore "Zapas.Api/Zapas.Api.csproj"

COPY . .
WORKDIR /src/Zapas.Api
RUN dotnet publish "Zapas.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

RUN mkdir -p /data && chown "$APP_UID:$APP_UID" /data
USER $APP_UID

COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .
ENTRYPOINT ["dotnet", "Zapas.Api.dll"]
```

Review:

```powershell
Get-Content -Raw .\Dockerfile
git status --short -- Dockerfile
```

Expected: SDK is used only for build; the final stage contains published output, runs non-root, listens on 8080, and can write `/data`.

Common failures:

- Project not found: run from repository root and check `COPY` paths.
- Publish lacks restored assets: inspect the restore step and `--no-restore` ordering.
- `$APP_UID` is invalid: inspect the selected base image with `docker run --rm --entrypoint /bin/sh mcr.microsoft.com/dotnet/aspnet:10.0 -c 'echo $APP_UID'`.
- `/data` is not writable: inspect `id` and `ls -ld /data` inside the built image.

Stop and explain why copying the project file before the rest of the source improves restore-layer reuse.

### Checkpoint 3: create `.dockerignore` (7 minutes)

Why: Git ignore rules do not bound the Docker build context. Prevent local state, secrets, repository history, tests, and build artifacts from being available to `COPY`.

File: repository-root `.dockerignore`.

```dockerignore
**
!Zapas.Api/
!Zapas.Api/**

Zapas.Api/bin/
Zapas.Api/obj/
Zapas.Api/TestData/
Zapas.Api/*.db
Zapas.Api/*.db-shm
Zapas.Api/*.db-wal
Zapas.Api/appsettings.Local.json
Zapas.Api/appsettings.*.Local.json
Zapas.Api/secrets.json
Zapas.Api/.env
Zapas.Api/.env.*
Zapas.Api/*.pfx
Zapas.Api/*.p12
Zapas.Api/*.pem
Zapas.Api/*.key
```

Validate without displaying any secret contents:

```powershell
Get-Content -Raw .\.dockerignore
git status --short -- Dockerfile .dockerignore
Get-ChildItem -Recurse -File Zapas.Api |
    Where-Object { $_.Name -match '\.(db|db-shm|db-wal|pfx|p12|pem|key)$' -or $_.Name -match '^\.env' } |
    Select-Object FullName
```

Expected: only API source is admitted, then unsafe/unnecessary files are excluded from that subtree.

Common failure: restore says the project is missing. Check rule ordering; later matching rules can override earlier ones.

Stop and explain the different threat boundaries of `.gitignore` and `.dockerignore`.

### Checkpoint 4: build, inspect, and run (13 minutes)

Build:

```powershell
docker build --pull --progress=plain --tag zapas-api:day1 .
docker image inspect zapas-api:day1 --format 'Id={{.Id}} User={{.Config.User}} Ports={{json .Config.ExposedPorts}}'
docker history --no-trunc zapas-api:day1
docker run --rm --entrypoint /bin/sh zapas-api:day1 -c 'id; test -w /data; dotnet --list-runtimes'
```

Expected:

- Build succeeds.
- User is non-root.
- Port 8080 is declared.
- `/data` is writable.
- Runtime is present; SDK/build stage is absent.
- History does not reveal a JWT value, connection string, password, or token.

Run with non-secret placeholder identity settings:

```powershell
docker run --detach --rm --name zapas-day1 --publish 8080:8080 --env ASPNETCORE_ENVIRONMENT=Production --env Jwt__Authority=https://identity.test.example/ --env Jwt__Audience=zapas-api-day1 --env "ConnectionStrings__ZapasDb=Data Source=/data/zapas.db" zapas-api:day1
docker ps --filter name=zapas-day1
docker logs zapas-day1
curl.exe --silent --show-error --include http://localhost:8080/health/live
curl.exe --silent --show-error --include http://localhost:8080/health/ready
```

Expected: container remains running and both health paths return HTTP 200. A local HTTPS-redirection warning may appear because public TLS termination is not configured in this local HTTP lab; record rather than conceal it.

Troubleshooting:

- Exited container: use `docker ps --all` and `docker logs`; inspect required JWT/connection settings and entry point.
- Connection refused: compare `docker port`, listener logs, and requested host port.
- Live 200, ready 503: inspect SQLite path/permissions and health-check logs.
- Both paths 404: verify paths and rebuild from current source.

### Checkpoint 5: controlled failure and repair (10 minutes)

Objective: prove runtime configuration, non-root storage, and liveness/readiness diagnosis.

Predict first:

- Will the process remain alive if SQLite cannot open its path?
- What will live return?
- What will ready return?
- Which evidence will reveal the cause?

Introduce an unwritable path:

```powershell
docker stop zapas-day1
docker run --detach --rm --name zapas-day1-failure --publish 8080:8080 --env ASPNETCORE_ENVIRONMENT=Production --env Jwt__Authority=https://identity.test.example/ --env Jwt__Audience=zapas-api-day1 --env "ConnectionStrings__ZapasDb=Data Source=/forbidden/zapas.db" zapas-api:day1
curl.exe --silent --show-error --include http://localhost:8080/health/live
curl.exe --silent --show-error --include http://localhost:8080/health/ready
docker logs zapas-day1-failure
docker exec zapas-day1-failure /bin/sh -c 'id; ls -ld / /data /forbidden 2>&1'
```

Expected:

- Process can stay alive.
- Live returns 200.
- Ready returns 503.
- Logs show SQLite cannot open/create the configured database.
- Non-root user can write `/data` but cannot create `/forbidden`.

If the process exits, use `docker ps --all` and logs, classify it as startup failure, and explain why the result differed from the prediction.

Repair:

```powershell
docker stop zapas-day1-failure
docker run --detach --rm --name zapas-day1 --publish 8080:8080 --env ASPNETCORE_ENVIRONMENT=Production --env Jwt__Authority=https://identity.test.example/ --env Jwt__Audience=zapas-api-day1 --env "ConnectionStrings__ZapasDb=Data Source=/data/zapas.db" zapas-api:day1
curl.exe --silent --show-error --include http://localhost:8080/health/live
curl.exe --silent --show-error --include http://localhost:8080/health/ready
docker logs zapas-day1
docker stop zapas-day1
```

Expected: both paths return 200 after repair, and `--rm` removes the stopped lab container.

## 7. Track 4 — Retrieval and notes (15 minutes)

### Generic retrieval (7 minutes)

Without using Zapas, explain:

1. Registry, repository, image, layer, tag, and digest.
2. Authentication versus authorization for private ACR pull.
3. Why unique tags and digests are stronger than `latest`.
4. App Service plan versus web app.
5. Image pull failure versus startup failure versus port failure versus readiness failure.
6. How App Service settings reach a container.
7. Why public HTTPS does not require Kestrel to terminate TLS inside the container.

### Five short assessment questions (5 minutes)

Four of five are generic Azure scenarios.

1. **Generic:** Which immutable identifier should be recorded to prove the exact ACR manifest deployed?
2. **Generic:** An App Service identity authenticates to ACR but cannot download layers. What category of configuration is missing?
3. **Generic:** A process listens on 8080 but the platform forwards to 80. Which evidence and setting should be checked first?
4. **Generic:** Why does deleting a secret in a later Docker layer not reliably remove its exposure?
5. **Zapas:** Live is 200 while ready is 503 with `/forbidden/zapas.db`. What does each result prove?

Do not award objective coverage from these five questions alone. C1/C3 need later Azure implementation and multiple scenario formats.

### Evidence and documentation (3 minutes)

Run:

```powershell
dotnet test .\Zapas.slnx --nologo --verbosity minimal
docker build --tag zapas-api:day1 .
docker image inspect zapas-api:day1 --format 'Id={{.Id}} Created={{.Created}} User={{.Config.User}} Ports={{json .Config.ExposedPorts}}'
git status --short --untracked-files=all
Get-Content -Raw .\Dockerfile
Get-Content -Raw .\.dockerignore
```

Record observed results only.

## 8. Completion gate and handoff

### Completed

- [ ] Independent C1/C3 service model explained.
- [ ] Five scenarios answered, including four generic Azure scenarios.
- [ ] `Dockerfile` created and reviewed.
- [ ] `.dockerignore` created and reviewed.
- [ ] Image built and inspected.
- [ ] Valid run demonstrated.
- [ ] Controlled failure diagnosed and repaired.

### Validated

- Host tests:
- Image tag and ID:
- Runtime user:
- Container port:
- Valid live result:
- Valid ready result:
- Controlled-failure live result:
- Controlled-failure ready result:
- Diagnostic evidence:

### Unfinished

- ACR build/store/version/manage practice:
- App Service deployment/settings/secrets practice:
- Any failed or skipped local validation:

### Files changed

- `Dockerfile`
- `.dockerignore`
- `residency/week01_day01.md`
- Other learning records actually changed:

### Commands worth remembering

```text
docker build --tag zapas-api:day1 .
docker run --detach --rm --publish 8080:8080 ...
docker logs <container>
docker image inspect <image>
```

### Reproduce from memory

Reproduce the runtime port mapping and the three environment variables required to start the Zapas container.

### Competency recommendation

- **C1:** Level 1 after accurate service-model recall. Level 2 after independently building and explaining the local image. Do not assign Level 3 until ACR build, storage, versioning, management, and troubleshooting evidence exists.
- **C3:** Level 1 after accurate App Service model recall. Local contract diagnosis is supporting evidence only. Do not assign Level 2/3 from local Docker alone; actual App Service deployment, settings/secrets, validation, and failure diagnosis are still required.

### Suggested exam error log entry

> Mistake: I treated `EXPOSE 8080` as host/platform port publication.  
> Correction: `EXPOSE` is image metadata. Docker `--publish` or the hosting platform's container-port configuration establishes routing to the process listener.

### Suggested engineering wisdom entry

> Prove one immutable image locally before adding registry and hosting variables. Keep environment values outside image layers, use a non-root process, make writable state explicit, and separate liveness from readiness.

### Suggested ADR input

> Candidate decision: root-context multi-stage .NET 10 image; non-root `$APP_UID`; internal HTTP 8080; runtime environment settings; `/data/zapas.db` only for disposable Week 1 evidence. Final image-versioning and deployment-selection policy remains open until ACR/App Service evidence is complete.

### Suggested competency matrix entry

> C1 introduced: service model plus local build evidence; ACR verbs still unproven. C3 introduced: App Service model plus local runtime-contract evidence; Azure deployment and settings/secrets still unproven. Link image ID, health results, controlled-failure logs, and missed generic scenarios.

