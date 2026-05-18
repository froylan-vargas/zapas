# Day 5 — Testing, Clean Code, Refactoring, Mock Interview

## Instructor Goal

Today you are practicing how to prove that Zapas is maintainable, testable, and safe to change.

By the end of the day, you should be able to explain:

- What belongs in a unit test versus an integration test
- How test doubles help isolate service behavior
- How API integration tests exercise routing, model binding, middleware, and dependency injection
- Why Arrange / Act / Assert keeps tests readable
- How to refactor code without changing behavior
- How SOLID principles apply pragmatically in a small ASP.NET Core API
- How to discuss testing strategy in a senior interview
- How to handle the first mock interview without rambling

The objective is not to chase 100% test coverage. The objective is to cover behavior that protects the API from regressions.

---

## 5-Hour Structure

| Activity | Time |
|---|---:|
| Build/refactor code | 2 hours |
| Technical review | 1 hour |
| Live coding | 1 hour |
| Interview speaking practice | 1 hour |

Main rule for today:

> Test behavior and contracts, not private implementation details.

---

## Hour 1-2 — Build / Refactor Code

### Target Area

Work inside the current Zapas solution:

- `Zapas.slnx`
- `Zapas.Api/Program.cs`
- `Zapas.Api/Services/SessionService.cs`
- `Zapas.Api/Repositories/SessionRepository.cs`
- `Zapas.Api.Tests/`

Current good baseline:

- Controllers are thin.
- Service contains validation, FIT parsing, caching, and orchestration.
- Repository isolates EF Core persistence.
- Async and cancellation flow through controller, service, and repository methods.
- `GET /sessions` already uses filtering, sorting, pagination, projection, and `AsNoTracking()`.

Today you will add tests around the behavior that matters most.

---

## Task 1 — Add A Test Project

### Why

Tests should live outside the API project so production code does not reference test packages.

Use:

- xUnit for the test framework
- FluentAssertions for readable assertions
- NSubstitute for mocks/test doubles
- `Microsoft.AspNetCore.Mvc.Testing` for integration tests
- EF Core SQLite in-memory connections for realistic repository tests

### Implementation

Create the test project:

```bash
dotnet new xunit -n Zapas.Api.Tests
dotnet sln Zapas.slnx add Zapas.Api.Tests/Zapas.Api.Tests.csproj
dotnet add Zapas.Api.Tests/Zapas.Api.Tests.csproj reference Zapas.Api/Zapas.Api.csproj
dotnet add Zapas.Api.Tests/Zapas.Api.Tests.csproj package FluentAssertions
dotnet add Zapas.Api.Tests/Zapas.Api.Tests.csproj package NSubstitute
dotnet add Zapas.Api.Tests/Zapas.Api.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add Zapas.Api.Tests/Zapas.Api.Tests.csproj package Microsoft.EntityFrameworkCore.Sqlite
```

Expected `Zapas.Api.Tests/Zapas.Api.Tests.csproj` shape:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="FluentAssertions" Version="8.8.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.7" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.7" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Zapas.Api\Zapas.Api.csproj" />
  </ItemGroup>

</Project>
```

### Interview Explanation

> "I keep tests in a separate project so production code stays clean. I use unit tests for business rules and service orchestration, repository tests for persistence behavior, and API integration tests for routing, middleware, model binding, and HTTP responses."

---

## Task 2 — Make The API Test Host Friendly

### Why

`WebApplicationFactory<Program>` needs access to the API entry point.

### Implementation

At the bottom of `Zapas.Api/Program.cs`, add:

```csharp
public partial class Program;
```

If rate limiting is enabled, make sure the middleware order is correct:

```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");
```

### Interview Explanation

> "A partial `Program` class is a common ASP.NET Core testing hook. It lets the test project create an in-memory host with the same startup path as production."

---

## Task 3 — Unit Test File Validation

### Why This Behavior

Upload validation is a good unit-test target because it is deterministic and protects an expensive endpoint.

Test these cases:

- Empty file is rejected.
- Wrong extension is rejected.
- Oversized file is rejected.
- Repository is not called when validation fails.

### Implementation

Create `Zapas.Api.Tests/Services/SessionServiceValidationTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Zapas.Api.Repositories;
using Zapas.Api.Services;

namespace Zapas.Api.Tests.Services;

public sealed class SessionServiceValidationTests
{
    private readonly ISessionRepository _repository = Substitute.For<ISessionRepository>();
    private readonly SessionService _service;

    public SessionServiceValidationTests()
    {
        _service = new SessionService(
            _repository,
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<SessionService>.Instance);
    }

    [Fact]
    public async Task CreateSessionAsync_rejects_empty_file()
    {
        using var stream = Stream.Null;

        var result = await _service.CreateSessionAsync(
            stream,
            "activity.fit",
            fileLength: 0,
            CancellationToken.None);

        result.State.Should().Be(CreateSessionState.Rejected);
        result.Error.Should().Be("A non-empty .fit file is required.");
        await _repository.DidNotReceiveWithAnyArgs()
            .AddSessionAsync(default!, default);
    }

    [Fact]
    public async Task CreateSessionAsync_rejects_non_fit_extension()
    {
        using var stream = new MemoryStream([1, 2, 3]);

        var result = await _service.CreateSessionAsync(
            stream,
            "activity.txt",
            fileLength: stream.Length,
            CancellationToken.None);

        result.State.Should().Be(CreateSessionState.Rejected);
        result.Error.Should().Be("Only .fit files are supported.");
        await _repository.DidNotReceiveWithAnyArgs()
            .AddSessionAsync(default!, default);
    }

    [Fact]
    public async Task CreateSessionAsync_rejects_oversized_file()
    {
        using var stream = new MemoryStream([1]);
        const long maxFileSize = 10 * 1024 * 1024;

        var result = await _service.CreateSessionAsync(
            stream,
            "activity.fit",
            fileLength: maxFileSize + 1,
            CancellationToken.None);

        result.State.Should().Be(CreateSessionState.Rejected);
        result.Error.Should().Be("The uploaded file is too large.");
        await _repository.DidNotReceiveWithAnyArgs()
            .AddSessionAsync(default!, default);
    }
}
```

### Interview Explanation

> "These tests protect the upload boundary. They also verify that invalid input exits before parsing or persistence, which matters because parsing and database writes are the expensive parts of the workflow."

---

## Task 4 — Unit Test Session Lookup Caching

### Why This Behavior

`GET /sessions/{id}` uses `IMemoryCache`. A useful test verifies the observable behavior: two service calls should only hit the repository once.

### Implementation

Add this test to `SessionServiceValidationTests.cs` or create `SessionServiceCachingTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Zapas.Api.Models;
using Zapas.Api.Repositories;
using Zapas.Api.Services;

namespace Zapas.Api.Tests.Services;

public sealed class SessionServiceCachingTests
{
    [Fact]
    public async Task GetSessionByIdAsync_returns_cached_session_after_first_lookup()
    {
        var repository = Substitute.For<ISessionRepository>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new SessionService(
            repository,
            cache,
            NullLogger<SessionService>.Instance);

        var session = new Session(
            Id: Guid.NewGuid(),
            Name: "Morning run",
            TotalDistance: 5000,
            TotalDuration: TimeSpan.FromMinutes(25),
            AveragePace: TimeSpan.FromMinutes(5),
            AverageHeartRate: 150,
            MaxHeartRate: 170,
            StartTime: DateTimeOffset.UtcNow,
            CreatedAt: DateTimeOffset.UtcNow,
            RunIntervals: []);

        repository
            .GetSessionByIdAsync(session.Id, Arg.Any<CancellationToken>())
            .Returns(session);

        var first = await service.GetSessionByIdAsync(session.Id, CancellationToken.None);
        var second = await service.GetSessionByIdAsync(session.Id, CancellationToken.None);

        first.Should().BeSameAs(session);
        second.Should().BeSameAs(session);
        await repository.Received(1)
            .GetSessionByIdAsync(session.Id, Arg.Any<CancellationToken>());
    }
}
```

### Interview Explanation

> "I do not test the internals of `IMemoryCache`. I test the contract I care about: after a cacheable lookup, the service avoids a second repository call."

---

## Task 5 — Repository Tests With SQLite

### Why Not Mock EF Core

Mocking `DbSet` usually gives false confidence because the important behavior is LINQ translation, relationships, and database execution.

For repository tests, use SQLite in-memory so EF Core runs real SQL against a lightweight database.

### Implementation

Create `Zapas.Api.Tests/Repositories/SessionRepositoryTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Zapas.Api.Data;
using Zapas.Api.DTOs;
using Zapas.Api.Models;
using Zapas.Api.Repositories;

namespace Zapas.Api.Tests.Repositories;

public sealed class SessionRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ZapasDbContext _dbContext;
    private readonly SessionRepository _repository;

    public SessionRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ZapasDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ZapasDbContext(options);
        _dbContext.Database.EnsureCreated();
        _repository = new SessionRepository(_dbContext);
    }

    [Fact]
    public async Task AddSessionAsync_stores_session_with_intervals()
    {
        var session = new Session(
            Id: Guid.NewGuid(),
            Name: "Intervals",
            TotalDistance: 1000,
            TotalDuration: TimeSpan.FromMinutes(4),
            AveragePace: TimeSpan.FromMinutes(4),
            AverageHeartRate: 160,
            MaxHeartRate: 180,
            StartTime: DateTimeOffset.UtcNow,
            CreatedAt: DateTimeOffset.UtcNow,
            RunIntervals:
            [
                new RunInterval(
                    Distance: 1000,
                    Duration: TimeSpan.FromMinutes(4),
                    AverageHeartRate: 160,
                    MaxHeartRate: 180,
                    Pace: TimeSpan.FromMinutes(4))
            ]);

        await _repository.AddSessionAsync(session, CancellationToken.None);

        var stored = await _repository.GetSessionByIdAsync(session.Id, CancellationToken.None);

        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Intervals");
        stored.RunIntervals.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSessionsAsync_applies_date_filter_and_pagination()
    {
        await _repository.AddSessionAsync(CreateSession("Old", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)), CancellationToken.None);
        await _repository.AddSessionAsync(CreateSession("New", new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)), CancellationToken.None);

        var request = new GetSessionsRequestDto(
            Page: 1,
            PageSize: 10,
            Sort: "startTime",
            From: new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero),
            To: null,
            HasIntervals: null,
            Name: null);

        var sessions = await _repository.GetSessionsAsync(request, CancellationToken.None);

        sessions.Should().ContainSingle();
        sessions[0].Name.Should().Be("New");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    private static Session CreateSession(string name, DateTimeOffset startTime)
    {
        return new Session(
            Id: Guid.NewGuid(),
            Name: name,
            TotalDistance: 5000,
            TotalDuration: TimeSpan.FromMinutes(25),
            AveragePace: TimeSpan.FromMinutes(5),
            AverageHeartRate: null,
            MaxHeartRate: null,
            StartTime: startTime,
            CreatedAt: startTime,
            RunIntervals: []);
    }
}
```

### Interview Explanation

> "For repository tests, I prefer a real provider over mocking EF Core. SQLite in-memory gives me fast tests while still exercising relational behavior, query translation, and persistence mapping."

---

## Task 6 — Integration Tests For HTTP Behavior

### Why

Unit tests do not prove that routing, dependency injection, middleware, model binding, validation attributes, and JSON responses work together.

### Implementation

Create `Zapas.Api.Tests/Infrastructure/ZapasApiFactory.cs`:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Zapas.Api.Data;

namespace Zapas.Api.Tests.Infrastructure;

public sealed class ZapasApiFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                service => service.ServiceType == typeof(DbContextOptions<ZapasDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ZapasDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ZapasDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
```

Create `Zapas.Api.Tests/Controllers/SessionsControllerTests.cs`:

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Zapas.Api.Data;
using Zapas.Api.Entities;
using Zapas.Api.Tests.Infrastructure;

namespace Zapas.Api.Tests.Controllers;

public sealed class SessionsControllerTests : IClassFixture<ZapasApiFactory>
{
    private readonly ZapasApiFactory _factory;
    private readonly HttpClient _client;

    public SessionsControllerTests(ZapasApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSessions_returns_ok_with_json_array()
    {
        await SeedSessionAsync();

        var response = await _client.GetAsync("/sessions?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var sessions = await response.Content.ReadFromJsonAsync<List<object>>();
        sessions.Should().NotBeNull();
        sessions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetSession_returns_not_found_for_unknown_id()
    {
        var response = await _client.GetAsync($"/sessions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostSession_returns_bad_request_for_missing_file()
    {
        using var content = new MultipartFormDataContent();

        var response = await _client.PostAsync("/sessions", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task SeedSessionAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZapasDbContext>();

        dbContext.Sessions.Add(new SessionEntity
        {
            Id = Guid.NewGuid(),
            Name = "Seeded run",
            StartTime = DateTimeOffset.UtcNow,
            TotalDistanceMeters = 5000,
            TotalDuration = TimeSpan.FromMinutes(25),
            AveragePaceSecondsPerKm = 300,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            AverageHeartRate = 150,
            MaxHeartRate = 170,
            Intervals = []
        });

        await dbContext.SaveChangesAsync();
    }
}
```

### Interview Explanation

> "Integration tests are slower than unit tests, so I use fewer of them. I keep them focused on important HTTP contracts: status codes, routing, response shape, validation, and middleware behavior."

---

## Task 7 — Clean Code Refactor: Extract FIT Parsing

### Why

`SessionService` currently validates uploads, parses FIT data, persists sessions, caches results, and logs timings. That is acceptable for a small app, but parsing can be extracted behind an interface to make the service easier to test.

### Implementation

Create `Zapas.Api/Services/IFitSessionParser.cs`:

```csharp
using Zapas.Api.Models;

namespace Zapas.Api.Services;

public interface IFitSessionParser
{
    Session Parse(Stream fitStream, string? fallbackName);
}
```

Create `Zapas.Api/Services/FitSessionParser.cs` and move `ExtractSessionInfo`, `GetPace`, `ToUtcDateTimeOffset`, and `GetSessionName` from `SessionService` into it:

```csharp
using Dynastream.Fit;
using Zapas.Api.Models;

namespace Zapas.Api.Services;

public sealed class FitSessionParser : IFitSessionParser
{
    public Session Parse(Stream fitStream, string? fallbackName)
    {
        var decoder = new Decode();
        var broadcaster = new MesgBroadcaster();
        var intervals = new List<RunInterval>();
        SessionMesg? session = null;

        decoder.MesgEvent += broadcaster.OnMesg;
        decoder.MesgDefinitionEvent += broadcaster.OnMesgDefinition;

        broadcaster.SessionMesgEvent += (_, args) =>
        {
            if (args.mesg is SessionMesg sessionMesg)
            {
                session = sessionMesg;
            }
        };

        broadcaster.LapMesgEvent += (_, args) =>
        {
            if (args.mesg is not LapMesg lap)
            {
                return;
            }

            if (lap.GetSport() != Sport.Running || lap.GetIntensity() != Intensity.Active)
            {
                return;
            }

            var distance = lap.GetTotalDistance();
            var duration = lap.GetTotalTimerTime();

            if (distance is null || duration is null || distance <= 0 || duration <= 0)
            {
                return;
            }

            intervals.Add(new RunInterval(
                Distance: distance.Value,
                Duration: TimeSpan.FromSeconds(duration.Value),
                AverageHeartRate: lap.GetAvgHeartRate(),
                MaxHeartRate: lap.GetMaxHeartRate(),
                Pace: TimeSpan.FromSeconds(duration.Value / (distance.Value / 1000))));
        };

        decoder.Read(fitStream);

        var totalDistance = session?.GetTotalDistance();
        var totalTime = session?.GetTotalTimerTime();

        return new Session(
            Id: Guid.NewGuid(),
            Name: GetSessionName(session, fallbackName),
            TotalDistance: totalDistance ?? 0,
            TotalDuration: totalTime is null ? TimeSpan.Zero : TimeSpan.FromSeconds(totalTime.Value),
            AveragePace: GetPace(totalDistance, totalTime) ?? TimeSpan.Zero,
            AverageHeartRate: session?.GetAvgHeartRate(),
            MaxHeartRate: session?.GetMaxHeartRate(),
            StartTime: ToUtcDateTimeOffset(session?.GetStartTime()?.GetDateTime()) ?? DateTimeOffset.UtcNow,
            CreatedAt: DateTimeOffset.UtcNow,
            RunIntervals: intervals);
    }

    internal static TimeSpan? GetPace(float? distance, float? duration)
    {
        if (distance is null || duration is null || distance <= 0 || duration <= 0)
        {
            return null;
        }

        return TimeSpan.FromSeconds(duration.Value / (distance.Value / 1000));
    }

    private static DateTimeOffset? ToUtcDateTimeOffset(DateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        var dateTime = value.Value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : value.Value.ToUniversalTime();

        return new DateTimeOffset(dateTime, TimeSpan.Zero);
    }

    private static string GetSessionName(SessionMesg? session, string? fallbackName)
    {
        var name = session?.GetSportProfileNameAsString();

        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        var fileName = Path.GetFileNameWithoutExtension(fallbackName);

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return fileName;
        }

        return session?.GetSport()?.ToString() ?? "Unknown session";
    }
}
```

Update `SessionService` constructor:

```csharp
private readonly IFitSessionParser _fitSessionParser;

public SessionService(
    ISessionRepository sessionRepository,
    IMemoryCache cache,
    ILogger<SessionService> logger,
    IFitSessionParser fitSessionParser)
{
    _sessionRepository = sessionRepository;
    _cache = cache;
    _logger = logger;
    _fitSessionParser = fitSessionParser;
}
```

Update parsing call:

```csharp
session = _fitSessionParser.Parse(fitStream, fileName);
```

Register in `Program.cs`:

```csharp
builder.Services.AddScoped<IFitSessionParser, FitSessionParser>();
```

Unit test pace calculation:

```csharp
using FluentAssertions;
using Zapas.Api.Services;

namespace Zapas.Api.Tests.Services;

public sealed class FitSessionParserTests
{
    [Fact]
    public void GetPace_returns_null_when_distance_is_zero()
    {
        FitSessionParser.GetPace(0, 1200).Should().BeNull();
    }

    [Fact]
    public void GetPace_returns_seconds_per_kilometer()
    {
        var pace = FitSessionParser.GetPace(5000, 1500);

        pace.Should().Be(TimeSpan.FromSeconds(300));
    }
}
```

### Interview Explanation

> "I extract parsing when the service starts doing too much. The service should orchestrate validation, parsing, persistence, caching, and logging. The parser should know FIT SDK details. That makes the workflow easier to unit test with a fake parser."

### Tradeoff To Say Out Loud

This refactor is useful if you want isolated tests around the workflow. If the app is tiny and parsing rarely changes, keeping it in the service is acceptable until complexity grows.

---

## Hour 3 — Technical Review

### Unit Tests

Say this clearly:

> "A unit test checks a small piece of behavior in isolation. It should be fast, deterministic, and easy to understand."

Good unit-test candidates:

- File validation rules
- Pace calculation
- Caching behavior
- Service orchestration
- Mapping logic

Poor unit-test candidates:

- EF Core LINQ translation
- Full HTTP pipeline behavior
- Third-party FIT SDK internals
- Database constraints

### Integration Tests

Say this clearly:

> "An integration test verifies that multiple parts work together. In ASP.NET Core, I use them to test routing, dependency injection, middleware, model binding, filters, and HTTP responses."

Good integration-test candidates:

- `GET /sessions` returns `200 OK`.
- `GET /sessions/{id}` returns `404` for an unknown id.
- `POST /sessions` returns `400` for invalid upload input.
- `/health` returns a health response.

### Mocking

Use mocks when:

- You want to isolate service logic.
- The dependency is slow, nondeterministic, or external.
- You need to verify an interaction, such as "repository was not called."

Avoid mocks when:

- The fake is more complicated than the real dependency.
- You are testing EF Core query behavior.
- The mock would duplicate implementation details.

### Clean Code

Practical senior framing:

> "Clean code is code that is easy to change safely. I care more about clear boundaries, small methods, explicit names, focused tests, and low surprise than applying patterns for their own sake."

### SOLID In Zapas

- Single Responsibility: controllers handle HTTP, services handle workflows, repositories handle persistence.
- Open/Closed: new repository or parser implementations can be added behind interfaces.
- Liskov Substitution: interface implementations should preserve expected behavior.
- Interface Segregation: `ISessionRepository` should expose only session persistence operations.
- Dependency Inversion: `SessionService` depends on abstractions instead of concrete EF Core classes.

---

## Hour 4 — Live Coding

Spend one hour coding without looking up full solutions. After each exercise, explain complexity and test cases out loud.

### Exercise 1 — Validate File Name

Write a method that validates a `.fit` file name.

Starter:

```csharp
public static bool IsValidFitFileName(string? fileName)
{
}
```

Implementation:

```csharp
public static bool IsValidFitFileName(string? fileName)
{
    if (string.IsNullOrWhiteSpace(fileName))
    {
        return false;
    }

    return string.Equals(
        Path.GetExtension(fileName),
        ".fit",
        StringComparison.OrdinalIgnoreCase);
}
```

Test cases:

- `activity.fit` returns true.
- `activity.FIT` returns true.
- `activity.txt` returns false.
- `null` returns false.
- empty string returns false.

### Exercise 2 — Calculate Pace

Write a method that returns pace per kilometer.

Starter:

```csharp
public static TimeSpan? CalculatePace(float distanceMeters, float durationSeconds)
{
}
```

Implementation:

```csharp
public static TimeSpan? CalculatePace(float distanceMeters, float durationSeconds)
{
    if (distanceMeters <= 0 || durationSeconds <= 0)
    {
        return null;
    }

    var distanceKilometers = distanceMeters / 1000;
    return TimeSpan.FromSeconds(durationSeconds / distanceKilometers);
}
```

Interview explanation:

> "I return null for invalid inputs because zero distance or zero duration cannot produce a meaningful pace. I would rather make invalid state explicit than hide it behind a default value."

### Exercise 3 — Testable Service With A Fake

Given this service:

```csharp
public sealed class ImportService
{
    public async Task<bool> ImportAsync(Stream stream)
    {
        var parser = new ExpensiveParser();
        var result = parser.Parse(stream);
        await SaveAsync(result);
        return true;
    }
}
```

Identify the problems:

- The service creates its own dependency.
- The parser cannot be replaced in tests.
- Failure behavior is unclear.
- Persistence is hidden inside the service.

Better shape:

```csharp
public interface IParser
{
    ParsedResult Parse(Stream stream);
}

public interface IImportRepository
{
    Task SaveAsync(ParsedResult result, CancellationToken cancellationToken);
}

public sealed class ImportService
{
    private readonly IParser _parser;
    private readonly IImportRepository _repository;

    public ImportService(IParser parser, IImportRepository repository)
    {
        _parser = parser;
        _repository = repository;
    }

    public async Task ImportAsync(Stream stream, CancellationToken cancellationToken)
    {
        var result = _parser.Parse(stream);
        await _repository.SaveAsync(result, cancellationToken);
    }
}
```

Interview explanation:

> "Constructor injection makes dependencies visible and replaceable. That improves testability and makes the service easier to reason about."

### Exercise 4 — Find The Test Smell

Given this test:

```csharp
[Fact]
public async Task Test1()
{
    var service = new SessionService(...);
    var result = await service.CreateSessionAsync(...);
    Assert.True(result != null);
}
```

Identify the problems:

- Test name does not describe behavior.
- Setup is hidden behind `...`.
- Assertion is too weak.
- It does not explain the expected state.
- It may depend on too many real dependencies.

Better shape:

```csharp
[Fact]
public async Task CreateSessionAsync_rejects_empty_file()
{
    using var stream = Stream.Null;

    var result = await _service.CreateSessionAsync(
        stream,
        "activity.fit",
        fileLength: 0,
        CancellationToken.None);

    result.State.Should().Be(CreateSessionState.Rejected);
    result.Error.Should().Be("A non-empty .fit file is required.");
}
```

---

## Hour 5 — Interview Speaking Practice

Answer these out loud. Keep each answer under 2 minutes.

### Questions

1. What is the difference between unit tests and integration tests?
2. What makes a good unit test?
3. What should you avoid mocking?
4. How would you test an ASP.NET Core controller?
5. How would you test EF Core repository behavior?
6. What is Arrange / Act / Assert?
7. What is dependency inversion?
8. What does Single Responsibility mean in practice?
9. How do you refactor safely?
10. How do you decide what to test?
11. What is a brittle test?
12. How would you improve test coverage in a legacy codebase?

### Strong Answer — Unit vs Integration Tests

> "A unit test checks a small behavior in isolation and should be fast, deterministic, and easy to diagnose when it fails. An integration test checks that multiple parts work together, such as routing, dependency injection, middleware, EF Core, and JSON responses. I use many focused unit tests and fewer integration tests around the most important API contracts."

### Strong Answer — What To Test

> "I prioritize tests around business rules, validation, edge cases, failure paths, and contracts that would hurt users if they regressed. In Zapas, that means upload validation, FIT parsing behavior, pace calculation, caching behavior, repository queries, and HTTP status codes. I do not test every getter or framework behavior."

### Strong Answer — Mocking

> "I use mocks to isolate my code from slow or external dependencies, and to verify important interactions. I avoid mocking EF Core queries because that does not prove the LINQ translates or the database relationship works. For repository behavior, I prefer a real lightweight provider such as SQLite in-memory."

### Strong Answer — Refactoring Safely

> "I refactor safely by first understanding the current behavior, adding characterization tests if needed, making small changes, and running tests after each meaningful step. I avoid mixing refactoring with unrelated feature changes because that makes regressions harder to diagnose."

### Behavioral Practice

Prepare short answers for:

- Tell me about yourself.
- Why are you exploring after 10 years?
- Tell me about a difficult bug.
- Tell me about a disagreement with another developer.
- Tell me about a system you improved.
- Tell me about a production issue.
- What are your strengths?
- What do you want to improve?

### "Tell Me About Yourself" Draft

> "I am a .NET web developer with 10 years of experience building and maintaining web applications in the same company. That gave me deep ownership of long-lived systems, production support experience, business context, and a strong focus on maintainability. Recently I have been refreshing modern ASP.NET Core, EF Core, testing, diagnostics, and architecture through a practice API called Zapas, which parses running FIT files and exposes session data through HTTP endpoints."

### "Why Are You Exploring?" Draft

> "After 10 years in one company, I am ready for a new environment where I can keep growing technically and work with broader engineering practices. I value the ownership and domain depth I built there, but I want my next role to expose me to more modern .NET systems, stronger testing culture, cloud-native practices, and larger technical challenges."

---

## Day 5 Deliverable

At the end of today, Zapas should have:

- A separate `Zapas.Api.Tests` project
- Unit tests for upload validation
- Unit tests for caching behavior
- Unit tests for pace calculation if parsing is extracted
- Repository tests using SQLite in-memory
- API integration tests for important HTTP behavior
- Optional `IFitSessionParser` extraction
- Clear explanations for test strategy, mocking, integration tests, and refactoring

Write notes for yourself:

- One behavior I protected with a unit test
- One API contract I protected with an integration test
- One refactor I made easier because tests existed
- One test I deliberately did not write and why
- One testing tradeoff I can explain clearly

---

## Final Interview Framing

Use this as your Day 5 summary:

> "I improved Zapas by adding a test project with focused unit tests and API integration tests. I tested upload validation, caching behavior, repository persistence, and important HTTP responses. I can explain why I use unit tests for business behavior, integration tests for API contracts, SQLite in-memory for EF Core behavior, and mocks only where they clarify service-level tests."
