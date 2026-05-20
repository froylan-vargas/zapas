# Day 6 — Architecture, Design Patterns, Senior Judgment

## Instructor Goal

Today you are practicing how to explain architecture decisions like a senior .NET engineer using the Zapas FIT Session API.

By the end of the day, you should be able to explain:

- How to structure a medium-sized ASP.NET Core application
- What belongs in controllers, services, domain models, repositories, and infrastructure
- Why dependency inversion improves testability and maintainability
- Where Clean Architecture helps and where it can become ceremony
- When Repository, Unit of Work, Mediator, CQRS, and DDD concepts are useful
- Why a modular monolith is often a better starting point than microservices
- How to discuss tradeoffs without sounding dogmatic

The objective is not to rebuild Zapas into a textbook architecture. The objective is to make its boundaries clearer and easier to defend in an interview.

---

## 5-Hour Structure

| Activity | Time |
|---|---:|
| Build/refactor code | 2 hours |
| Technical review | 1 hour |
| Live coding | 1 hour |
| Interview speaking practice | 1 hour |

Main rule for today:

> Use architecture to reduce real complexity, not to create impressive folders.

---

## Hour 1-2 — Build / Refactor Code

### Target Area

Work inside the current Zapas API:

- `Zapas.Api/Controllers/SessionsController.cs`
- `Zapas.Api/Services/ISessionService.cs`
- `Zapas.Api/Services/SessionService.cs`
- `Zapas.Api/Services/IFitSessionParser.cs`
- `Zapas.Api/Services/FitSessionParser.cs`
- `Zapas.Api/Repositories/ISessionRepository.cs`
- `Zapas.Api/Repositories/SessionRepository.cs`
- `Zapas.Api/Models/Session.cs`
- `Zapas.Api/Models/RunInterval.cs`
- `Zapas.Api/Entities/SessionEntity.cs`
- `Zapas.Api/Entities/RunIntervalEntity.cs`
- `Zapas.Api/DTOs/`
- `Zapas.Api/Program.cs`

Current good baseline:

- Controllers are thin and HTTP-focused.
- `SessionService` owns the create-session workflow.
- FIT parsing is already behind `IFitSessionParser`.
- Repository interfaces hide persistence details from services.
- EF Core entities are separate from API DTOs.
- Tests exist or are planned around validation, caching, repository behavior, and HTTP behavior.

Today you will make the architecture easier to explain, not necessarily larger.

---

## Task 1 — Draw The Current Boundaries

### Why

Before refactoring, be able to describe what the app already does well.

Write a short architecture note for yourself:

```text
HTTP/API layer
  Controllers, DTOs, request validation shape, status codes

Application layer
  SessionService, create-session workflow, orchestration, caching decisions

Domain layer
  Session, RunInterval, pace rules, invariants, values the app cares about

Infrastructure layer
  EF Core DbContext, repositories, FIT SDK parser, database entities
```

### Interview Explanation

> "I think about architecture as boundaries between reasons to change. HTTP concerns change for API contract reasons, application services change for workflow reasons, domain models change for business rules, and infrastructure changes when persistence or external libraries change."

### What To Avoid

Do not claim Zapas is a perfect Clean Architecture implementation. Say it is a small API with clean boundaries inspired by those ideas.

---

## Task 2 — Verify The FIT Parser Boundary

### Why

FIT parsing uses an external SDK. That is infrastructure detail, not something controllers or domain models should know about.

Expected shape:

```csharp
public interface IFitSessionParser
{
    Session Parse(Stream fitStream, string fileName);
}
```

`SessionService` should depend on the abstraction:

```csharp
private readonly IFitSessionParser _fitSessionParser;
```

And use it inside the workflow:

```csharp
var session = _fitSessionParser.Parse(fitStream, fileName);
```

### Refactor If Needed

If `SessionService` still contains Garmin FIT SDK details, move them into `FitSessionParser`.

Keep `SessionService` focused on:

- File validation
- Cancellation checks
- Calling the parser
- Calling the repository
- Caching newly created sessions
- Returning a clear create result

### Interview Explanation

> "I hide the Garmin FIT SDK behind `IFitSessionParser` because parsing is an external technical detail. The service only needs the outcome: a parsed session. That makes the workflow easier to test and gives me a clear place to replace the parser if the SDK changes."

---

## Task 3 — Move Domain Rules Out Of Infrastructure

### Why

Pace calculation and session invariants are domain rules. They should not be buried inside controllers, EF entities, or DTO mapping code.

Good candidates:

- Pace calculation
- Validation that distance and duration are positive before calculating pace
- Rules for creating a `RunInterval`
- Rules for naming or defaulting a session

Possible shape:

```csharp
public static class RunningMetrics
{
    public static TimeSpan? CalculatePace(double distanceMeters, double durationSeconds)
    {
        if (distanceMeters <= 0 || durationSeconds <= 0)
        {
            return null;
        }

        var distanceKilometers = distanceMeters / 1000;
        return TimeSpan.FromSeconds(durationSeconds / distanceKilometers);
    }
}
```

Use it from the parser instead of duplicating pace math.

### Interview Explanation

> "I keep domain rules close to the domain model so they are reusable and testable. EF entities and DTOs are shapes for storage and transport; they should not become the main place where business rules live."

### Tradeoff To Say Out Loud

For a small app, a simple static helper can be enough. You do not need a complex domain service unless the rule becomes stateful, depends on collaborators, or grows beyond simple calculation.

---

## Task 4 — Make Mapping Explicit

### Why

Zapas has several shapes for similar data:

- Domain models: `Session`, `RunInterval`
- EF entities: `SessionEntity`, `RunIntervalEntity`
- API DTOs: `SessionDto`, `SessionSummaryDto`, `RunIntervalDto`

Mapping should be easy to find and consistent.

### Implementation Options

Use one of these approaches:

- Keep small mapping methods near the repository for entity/domain mapping.
- Keep DTO mapping near the controller or in focused extension methods.
- Avoid adding AutoMapper unless the mapping becomes repetitive enough to justify the dependency.

Possible DTO mapper:

```csharp
public static class SessionDtoMapper
{
    public static SessionDto ToDto(this Session session)
    {
        return new SessionDto(
            session.Id,
            session.Name,
            session.TotalDistance,
            session.TotalDuration,
            session.AveragePace,
            session.AverageHeartRate,
            session.MaxHeartRate,
            session.StartTime,
            session.CreatedAt,
            session.RunIntervals.Select(interval => interval.ToDto()).ToList());
    }

    private static RunIntervalDto ToDto(this RunInterval interval)
    {
        return new RunIntervalDto(
            interval.Distance,
            interval.Duration,
            interval.AverageHeartRate,
            interval.MaxHeartRate,
            interval.Pace);
    }
}
```

### Interview Explanation

> "I keep mapping explicit because it makes API contracts visible. I do not expose EF entities directly from controllers because storage shape and API shape should be allowed to change independently."

---

## Task 5 — Review Repository And Unit Of Work Boundaries

### Repository Pattern

Zapas uses a repository to hide persistence details from the application workflow.

Good repository responsibilities:

- Add a session
- Get a session by id
- Query sessions with filtering, sorting, and pagination
- Handle entity/domain mapping if that is the chosen local pattern

Poor repository responsibilities:

- Parse FIT files
- Decide HTTP status codes
- Know about `IFormFile`
- Build API DTO responses
- Contain unrelated business workflows

### Unit Of Work

EF Core `DbContext` already behaves like a Unit of Work:

- It tracks changes.
- It coordinates inserts and updates.
- `SaveChangesAsync` commits the transaction boundary for simple cases.

Do not add a custom `IUnitOfWork` unless there is a real need.

### Interview Explanation

> "I use a repository here to keep EF Core out of the service layer and to make persistence replaceable in tests. I would not add a separate Unit of Work abstraction yet because EF Core's `DbContext` already provides that behavior for this app."

---

## Task 6 — Decide Whether To Add More Projects

### Options

Option A: Keep one API project with clear folders.

This is enough when:

- The app is small.
- The team is small.
- Boundaries are clear by convention.
- Extra projects would mostly add ceremony.

Option B: Split into projects.

Possible future shape:

```text
Zapas.Api
Zapas.Application
Zapas.Domain
Zapas.Infrastructure
Zapas.Api.Tests
```

This is useful when:

- The codebase grows.
- Dependencies are leaking across boundaries.
- Multiple teams need clearer ownership.
- You want compile-time enforcement of architecture rules.

### Today's Recommendation

For Day 6, prefer Option A unless the code is already difficult to navigate.

Create a note explaining what the split would look like, but do not split projects just to prove you know Clean Architecture.

### Interview Explanation

> "I would start Zapas as a modular monolith with clear folders and dependency boundaries. If the codebase grew, I could split domain, application, infrastructure, and API into separate projects to enforce those boundaries at compile time."

---

## Task 7 — Add A Lightweight Architecture Decision Record

### Why

Senior engineers do not only write code. They make decisions traceable.

Create `docs/architecture-notes.md` if you want a durable note, or add private notes for yourself.

Suggested content:

```markdown
# Zapas Architecture Notes

## Current Approach

Zapas is an ASP.NET Core modular monolith. It uses controllers for HTTP concerns,
services for application workflows, domain models for session concepts, repositories
for persistence abstraction, and infrastructure adapters for EF Core and FIT parsing.

## Decisions

- Keep controllers thin.
- Keep FIT SDK usage behind IFitSessionParser.
- Keep EF Core behind ISessionRepository.
- Keep DTOs separate from domain models and EF entities.
- Avoid microservices until there is a clear deployment, scaling, team, or domain boundary need.
```

### Interview Explanation

> "I like lightweight architecture notes because they capture why a decision was made. That helps future maintainers understand the tradeoff instead of guessing from the final code shape."

---

## Hour 3 — Technical Review

### Layered Architecture

Basic direction:

```text
Controllers -> Services -> Repositories -> Database
```

Good answer:

> "Layered architecture is simple and understandable. The risk is that it can become too CRUD-oriented or allow business logic to drift into the wrong layer if the team is not disciplined."

### Clean Architecture

Core idea:

```text
Outer details depend inward on application and domain rules.
Domain should not depend on EF Core, ASP.NET Core, or external SDKs.
```

Good answer:

> "Clean Architecture helps protect business rules from framework and infrastructure changes. The downside is extra indirection, especially in small apps where too many abstractions can slow development without adding much value."

### Vertical Slice Architecture

Core idea:

Group code by feature instead of technical layer:

```text
Sessions/
  CreateSession/
  GetSession/
  ListSessions/
```

Good answer:

> "Vertical slices can reduce cross-folder changes because each feature owns its request, handler, validation, and response. I like it when an app has many use cases. For a small API like Zapas, layered folders are still easy to understand."

### SOLID In Practice

Keep the explanations practical:

- Single Responsibility: one reason to change
- Open/Closed: extend behavior without rewriting stable code
- Liskov Substitution: abstractions should be safely replaceable
- Interface Segregation: avoid forcing callers to depend on methods they do not use
- Dependency Inversion: high-level workflows depend on abstractions, not concrete details

Strong answer:

> "I use SOLID as a design smell checklist, not as a reason to add interfaces everywhere. In Zapas, dependency inversion matters around FIT parsing and persistence because those are external details that I want to test and replace."

### DDD Basics

Know these terms:

- Entity: object with identity, such as `Session`
- Value object: object defined by value, such as a pace or distance concept
- Aggregate: consistency boundary around related objects
- Domain service: domain behavior that does not naturally belong to one entity
- Repository: collection-like abstraction for loading and saving aggregates

Senior-level phrasing:

> "I would use DDD selectively. For Zapas, `Session` and `RunInterval` are useful domain concepts, but I would not introduce heavy DDD patterns unless the business rules became complex enough to justify them."

### CQRS

Core idea:

Separate write models from read models when reads and writes have different needs.

For Zapas:

- Commands: upload and create a session
- Queries: list sessions, get session details

Good answer:

> "Zapas already has a natural command/query split at the service level. I would not introduce full CQRS infrastructure unless querying and writing became complex enough to need separate models or storage paths."

### Mediator Pattern

Good use:

- Many use cases
- Complex pipeline behaviors
- Cross-cutting validation/logging around handlers
- Vertical slice organization

Bad use:

- Hiding simple method calls
- Making navigation harder
- Adding ceremony before there is complexity

Interview explanation:

> "Mediator can be useful when an app has many independent request handlers. I would not add it to Zapas yet because the current service layer is still small and readable."

### Modular Monolith vs Microservices

Use this answer often:

> "I prefer starting with a well-structured modular monolith unless there is a clear scaling, team, deployment, or domain boundary reason to split into microservices."

Microservices help when:

- Independent deployment matters
- Team ownership is clearly separated
- Scaling needs differ by component
- Domains are stable enough to split

Microservices hurt when:

- The team is small
- Boundaries are unclear
- Data consistency is simple today
- Operational maturity is low
- Network, deployment, and observability complexity would dominate

---

## Hour 4 — Live Coding

Practice these out loud. Focus on explaining design choices while coding.

### Exercise 1 — Extract An Interface

Given this service:

```csharp
public sealed class UploadService
{
    public Session Import(Stream stream, string fileName)
    {
        var parser = new FitParser();
        return parser.Parse(stream, fileName);
    }
}
```

Refactor it so the parser can be replaced in tests.

Expected shape:

```csharp
public interface ISessionParser
{
    Session Parse(Stream stream, string fileName);
}

public sealed class UploadService
{
    private readonly ISessionParser _parser;

    public UploadService(ISessionParser parser)
    {
        _parser = parser;
    }

    public Session Import(Stream stream, string fileName)
    {
        return _parser.Parse(stream, fileName);
    }
}
```

Interview explanation:

> "The service now depends on the parser abstraction instead of constructing the concrete parser. That makes the dependency visible, replaceable, and testable."

### Exercise 2 — Identify Layering Problems

Given this controller:

```csharp
[HttpPost]
public async Task<IActionResult> Upload(IFormFile file)
{
    var parser = new FitParser();
    var session = parser.Parse(file.OpenReadStream(), file.FileName);

    _dbContext.Sessions.Add(session);
    await _dbContext.SaveChangesAsync();

    return Ok(session);
}
```

Identify the problems:

- Controller knows about FIT parsing.
- Controller writes directly to the database.
- Controller returns internal shape directly.
- Validation and error handling are unclear.
- The workflow is hard to unit test.

Better shape:

```csharp
[HttpPost]
public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
{
    await using var stream = file.OpenReadStream();

    var result = await _sessionService.CreateSessionAsync(
        stream,
        file.FileName,
        file.Length,
        cancellationToken);

    return result.State switch
    {
        CreateSessionState.Created => CreatedAtAction(nameof(GetSession), new { id = result.Session!.Id }, result.Session.ToDto()),
        CreateSessionState.Rejected => BadRequest(new { error = result.Error }),
        _ => StatusCode(StatusCodes.Status422UnprocessableEntity, new { error = result.Error })
    };
}
```

Interview explanation:

> "The controller coordinates HTTP concerns and delegates the business workflow. That keeps it thin and makes the parsing and persistence behavior testable outside MVC."

### Exercise 3 — Choose Between Architecture Styles

Question:

> "Would you use Clean Architecture for a small API?"

Strong answer:

> "I would use the principles but not necessarily all the ceremony. I want controllers, application workflows, domain rules, and infrastructure details separated. For a small API, clear folders and dependency direction may be enough. I would split into separate projects when the codebase grows or when compile-time boundary enforcement becomes valuable."

### Exercise 4 — Design A Session Import Use Case

Whiteboard the flow:

```text
POST /sessions
  -> SessionsController validates HTTP shape
  -> SessionService validates file rules
  -> IFitSessionParser parses stream into Session
  -> ISessionRepository saves Session
  -> SessionService caches created Session
  -> Controller returns CreateSessionResponseDto
```

Explain each boundary:

- Controller: HTTP protocol and response codes
- Service: application workflow
- Parser: external FIT SDK adapter
- Repository: persistence abstraction
- DTO: API contract
- Domain model: app concept

---

## Hour 5 — Interview Speaking Practice

Answer these out loud. Keep each answer under 2 minutes.

### Questions

1. How do you structure a medium-sized .NET application?
2. What is Clean Architecture?
3. What are the downsides of Clean Architecture?
4. What is Vertical Slice Architecture?
5. When would you use microservices?
6. When would you avoid microservices?
7. What is a modular monolith?
8. How do you prevent business logic from leaking into controllers?
9. What is dependency inversion?
10. Why use a repository pattern?
11. Is EF Core already a repository and Unit of Work?
12. What is CQRS?
13. When would you use MediatR?
14. What DDD concepts have you used or understand?
15. How do you decide whether an abstraction is worth adding?

### Strong Answer — Structuring A .NET App

> "For a medium-sized ASP.NET Core app, I usually start with clear boundaries: controllers for HTTP, application services for workflows, domain models for business concepts, repositories or infrastructure services for persistence and external dependencies, and DTOs for API contracts. I keep the structure simple at first, then split into projects or vertical slices when the size of the codebase justifies stronger boundaries."

### Strong Answer — Clean Architecture

> "Clean Architecture keeps business and application rules independent from frameworks and infrastructure. In .NET, that usually means the domain and application layers do not depend on ASP.NET Core, EF Core, or external SDKs. The tradeoff is more interfaces and mapping, so I apply it pragmatically rather than mechanically."

### Strong Answer — Microservices

> "I would use microservices when there are clear domain boundaries, independent deployment needs, different scaling needs, and the team has the operational maturity to handle distributed systems. I would avoid them when the app is small or the boundaries are unclear because microservices add network, data consistency, deployment, and observability complexity."

### Strong Answer — Repository Pattern

> "A repository can be useful when I want the application layer to talk in domain terms instead of EF Core terms. In Zapas, `ISessionRepository` lets the service save and query sessions without knowing about DbContext or entity mapping. I also recognize that EF Core already has repository-like and Unit of Work behavior, so I do not add abstractions unless they make testing or boundaries clearer."

### Strong Answer — Adding Abstractions

> "I add an abstraction when it hides a real dependency, clarifies a boundary, or makes important behavior easier to test. I avoid adding interfaces just because a pattern says so. In Zapas, `IFitSessionParser` is useful because the Garmin SDK is an external detail. An interface around a simple helper method would probably be unnecessary."

### Behavioral Practice

Prepare short answers for:

- Tell me about yourself.
- Why are you exploring after 10 years?
- Tell me about a time you improved a system's architecture.
- Tell me about a time you disagreed about a technical direction.
- Tell me about a time you avoided over-engineering.
- Tell me about a production issue where architecture or boundaries mattered.
- How do you mentor less experienced developers on design decisions?

### "Architecture Improvement" Draft

> "One architecture improvement I made was separating responsibilities that had started to blur together. I look for places where UI, workflow, persistence, and external integration code are mixed, then create clearer boundaries with small interfaces or services. The goal is not to add layers for their own sake, but to make the system easier to test, change, and explain."

---

## Day 6 Deliverable

At the end of today, Zapas should have:

- A clear written explanation of its current architecture
- FIT parsing isolated behind `IFitSessionParser`
- `SessionService` focused on application workflow
- Domain rules such as pace calculation kept out of controllers and EF entities
- Explicit mapping between domain models, EF entities, and DTOs
- A defensible decision about staying as one project or later splitting into API/Application/Domain/Infrastructure projects
- Clear explanations for Clean Architecture, modular monoliths, repositories, CQRS, DDD basics, and microservices tradeoffs

Write notes for yourself:

- One boundary in Zapas that is currently clear
- One boundary that could become unclear as the app grows
- One abstraction that is justified and why
- One abstraction you deliberately did not add and why
- One architecture tradeoff you can explain clearly

---

## Final Interview Framing

Use this as your Day 6 summary:

> "I reviewed Zapas from an architecture perspective and kept it as a pragmatic modular monolith. Controllers handle HTTP, services handle workflows, domain models represent running sessions and intervals, repositories hide EF Core persistence, and the FIT SDK is isolated behind a parser abstraction. I can explain how this uses Clean Architecture ideas without over-engineering, and when I would consider CQRS, MediatR, separate projects, or microservices."
