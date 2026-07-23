# Week 1, Day 4: Build the Delivery Pipeline

## Outcome

Every change is built and tested consistently, and an approved revision can be deployed to the Azure development environment using short-lived federated identity rather than a stored Azure password.

## Problem

Yesterday’s manual deployment taught the mechanics, but a reliable team cannot depend on one workstation’s SDK, login session, or unpublished steps. Automation must preserve the same build artifact, make failures visible, prevent untested deployment, and provide a credible rollback path.

## Design

### 1. Separate CI from CD

Recommended responsibilities:

**CI (`ci.yml`)**

- Trigger on pull requests and pushes to the primary branch.
- Restore with the repository’s declared dependencies.
- Build Release with warnings visible.
- Run all tests.
- Evaluate dependency vulnerabilities.
- Publish the API artifact.
- Upload the artifact with bounded retention.

**CD (`deploy-dev.yml`)**

- Trigger after accepted changes or by an explicit manual dispatch.
- Request only the GitHub permissions needed for OIDC and repository checkout/artifacts.
- Authenticate to Azure through workload identity federation.
- Run Bicep what-if or validate before mutation.
- Provision/update the development environment.
- Deploy the exact CI artifact or rebuild once in a clearly documented model.
- Run smoke tests.
- Record the revision, workflow run, and deployment result.

Prefer “build once, deploy the same artifact.” If GitHub workflow constraints lead to a rebuild in Week 1, record that limitation and its supply-chain implication.

### 2. Use federated identity

The GitHub workflow should exchange its GitHub OIDC token for a short-lived Azure token. Do not store:

- A publish profile.
- A client secret.
- A user password.
- An Azure CLI token.

Store identifiers such as client ID, tenant ID, and subscription ID as repository or environment variables according to team policy; they identify resources but are not credentials by themselves. Restrict the Azure role assignment to the development resource group when possible.

Use a protected GitHub `development` environment if available. Scope its deployment identity and approval rules to the risk of this project.

### 3. Define pipeline gates

A deployment must not proceed when:

- Restore, build, or tests fail.
- A new vulnerability exceeds the team’s documented severity policy.
- Infrastructure validation fails.
- The artifact is missing.

A deployment is unsuccessful when:

- The platform reports rollout failure.
- Liveness does not return `200` within the retry budget.
- Readiness does not return `200` within the retry budget.
- The expected deployed version cannot be identified.

### 4. Define rollback before deployment

For a development App Service without deployment slots, the minimum rollback is redeploying the last known-good immutable artifact. If the chosen plan supports slots and the added cost is justified, document slot swap behavior separately; do not assume it exists.

Rollback does not restore a damaged SQLite file. Code rollback and data recovery are different problems.

## Build

### Task 1: Create CI

Add `.github/workflows/ci.yml`. It should:

- Pin the intended .NET SDK feature band or use a repository `global.json`.
- Cache only safe, reproducible dependency data.
- Use `dotnet restore`, then `--no-restore` for later build steps.
- Run the entire solution test suite in Release.
- Publish only `Zapas.Api`.
- Give the artifact an immutable identity tied to the commit SHA.
- Avoid printing environment values.

Decide how vulnerability policy behaves. A practical Week 1 rule is:

- Existing assessed advisories are recorded with expiry.
- New high/critical advisories fail CI.
- The baseline cannot become a permanent exemption.

If the tooling cannot compare to a baseline cleanly, fail on all high/critical warnings and remediate them before CD.

### Task 2: Configure Azure federation

Create a dedicated deployment identity with:

- A federated credential restricted to the repository and selected GitHub environment or branch.
- The minimum practical Azure role at development resource-group scope.
- No reusable client secret.

Document resource names and scopes, not credential material. Have a second engineer review any broad role such as Owner or Contributor at subscription scope.

### Task 3: Create CD

Add `.github/workflows/deploy-dev.yml`. Include:

- `permissions: id-token: write` and `contents: read`, plus only other permissions actually used.
- Azure login with OIDC.
- Bicep validation/what-if.
- Resource deployment.
- Artifact deployment.
- A concurrency group so two development deployments do not race.
- An environment URL if it can be derived safely.
- Smoke-test execution.

Pin third-party actions to immutable commit SHAs according to the repository’s supply-chain policy. If using version tags temporarily, record the decision and review date.

### Task 4: Add a smoke-test script

Create `scripts/smoke-test.ps1` with:

- A required base URL parameter.
- A bounded retry count and delay.
- HTTPS validation.
- `/health/live` and `/health/ready` assertions.
- Clear exit codes.
- No authentication token requirement for public health endpoints.
- No secret output.

Optionally add an authenticated GET smoke test only if the CI environment can obtain a short-lived test token safely. Never commit a bearer token.

### Task 5: Add rollback and failure drills

Exercise at least one controlled failure:

- Deploy an invalid non-secret database path/setting and observe liveness versus readiness; then restore it through Bicep.
- Or deploy a harmless revision whose startup intentionally fails in the development environment, then redeploy the known-good artifact.

Do not perform a destructive data test against data anyone cares about.

Document:

- Detection signal.
- Person or workflow that decides to roll back.
- Exact known-good artifact identity.
- Rollback command/workflow.
- Verification after rollback.

## Verify

- A pull request or equivalent branch run executes CI without Azure permissions.
- CI produces one identifiable Release artifact.
- CD cannot run before CI succeeds.
- GitHub has no Azure client secret or publish profile for this path.
- The Azure role is scoped to development resources.
- Two simultaneous CD attempts cannot interleave.
- Smoke tests fail the workflow when readiness is unavailable.
- A known-good artifact can be redeployed.

## Explain

Explain:

- Why CI should not need Azure credentials.
- How OIDC federation differs from a stored client secret.
- Why “build once” matters.
- Why health checks need retries but also a time limit.
- Why successful artifact upload is not successful deployment.
- Why application rollback does not solve database rollback.

## Defend

Tech Lead challenges:

- “A publish profile is easier. What risk does OIDC remove?”
- “Why not deploy every pull request to the shared dev app?”
- “Why pin actions?”
- “What prevents two commits from deploying out of order?”
- “How do you prove which commit is running?”
- “What happens when smoke testing succeeds but authenticated endpoints fail?”

## Reflect

Record:

- The trust boundaries in the pipeline.
- The broadest permission still present and why.
- A failure the pipeline catches that local tests do not.
- One improvement needed before a production pipeline.

## Definition of done

- [ ] CI restores, builds, tests, scans, and publishes.
- [ ] CD uses OIDC and a resource-scoped identity.
- [ ] The deployed artifact is traceable to a commit.
- [ ] Infrastructure and app deployment failures are distinguishable.
- [ ] Smoke tests are bounded and automated.
- [ ] Rollback has been executed, not merely described.

## Optional stretch

Generate a software bill of materials for the published artifact and retain it beside the CI artifact. Explain what it helps answer during an incident and what it does not guarantee.

