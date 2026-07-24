# Week 1, Day 2 Build Guide: Define the Runtime Contract

This guide expands the five Build tasks in
`documents/week01_day02.md`. Complete the tasks in order because the health
tests depend on the runtime configuration and health-check registration created
in the earlier tasks.

The goal is not merely to make the endpoints return the expected status codes.
By the end, the code and the ADR should explain what Zapas needs to start, what
it needs to receive traffic, and why App Service is the smallest justified
hosting step.

## Before you start

From the repository root, establish a baseline:

```powershell
git status --short
dotnet build Zapas.slnx
dotnet test Zapas.slnx --no-build
```

Do not discard unrelated working-tree changes. Record the current test count so
you can distinguish a regression from a pre-existing failure.

The main files used in this exercise are:

- `Zapas.Api/Program.cs`
- `Zapas.Api/Options/UploadOptions.cs`
- `Zapas.Api/appsettings.json`
- `Zapas.Api/appsettings.Development.json`
- `Zapas.Api.Tests/Infrastructure/ZapasApiFactory.cs`
- A new focused test file under `Zapas.Api.Tests/Health/`
- A new architecture note at
  `documents/architecture/week01-azure-development.md`
- A new ADR at `learnings/adr/0001-host-zapas-on-azure-app-service.md`

Create the missing `documents/architecture` and `learnings/adr` directories
when you reach the related step.

## Task 1: Add typed runtime options

### Step 1. Inventory the configuration reads

Inspect `Program.cs` and `UploadOptions.cs`. Classify each value before changing
code:

| Setting | Current access | Recommended treatment |
| --- | --- | --- |
| `ConnectionStrings:ZapasDb` | `GetConnectionString` | Require a nonblank value before registering EF Core; transient connectivity remains a readiness concern |
| `Jwt:Authority` | String indexer | Bind to typed options and validate at startup |
| `Jwt:Audience` | String indexer | Bind with the JWT options and validate at startup |
| `Cors:AllowedOrigins` | `GetSection().Get<string[]>()` | Bind to typed options; allow an empty list until a browser client is required |
| `Uploads:MaxFitFileSizeBytes` | `UploadOptions` | Keep typed; add validation |
| `Uploads:AllowedExtensions` | `UploadOptions` | Keep typed; add validation |

The distinction to preserve is:

- A missing connection-string setting is a configuration defect and should stop
  startup.
- A configured database that is temporarily unreachable is an operational
  dependency failure and should make readiness return `503`.
- Missing or structurally invalid JWT and upload settings make the deployed API
  unusable and should fail option validation during startup.

### Step 2. Add the option classes

Add `Zapas.Api/Options/JwtOptions.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Zapas.Api.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Authority { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;
}
```

Add `Zapas.Api/Options/CorsOptions.cs`:

```csharp
namespace Zapas.Api.Options;

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}
```

`UploadOptions` already supplies safe defaults. Add validation during
registration rather than duplicating the rules inside every consumer.

### Step 3. Bind and validate the options

Replace the unvalidated upload registration in `Program.cs` with the options
builder pattern:

```csharp
builder.Services
    .AddOptions<UploadOptions>()
    .BindConfiguration(UploadOptions.SectionName)
    .Validate(
        options => options.MaxFitFileSizeBytes > 0,
        "Uploads:MaxFitFileSizeBytes must be greater than zero.")
    .Validate(
        options => options.AllowedExtensions.Length > 0 &&
                   options.AllowedExtensions.All(extension =>
                       extension.StartsWith('.') &&
                       !string.IsNullOrWhiteSpace(extension)),
        "Uploads:AllowedExtensions must contain at least one dot-prefixed extension.")
    .ValidateOnStart();

builder.Services
    .AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .Validate(
        options => Uri.TryCreate(options.Authority, UriKind.Absolute, out var uri) &&
                   uri.Scheme == Uri.UriSchemeHttps,
        "Jwt:Authority must be an absolute HTTPS URI.")
    .ValidateOnStart();

builder.Services
    .AddOptions<CorsOptions>()
    .BindConfiguration(CorsOptions.SectionName)
    .Validate(
        options => options.AllowedOrigins.All(origin =>
            Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttps ||
             uri.Host is "localhost" or "127.0.0.1")),
        "CORS origins must be absolute HTTPS URIs; HTTP is allowed only for local development.")
    .ValidateOnStart();
```

Then obtain the bound values where framework configuration needs concrete
values:

```csharp
var jwtOptions = builder.Configuration
    .GetRequiredSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException("The Jwt section is required.");

var corsOptions = builder.Configuration
    .GetSection(CorsOptions.SectionName)
    .Get<CorsOptions>() ?? new CorsOptions();
```

Use `jwtOptions.Authority`, `jwtOptions.Audience`, and
`corsOptions.AllowedOrigins` in the existing authentication and CORS
registrations.

Before `AddDbContext`, require the connection-string key without attempting a
network or database connection:

```csharp
var zapasDbConnectionString =
    builder.Configuration.GetConnectionString("ZapasDb");

if (string.IsNullOrWhiteSpace(zapasDbConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:ZapasDb is required.");
}
```

Pass `zapasDbConnectionString` to `UseSqlite`.

### Step 4. Put safe defaults in the right layer

Use `appsettings.json` only for non-secret, environment-independent defaults:

```json
{
  "Uploads": {
    "MaxFitFileSizeBytes": 3145728,
    "AllowedExtensions": [".fit"]
  }
}
```

Place local-only values in `appsettings.Development.json`, user secrets, or
task-scoped environment variables. Production deployment values belong in App
Service settings. For example:

```powershell
$env:ConnectionStrings__ZapasDb = "Data Source=zapas-local.db"
$env:Jwt__Authority = "https://example.invalid/"
$env:Jwt__Audience = "zapas-api"
$env:Cors__AllowedOrigins__0 = "https://localhost.example"
```

Do not commit a real token, credential, production URL, or production
connection string.

### Step 5. Document the contract

Create `documents/architecture/week01-azure-development.md` and add the
configuration table from the Day 2 Design section. Add two clarifications:

1. Name the settings that stop startup and the validation rule for each.
2. State that a configured but unreachable SQLite database affects readiness,
   not liveness.

### Task 1 checkpoint

Run the API once with all required settings, then omit one required JWT setting
and confirm startup fails with a setting name but no secret value:

```powershell
dotnet run --project Zapas.Api/Zapas.Api.csproj
```

Restore the setting before continuing.

## Task 2: Implement liveness and readiness

### Step 1. Tag the database health check

In `Program.cs`, give the EF Core check a readiness tag:

```csharp
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ZapasDbContext>(
        name: "zapas-database",
        tags: ["ready"]);
```

The liveness endpoint does not need a custom check. If ASP.NET Core can execute
the health endpoint and the predicate selects no dependency checks, the process
is live.

### Step 2. Map two filtered endpoints

Replace `app.MapHealthChecks("/health")` with:

```csharp
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
});
```

Add the required namespace:

```csharp
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
```

Keep the default response writer for now. Its small status text avoids
serializing exception details, connection strings, and internal check data.
Do not return `HealthReportEntry.Exception` from a public response.

Decide explicitly whether to remove the old `/health` route or retain it as a
temporary compatibility alias. This exercise assumes it is removed because no
existing deployment probe contract has been documented.

### Step 3. Check middleware and authorization behavior

Health endpoints should be reachable by the platform without a JWT. Mapping
them outside the controller authorization policies currently achieves that.
Do not add database work to `/health/live`.

### Task 2 checkpoint

Start the application with valid local settings and probe both endpoints:

```powershell
Invoke-WebRequest https://localhost:<port>/health/live `
    -SkipCertificateCheck

Invoke-WebRequest https://localhost:<port>/health/ready `
    -SkipCertificateCheck
```

Confirm both return `200`. Do not treat this manual check as a replacement for
the integration tests.

## Task 3: Add integration tests

### Step 1. Add the healthy-path tests

Create `Zapas.Api.Tests/Health/HealthEndpointTests.cs`. Reuse
`ZapasApiFactory`, whose open in-memory SQLite connection makes the database
available for the lifetime of the test server:

```csharp
using System.Net;
using FluentAssertions;
using Zapas.Api.Tests.Infrastructure;

namespace Zapas.Api.Tests.Health;

public sealed class HealthEndpointTests
{
    [Fact]
    public async Task Live_returns_ok()
    {
        using var factory = new ZapasApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_returns_ok_when_database_is_available()
    {
        using var factory = new ZapasApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

If startup validation now requires settings that the test project does not
have, add deterministic fake values through `builder.UseSetting(...)` in
`ZapasApiFactory`. Do not weaken production validation to make tests pass.

### Step 2. Create an unavailable-database factory

Make `ZapasApiFactory` inheritable by removing `sealed`, or introduce a second
focused `WebApplicationFactory<Program>`. In the unavailable variant, replace
the production `ZapasDbContext` registration just as the healthy factory does,
but use a SQLite file in a directory that does not exist and set
`Mode=ReadWrite`. This prevents SQLite from silently creating a valid database:

```csharp
var missingDirectory = Path.Combine(
    Path.GetTempPath(),
    $"zapas-health-{Guid.NewGuid():N}");
var unavailableDatabase = Path.Combine(missingDirectory, "zapas.db");
var connectionString =
    $"Data Source={unavailableDatabase};Mode=ReadWrite";

services.RemoveAll<DbContextOptions<ZapasDbContext>>();
services.RemoveAll<IDbContextOptionsConfiguration<ZapasDbContext>>();
services.AddDbContext<ZapasDbContext>(options =>
    options.UseSqlite(connectionString));
```

Do not call `EnsureCreated` in this factory. The test must not depend on Azure,
DNS, a fixed drive letter, timing, or an already occupied TCP port.

### Step 3. Prove the health contracts independently

Add a test that uses the unavailable factory:

```csharp
[Fact]
public async Task Database_failure_keeps_live_but_makes_ready_unavailable()
{
    using var factory = new UnavailableDatabaseZapasApiFactory();
    using var client = factory.CreateClient();

    var liveResponse = await client.GetAsync("/health/live");
    var readyResponse = await client.GetAsync("/health/ready");

    liveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    readyResponse.StatusCode.Should()
        .Be(HttpStatusCode.ServiceUnavailable);
}
```

Testing both endpoints under the same failure proves the semantic separation,
not just two happy-path URLs.

### Step 4. Check for information disclosure

Read the response body from the failing readiness request and assert that it
does not contain values unique to the failure:

```csharp
var body = await readyResponse.Content.ReadAsStringAsync();

body.Should().NotContain(
    unavailableDatabase,
    "health responses must not reveal database paths");
body.Should().NotContain(
    "Microsoft.Data.Sqlite",
    "health responses must not reveal exception implementation details");
body.Should().NotContain(
    " at ",
    "health responses must not include a stack trace");
```

Expose the generated unavailable path as an internal property on the factory if
the test needs it. Prefer checking a unique marker over asserting only that the
body equals a framework string; the security requirement should survive a
future harmless response-format change.

### Step 5. Run focused and full tests

```powershell
dotnet test Zapas.Api.Tests/Zapas.Api.Tests.csproj `
    --filter "FullyQualifiedName~HealthEndpointTests"

dotnet test Zapas.slnx
```

If the readiness failure test unexpectedly returns `200`, confirm that:

- The replacement removed both EF Core registration types.
- The unavailable factory does not call `EnsureCreated`.
- `Mode=ReadWrite` is present.
- The database health registration still has the `ready` tag.

## Task 4: Review proxy behavior

This task is an evidence-gathering exercise. Do not add forwarded-header
middleware merely because the application will run behind a proxy.

### Step 1. Record the current facts

In `documents/architecture/week01-azure-development.md`, add a
`## HTTPS and proxy behavior` section. Begin with facts visible in the code:

- `UseHttpsRedirection()` is enabled.
- JWT metadata requires HTTPS outside Development.
- Swagger is enabled only in Development.
- No explicit forwarded-header middleware is currently registered.

### Step 2. Choose the intended App Service OS

Record whether the Week 1 plan is Windows or Linux. The proxy behavior differs:

- IIS integration configures forwarded headers for Windows-hosted ASP.NET Core.
- Linux/non-IIS hosting requires the forwarded scheme to be handled explicitly,
  commonly with the platform setting
  `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` or deliberately configured
  middleware.

Use the current Microsoft references rather than memory:

- [Configure ASP.NET Core for Azure App Service](https://learn.microsoft.com/azure/app-service/configure-language-dotnetcore)
- [Configure ASP.NET Core for proxy servers and load balancers](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-10.0)

### Step 3. Run a local proxy-focused experiment

If the deployment does not exist yet, use a focused integration test to observe
scheme handling. Send a request with `X-Forwarded-Proto: https` to a temporary
diagnostic endpoint available only in the test host, then observe whether
`Request.Scheme` is `http` or `https`.

Do not add a production diagnostic endpoint that returns arbitrary request
headers. Record:

- Host model tested.
- Input headers.
- Observed scheme.
- Whether an HTTPS redirect loop occurred.
- The exact configuration or middleware required, if any.

### Step 4. Verify the deployed behavior when available

For the actual App Service, check:

1. `http://<host>/health/live` redirects to HTTPS.
2. `https://<host>/health/live` returns `200` without a redirect loop.
3. The application observes the public request as HTTPS.
4. JWT/OIDC metadata is fetched over HTTPS in Production.
5. `/swagger` is unavailable outside Development.

If Linux App Service needs the forwarded-header platform setting, prefer
recording and provisioning that explicit setting. If code middleware is
required, place `UseForwardedHeaders` before middleware that reads the scheme,
including HTTPS redirection.

### Task 4 checkpoint

The architecture note must finish with one of these conclusions, supported by
evidence:

- Platform integration handles the forwarded scheme; no code change is needed.
- An explicit App Service setting is required.
- Explicit middleware is required, with its trust boundary and ordering
  documented.
- Deployment evidence is still pending, with an owner and target date.

## Task 5: Complete ADR-0001

### Step 1. Create the ADR

Create `learnings/adr/0001-host-zapas-on-azure-app-service.md` with this
structure:

```markdown
# ADR-0001: Host Zapas on Azure App Service

- Status: Proposed
- Date: YYYY-MM-DD

## Context

## Decision

## Options considered

### Azure App Service

### Azure Container Apps

### Azure Functions

## Positive consequences

## Negative consequences

## Constraints

## Revisit triggers
```

Use the current date and keep the status `Proposed` until the appropriate
reviewer accepts it.

### Step 2. Ground the context in this repository

Include these facts:

- Zapas is one controller-based ASP.NET Core HTTP API targeting `net10.0`.
- It is published as compiled .NET binaries.
- Upload parsing is synchronous within an HTTP request.
- SQLite, `IMemoryCache`, and rate-limit counters impose single-instance
  constraints for Week 1.
- The learning deployment needs managed HTTPS, settings, logs, health probes,
  and infrastructure-as-code support.

Avoid arguing that App Service is universally better. The decision is that it
fits the current workload with fewer new operational components.

### Step 3. State one decision and its cost

The decision should be direct:

> Host the Week 1 Zapas development deployment as a single instance on Azure
> App Service using the native .NET runtime, subject to .NET 10 availability in
> the selected OS and region.

Admit at least these costs:

- The App Service plan has a recurring cost outside free/shared allowances.
- Lower development tiers may not support deployment slots.
- Scaling out is intentionally blocked by local SQLite and process-local state.
- App Service conventions create some platform coupling.

Deployment slots require Standard tier or higher; do not imply a slot-based
release process for Free, Shared, or Basic. Check current tier features before
provisioning:

- [App Service plans](https://learn.microsoft.com/azure/app-service/overview-hosting-plans)
- [App Service deployment slots](https://learn.microsoft.com/azure/developer/azure-developer-cli/app-service-slots)

### Step 4. Perform the runtime-support gate

Do not infer regional runtime availability from the project target alone.
Capture the command and its output for the selected OS:

```powershell
az webapp list-runtimes --os linux
```

For a Windows plan, use:

```powershell
az webapp list-runtimes --os windows
```

Confirm that an appropriate .NET 10 native runtime is available before writing
the infrastructure definition. If it is not available in the chosen host/region
combination, document the container fallback rather than silently changing the
architecture.

### Step 5. Add concrete revisit triggers

Use observable triggers, for example:

- Native .NET 10 is unavailable in the required region.
- The workload standardizes on containers for a demonstrated operational need.
- Background or event-driven workloads appear.
- SQLite is replaced and all process-local correctness constraints are removed,
  making horizontal scale a real requirement.
- Revision traffic splitting or scale-to-zero becomes a measured requirement.

"Container Apps is more cloud-native" is not a trigger. A trigger identifies a
capability the current decision cannot satisfy economically or safely.

### Task 5 checkpoint

Ask another engineer to answer these questions using only the ADR:

1. What are we deploying?
2. Why App Service now?
3. Why only one instance?
4. What does the decision cost?
5. What specific observation would make us reconsider?

If any answer requires unwritten context, revise the ADR.

## Final verification

Run the complete Day 2 verification from the repository root:

```powershell
dotnet build Zapas.slnx
dotnet test Zapas.slnx --no-build
dotnet publish Zapas.Api/Zapas.Api.csproj `
    -c Release `
    -o .artifacts/publish
```

Run the published artifact with production-style environment settings:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__ZapasDb = "Data Source=zapas-day02.db"
$env:Jwt__Authority = "https://example.invalid/"
$env:Jwt__Audience = "zapas-api"
$env:Cors__AllowedOrigins__0 = "https://localhost.example"

dotnet .artifacts/publish/Zapas.Api.dll
```

Probe `/health/live` and `/health/ready`, then inspect logs for accidental
credentials, tokens, connection strings, or uploaded file content.

When finished, remove only the task-scoped environment variables:

```powershell
Remove-Item Env:ASPNETCORE_ENVIRONMENT
Remove-Item Env:ConnectionStrings__ZapasDb
Remove-Item Env:Jwt__Authority
Remove-Item Env:Jwt__Audience
Remove-Item Env:Cors__AllowedOrigins__0
```

Review the final diff:

```powershell
git status --short
git diff --check
git diff
```

The build is complete when all six Day 2 definition-of-done items are supported
by either executable tests or a linked, evidence-based document.
