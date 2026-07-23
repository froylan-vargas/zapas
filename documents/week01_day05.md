# Week 1, Day 5: Deploy, Operate, and Defend

## Outcome

Complete the first Azure deployment, collect operational evidence, update the architecture to reality, and defend every Week 1 decision in a Tech Lead Review.

## Problem

A green pipeline and a responding URL are necessary but insufficient. The team needs to know whether the correct revision is running, whether dependency failure is visible, where logs go, what the deployment costs, how it rolls back, and which current shortcuts prevent production use.

Today turns implementation into engineering judgment.

## Design

### 1. Define the demonstration

The demonstration should tell one coherent story:

1. A change enters CI.
2. CI proves the Release artifact meets the current gates.
3. CD authenticates without a stored Azure secret.
4. Bicep converges the development environment.
5. The exact artifact is deployed.
6. Liveness and readiness become healthy.
7. A normal request produces structured logs.
8. A dependency problem changes readiness without falsely reporting process death.
9. The previous artifact can be restored.
10. The environment can be removed safely.

### 2. Define success and failure evidence

Record UTC timestamps and immutable identifiers. Prefer command output, workflow links, and short Markdown tables over screenshots.

Do not place JWTs, CLI tokens, connection strings, workflow secrets, or user data in the review record.

### 3. Update the architecture

The final Week 1 diagram must show what exists, including:

- Client and trust boundary.
- Auth0 as the token issuer/validation authority.
- GitHub Actions and its federated trust to Azure.
- Azure subscription/resource-group boundary.
- App Service plan and Web App.
- Platform settings.
- In-process cache.
- Temporary SQLite file.
- Log destination used during the exercise.

Use solid lines for runtime traffic and dashed lines for deployment/control-plane flow. Add notes for single-instance and disposable-data constraints.

## Build

### Task 1: Run the deployment from automation

Deploy a commit that passed CI. Capture:

- Commit SHA.
- CI run and artifact identity.
- CD run.
- Infrastructure deployment name.
- Web App name and safe URL.
- Deployment start/end time.

Confirm the pipeline did not depend on a developer’s active Azure CLI session.

### Task 2: Run the smoke and behavior tests

At minimum:

```powershell
./scripts/smoke-test.ps1 -BaseUrl "https://<web-app-hostname>"
```

Also verify:

- Plain HTTP redirects or is rejected according to the HTTPS policy.
- `/health/live` returns `200`.
- `/health/ready` returns `200` with valid runtime configuration.
- An unauthenticated `/sessions` request returns `401`.
- A token without the required upload role cannot upload.
- A valid request produces a structured request log.
- Swagger is not exposed unintentionally in the production environment.

Use synthetic or disposable data only. Do not upload personal activity data for a pipeline demonstration.

### Task 3: Inspect operational behavior

Use the platform log stream and resource metrics available on the selected tier. Answer:

- How long did startup take?
- What was the health-check latency?
- Can logs tie a failure to a route and status code?
- Can you identify the deployed version?
- What happens after an application restart?
- What happens to the SQLite data after replacement/redeployment?
- Does the app remain one instance?

Do not infer persistence safety from one successful restart. Platform replacement and scale-out are the relevant failure modes.

### Task 4: Execute the failure drill

Use the safe drill designed on Day 4. Demonstrate:

- Liveness remains accurate.
- Readiness blocks or reports the unavailable dependency.
- Logs provide enough context without leaking configuration.
- The pipeline detects the failed smoke test.
- Restoring configuration or the known-good artifact returns readiness to `200`.

Record time to detection and time to recovery.

### Task 5: Finalize Week 1 documents

Update:

- `documents/architecture/week01-azure-development.md`
- `documents/architecture_evolution.md`
- `documents/decision_log.md`
- `documents/engineering_wisdom.md`
- `documents/tech_lead_reviews.md`
- `documents/adr/0001-host-zapas-on-azure-app-service.md`

The architecture evolution entry should distinguish:

- Before: local process, development JSON, local SQLite, manual build.
- After: managed Azure compute, external runtime configuration, health semantics, IaC, CI/CD, still-temporary SQLite.
- Deferred: durable storage, managed secrets, distributed cache/rate limiting, full observability, security remediation, production topology.

### Task 6: Review cost and teardown

List every resource in the resource group and identify which accrue cost while idle.

If the environment is not needed after the review, remove the exact dedicated resource group through the documented command. Resolve and display the target subscription and resource group before deletion. Never delete a shared or ambiguously named group.

If retaining the environment:

- Record owner and expiry/review date.
- Confirm the smallest justified SKU and instance count.
- Document how to stop or delete it.
- Set an external reminder or budget alert if available.

## Friday Tech Lead Review

### Demo agenda

1. Two-minute business and architecture context.
2. Five-minute pipeline and live deployment.
3. Five-minute health/failure drill.
4. Five-minute ADR and alternatives.
5. Five-minute risk, cost, and next-week handoff.
6. Ten-minute challenge questions.

### Challenge questions

1. Why is this environment cloud-ready but not production-ready?
2. Where can data be lost today?
3. Why is the application deliberately limited to one instance?
4. What protects Azure when GitHub Actions is compromised?
5. Which configuration should eventually move to Key Vault, and why not all configuration?
6. What is the blast radius of the deployment identity?
7. What signal detects a broken database, and what signal detects a dead process?
8. What is the recovery path if the newest binary is bad?
9. Which dependency advisory remains and who owns it?
10. What must Week 2 change before durable user data is accepted?

### Required review record

In `tech_lead_reviews.md`, capture:

- Date and reviewers.
- Commit and deployed environment.
- Score from the Week 1 scorecard.
- Decisions accepted or challenged.
- Demonstrated failure and recovery.
- Open risks with owners and target weeks.
- Three concrete Week 2 entry criteria.

## Verify

- The deployed URL and revision are traceable.
- HTTPS, authentication boundary, liveness, and readiness behave as designed.
- Logs are useful and contain no obvious credential or personal-data leakage.
- The failure drill is detected and recovered.
- IaC matches the actual resource inventory.
- The environment is either safely deleted or has an owner and expiry.
- All diagrams and ADRs describe the final state rather than the plan from Day 1.

## Explain

Give a ten-minute architecture narrative without reading service definitions:

- Start from the user’s upload need.
- Follow the runtime request and deployment flows.
- Explain each Azure capability by the problem it solves.
- State current failure modes and blast radii.
- End with why durable data is the next engineering question.

## Defend

A senior-level defense does not claim certainty. It says:

- What was observed.
- What was assumed.
- Why the current choice is proportional.
- What negative consequences were accepted.
- What measurable trigger will change the choice.

If a challenge exposes a missing fact, add an experiment or backlog item instead of improvising confidence.

## Reflect

Write the final Week 1 reflection:

- The decision that changed most during the week.
- The strongest piece of evidence gathered.
- A cloud concept now understood through Zapas rather than memorization.
- The riskiest current shortcut.
- What you would do differently in a second deployment.
- The Week 2 question you are now prepared to answer.

## Definition of done

- [ ] An accepted commit reached Azure through the automated path.
- [ ] The deployed behavior and version are verified.
- [ ] A safe failure and recovery were demonstrated.
- [ ] The architecture diagram matches reality.
- [ ] The hosting ADR is defensible and has revisit triggers.
- [ ] Risks, costs, and deferred work have owners.
- [ ] The environment was safely deleted or assigned an expiry.
- [ ] The Friday review is recorded.

## Week 2 handoff

Week 2 begins with this question:

> How should Zapas store user-owned sessions durably, migrate schema safely, recover from failure, and support more than one API instance?

Bring forward the SQLite limitation, migration ownership, backup/recovery objectives, connection management, session ownership query, and expected data volume. Do not select an Azure database until those requirements are explicit.
