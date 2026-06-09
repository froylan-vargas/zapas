# Day 10 - Final Mock Interview And Review

## Instructor Goal

Today you are simulating the real senior .NET interview from start to finish.

By the end of the day, you should be able to:

- Introduce your experience clearly
- Explain why you are interviewing after 10 years at one company
- Answer common senior .NET technical questions
- Solve one medium backend-style coding problem while talking
- Walk through a practical system design
- Connect answers back to production experience
- Ask senior-level questions to the interviewer
- Leave with a final cheat sheet for real interviews

The objective is not to be perfect. The objective is to sound prepared, calm, practical, and senior.

---

## 5-Hour Structure

| Activity | Time |
|---|---:|
| Hour 1 - Behavioral interview | 60 min |
| Hour 2 - Technical screen | 60 min |
| Hour 3 - Live coding | 60 min |
| Hour 4 - System design | 60 min |
| Hour 5 - Final review and cheat sheet | 60 min |

Main rule for today:

> Answer like someone who has maintained production software, not like someone reciting definitions.

---

## Mock Interview Template

Use this structure for every answer.

### Step 1 - Direct Answer

Start with the short answer first.

Say:

> "The short version is..."

### Step 2 - Explain The Why

Add the reasoning behind the answer.

Say:

> "The reason I care about this is..."

### Step 3 - Give A Concrete Example

Use Zapas or your real work experience.

Say:

> "In Zapas, an example is..."

### Step 4 - Mention Tradeoffs

Senior engineers do not pretend every answer is absolute.

Say:

> "The tradeoff is..."

### Step 5 - Close Cleanly

Stop before the answer becomes a lecture.

Say:

> "That is the approach I would start with, then I would measure and adjust."

---

# Hour 1 - Behavioral Interview

## Goal

Practice concise stories that show ownership, judgment, communication, and production maturity.

Use the STAR structure:

```text
Situation - What was happening?
Task      - What were you responsible for?
Action    - What did you do?
Result    - What changed because of it?
```

Keep most answers between 90 seconds and 2 minutes.

---

## Question 1 - Tell Me About Yourself

### Step-by-Step Guide

### Step 1 - Start With Role And Experience

Say:

> "I am a .NET web developer with 10 years of experience building and maintaining web applications."

### Step 2 - Frame Long Tenure Positively

Mention:

- Ownership
- Long-lived systems
- Business context
- Production support
- Maintainability

### Step 3 - Explain Current Direction

Mention:

- Modern ASP.NET Core
- EF Core
- Testing
- Architecture
- Cloud-native concepts

### Step 4 - Connect To The Role

Close with the kind of work you want next.

### Reference Answer

> "I am a .NET web developer with 10 years of experience building and maintaining web applications in the same company. That gave me deep ownership of long-lived systems, production support experience, business context, and a strong appreciation for maintainability. Recently I have been refreshing and sharpening my modern .NET skills with ASP.NET Core, EF Core, testing, authentication, observability, and system design. I built a practice API called Zapas that parses uploaded running FIT files and uses it to discuss clean API design, persistence, validation, and production tradeoffs. I am now looking for a role where I can combine that long-term ownership experience with a more modern engineering environment."

### What To Avoid

Do not say:

```text
I stayed too long.
I only worked at one company.
I do not know modern .NET.
```

Say instead:

> "Staying at one company gave me depth. Now I want to bring that experience into a broader modern engineering environment."

---

## Question 2 - Why Now After 10 Years?

### Step-by-Step Guide

### Step 1 - Do Not Apologize

The reason should sound intentional, not defensive.

### Step 2 - Mention Growth

Say you are looking for:

- Broader technical exposure
- Modern engineering practices
- New product or domain challenges
- More architecture ownership

### Step 3 - Keep It Positive

Do not criticize your current employer.

### Reference Answer

> "After 10 years, I have had the chance to own systems deeply and understand the long-term impact of technical decisions. That has been valuable. At this point, I am ready for broader technical exposure, a more modern .NET environment, and new product challenges. I am not leaving because I want to escape something. I am looking for the next place where I can keep growing and contribute with the production maturity I have built."

---

## Question 3 - Tell Me About A Production Issue

### Story Shape

```text
Situation:
  A user-facing issue affected reliability, performance, data quality, or operations.

Task:
  You needed to diagnose, communicate, and restore service.

Action:
  You checked logs, reproduced the issue, isolated the cause, fixed or mitigated it,
  and added prevention.

Result:
  The system recovered, and you improved monitoring, tests, validation, or process.
```

### Reference Answer

> "A strong production issue answer should show that I did not just patch the symptom. I would explain how I detected the issue, how I narrowed it down, what mitigation I used, and what I changed afterward to prevent it from repeating. For example, in an API like Zapas, if uploads started timing out, I would first check request latency, file sizes, parser errors, thread pool behavior, and database timing. A short-term mitigation might be rate limiting or upload size enforcement. A longer-term fix might be moving parsing to a background worker and adding metrics around parse duration and failure rates."

---

## Question 4 - Conflict With A Teammate

### Good Answer Pattern

```text
I disagreed about a technical approach.
I clarified the goal and constraints.
I listened to the other person's reasoning.
We compared tradeoffs.
We chose an approach based on maintainability, risk, and delivery.
```

### Reference Answer

> "When I disagree technically, I try to move the discussion from preference to tradeoffs. For example, if one person wants a complex architecture and another wants a simpler design, I would ask what problem we are solving now, what change we expect later, and what risk each option introduces. I am comfortable pushing for quality, but I want the final decision to be based on evidence and the system's needs, not personal style."

---

## Question 5 - Technical Decision You Regret

### Good Answer Pattern

```text
Own the decision.
Explain what you knew at the time.
Explain what changed.
Explain what you learned.
Explain what you do differently now.
```

### Reference Answer

> "One kind of decision I would be careful about now is letting business logic accumulate in controllers or UI event handlers because it feels faster early on. It can work for a small feature, but over time it becomes harder to test and change. What I do differently now is separate HTTP concerns from application workflow and persistence. In Zapas, for example, the controller delegates upload work to a service, FIT parsing is behind an interface, and persistence is behind a repository."

---

# Hour 2 - Technical Screen

## Goal

Answer common senior .NET questions clearly, with examples and tradeoffs.

Use this answer shape:

```text
Definition
Why it matters
Example
Tradeoff or pitfall
```

---

## Question 1 - Explain Dependency Injection Lifetimes

### Step-by-Step Answer

### Step 1 - Define The Lifetimes

```text
Singleton:
  One instance for the application lifetime.

Scoped:
  One instance per request scope.

Transient:
  New instance each time it is requested.
```

### Step 2 - Give Examples

```text
Singleton:
  Stateless configuration helpers, caches, shared clients that are safe to reuse.

Scoped:
  DbContext, repositories, services that participate in one request.

Transient:
  Lightweight stateless services.
```

### Step 3 - Mention Pitfalls

```text
Do not inject Scoped services into Singleton services.
Be careful with mutable state in Singleton services.
DbContext is usually Scoped.
```

### Reference Answer

> "Singleton lives for the whole application, Scoped lives for one request, and Transient is created each time it is requested. In ASP.NET Core, `DbContext` is usually Scoped because one request should get a consistent unit of work and EF Core contexts are not thread-safe. The main pitfall is putting request-specific state into a Singleton or injecting a Scoped service into a Singleton."

---

## Question 2 - Explain Async/Await

### Reference Answer

> "`async` and `await` let a method pause while waiting for I/O without blocking the request thread. In a web API, that matters because blocked threads reduce throughput. For database calls, HTTP calls, and file I/O, I prefer async APIs with cancellation tokens. For CPU-bound work, async does not magically make the work cheaper; I need to consider background processing, queues, or capacity limits."

### Zapas Example

```text
GET /sessions:
  EF Core async query is I/O-bound and should be awaited.

POST /sessions:
  File upload and database save are I/O-bound.
  FIT parsing may be CPU/file-processing work and might need guardrails.
```

---

## Question 3 - Explain ASP.NET Core Middleware

### Reference Answer

> "Middleware is the request pipeline. Each middleware can inspect the request, do work before the next component, call the next middleware, and then do work on the response. Order matters. For example, exception handling should be early, routing must happen before endpoint execution, authentication must run before authorization, and rate limiting should happen before expensive endpoint work."

### Good Pipeline Example

```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("ZapasFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
```

---

## Question 4 - Explain EF Core Tracking

### Reference Answer

> "EF Core tracks entities so it can detect changes and save updates. That is useful for write workflows, but it adds overhead for read-only queries. For API list or detail reads, I usually use `AsNoTracking` and project to DTOs. I avoid returning EF entities directly because that couples the API contract to persistence."

### Good Query Example

```csharp
var sessions = await dbContext.Sessions
    .AsNoTracking()
    .OrderByDescending(session => session.StartTime)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(session => new SessionSummaryDto(
        session.Id,
        session.Name,
        session.StartTime,
        session.DistanceMeters))
    .ToListAsync(cancellationToken);
```

---

## Question 5 - Explain API Versioning

### Reference Answer

> "API versioning is about changing contracts without breaking existing clients. Common approaches are URL versioning, query-string versioning, and header-based versioning. I prefer keeping versioning boring and explicit. I would version when the response shape or behavior changes incompatibly, not for every internal refactor."

### Example

```text
GET /v1/sessions/{id}
GET /v2/sessions/{id}
```

---

## Question 6 - Explain Caching

### Reference Answer

> "Caching stores expensive or frequently requested data so the system can avoid repeated work. The tradeoff is invalidation and freshness. In Zapas, caching `GET /sessions/{id}` can make sense because session data is mostly read after creation. I would set expiration, invalidate on delete or update, and track hit rate. For multiple API instances, I would consider a distributed cache."

---

## Question 7 - Explain Testing Strategy

### Reference Answer

> "I like tests at different levels. Unit tests cover business rules and validation. Integration tests cover the HTTP boundary, routing, middleware, authentication, and persistence. For Zapas, I would unit test upload validation and pace calculations, and integration test `GET /sessions`, not found responses, auth behavior, and invalid file upload responses."

---

## Question 8 - Explain Authentication Flow

### Reference Answer

> "Authentication establishes who the caller is. Authorization decides what that caller can do. In a JWT-based API, the identity provider issues a signed token, the client sends it as a bearer token, and the API validates issuer, audience, signature, and expiration. After that, policies and ownership checks decide access. For Zapas, users should only read their own sessions unless they have an admin permission."

---

## Question 9 - Explain Clean Architecture

### Reference Answer

> "Clean Architecture is about keeping business rules independent from frameworks and infrastructure. Controllers should not contain business logic, EF Core should not leak into the API contract, and external SDKs should be hidden behind interfaces when they matter to the domain. The tradeoff is that too much ceremony can slow down a small app, so I apply the boundaries proportionally."

---

# Hour 3 - Live Coding

## Goal

Solve one medium backend-style problem while talking.

Use the same template from Day 9:

1. Restate the problem
2. Ask clarifying questions
3. Explain approach
4. Write clean code
5. Test manually
6. Discuss complexity
7. Mention production tradeoffs

Pick one of these problems for the mock:

- Transaction-safe inventory update
- File parser
- Deduplication service

---

# Problem 1 - Transaction-Safe Inventory Update

## Prompt

Implement a service method that reserves inventory for an order.

Requirements:

- The caller provides an `orderId`, `productId`, and `quantity`.
- Quantity must be positive.
- Inventory cannot go below zero.
- The same `orderId` should not reserve inventory twice.
- The operation should be safe when two requests happen at the same time.

---

## Step-by-Step Guide

### Step 1 - Restate

Say:

> "I need to reserve stock exactly once per order, reject invalid quantities, prevent negative inventory, and handle concurrent requests safely."

### Step 2 - Clarify

Ask:

- Is this in memory or database-backed?
- Should duplicate order ids return success or conflict?
- Do we need to support releasing reservations?
- Is product id guaranteed to exist?

For this exercise, use:

```text
Database-backed EF Core service.
Duplicate order id is treated as idempotent success if the same reservation exists.
Missing product returns failure.
```

### Step 3 - Define Result Types

```csharp
public enum ReservationStatus
{
    Reserved,
    AlreadyReserved,
    InvalidQuantity,
    ProductNotFound,
    InsufficientInventory
}

public sealed record ReservationResult(
    ReservationStatus Status,
    Guid? ReservationId = null);
```

### Step 4 - Define Entities

```csharp
public sealed class ProductInventory
{
    public Guid ProductId { get; set; }
    public int AvailableQuantity { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public sealed class InventoryReservation
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

### Step 5 - Configure EF Core Concurrency

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<ProductInventory>()
        .HasKey(inventory => inventory.ProductId);

    modelBuilder.Entity<ProductInventory>()
        .Property(inventory => inventory.RowVersion)
        .IsRowVersion();

    modelBuilder.Entity<InventoryReservation>()
        .HasIndex(reservation => reservation.OrderId)
        .IsUnique();
}
```

### Step 6 - Solution

```csharp
public sealed class InventoryService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        AppDbContext dbContext,
        ILogger<InventoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ReservationResult> ReserveAsync(
        Guid orderId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        if (quantity <= 0)
        {
            return new ReservationResult(ReservationStatus.InvalidQuantity);
        }

        var existingReservation = await _dbContext.InventoryReservations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                reservation => reservation.OrderId == orderId,
                cancellationToken);

        if (existingReservation is not null)
        {
            return new ReservationResult(
                ReservationStatus.AlreadyReserved,
                existingReservation.Id);
        }

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var inventory = await _dbContext.ProductInventories
            .SingleOrDefaultAsync(
                item => item.ProductId == productId,
                cancellationToken);

        if (inventory is null)
        {
            return new ReservationResult(ReservationStatus.ProductNotFound);
        }

        if (inventory.AvailableQuantity < quantity)
        {
            return new ReservationResult(ReservationStatus.InsufficientInventory);
        }

        inventory.AvailableQuantity -= quantity;

        var reservation = new InventoryReservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.InventoryReservations.Add(reservation);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ReservationResult(
                ReservationStatus.Reserved,
                reservation.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(
                ex,
                "Inventory reservation concurrency conflict for order {OrderId}.",
                orderId);

            await transaction.RollbackAsync(cancellationToken);
            return new ReservationResult(ReservationStatus.InsufficientInventory);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(
                ex,
                "Inventory reservation failed for order {OrderId}.",
                orderId);

            await transaction.RollbackAsync(cancellationToken);

            var existingAfterConflict = await _dbContext.InventoryReservations
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    reservation => reservation.OrderId == orderId,
                    cancellationToken);

            if (existingAfterConflict is not null)
            {
                return new ReservationResult(
                    ReservationStatus.AlreadyReserved,
                    existingAfterConflict.Id);
            }

            throw;
        }
    }
}
```

Required usings:

```csharp
using Microsoft.EntityFrameworkCore;
```

### Manual Test Cases

```text
quantity = 0
  -> InvalidQuantity

missing product
  -> ProductNotFound

available = 10, reserve 3
  -> Reserved, available becomes 7

available = 2, reserve 3
  -> InsufficientInventory

same orderId submitted twice
  -> second call returns AlreadyReserved

two callers reserve the final unit at the same time
  -> one succeeds, one fails or retries based on policy
```

### Complexity

```text
Time: O(1) database lookups by indexed product id and order id.
Space: O(1) application memory.
```

### Production Notes

Mention:

- Database constraints are part of correctness.
- Application checks alone are not enough under concurrency.
- Use unique indexes for idempotency keys.
- Use row version, transactions, or database locks depending on isolation needs.
- Return clear API responses for conflicts and validation failures.

### Interview Explanation

> "The important part is that correctness depends on the database boundary, not only in-memory checks. I use a unique order id for idempotency and optimistic concurrency on inventory so two callers cannot both safely reserve the same stock."

---

# Problem 2 - File Parser

## Prompt

Parse lines from a simple CSV-like file containing running sessions.

Input format:

```text
sessionId,startTime,distanceMeters,durationSeconds
8f0f9e85-46db-4f0f-a41c-1aee62e19ac7,2026-05-01T10:00:00Z,5000,1500
```

Requirements:

- Skip the header row.
- Reject malformed rows.
- Parse `Guid`, `DateTimeOffset`, `double`, and `int`.
- Distance and duration must be positive.
- Return parsed records and row-level errors.

---

## Step-by-Step Guide

### Step 1 - Restate

Say:

> "I need to parse a text file into typed records while collecting row errors instead of crashing on the first bad row."

### Step 2 - Clarify

Ask:

- Is this real CSV with quoted commas?
- Should one bad row reject the whole file?
- Should line numbers be one-based?
- Can the file be large?

For this exercise:

```text
Simple comma split is acceptable.
Collect all row errors.
Line numbers are one-based.
```

### Step 3 - Define Models

```csharp
public sealed record ParsedRunSession(
    Guid SessionId,
    DateTimeOffset StartTime,
    double DistanceMeters,
    int DurationSeconds);

public sealed record ParseError(int LineNumber, string Message);

public sealed record ParseResult(
    IReadOnlyList<ParsedRunSession> Sessions,
    IReadOnlyList<ParseError> Errors);
```

### Step 4 - Solution

```csharp
public static ParseResult ParseSessions(IEnumerable<string> lines)
{
    ArgumentNullException.ThrowIfNull(lines);

    var sessions = new List<ParsedRunSession>();
    var errors = new List<ParseError>();

    var lineNumber = 0;

    foreach (var line in lines)
    {
        lineNumber++;

        if (lineNumber == 1)
        {
            continue;
        }

        if (string.IsNullOrWhiteSpace(line))
        {
            continue;
        }

        var columns = line.Split(',');

        if (columns.Length != 4)
        {
            errors.Add(new ParseError(
                lineNumber,
                "Expected 4 columns."));

            continue;
        }

        if (!Guid.TryParse(columns[0], out var sessionId))
        {
            errors.Add(new ParseError(lineNumber, "Invalid session id."));
            continue;
        }

        if (!DateTimeOffset.TryParse(columns[1], out var startTime))
        {
            errors.Add(new ParseError(lineNumber, "Invalid start time."));
            continue;
        }

        if (!double.TryParse(columns[2], out var distanceMeters)
            || distanceMeters <= 0)
        {
            errors.Add(new ParseError(lineNumber, "Invalid distance."));
            continue;
        }

        if (!int.TryParse(columns[3], out var durationSeconds)
            || durationSeconds <= 0)
        {
            errors.Add(new ParseError(lineNumber, "Invalid duration."));
            continue;
        }

        sessions.Add(new ParsedRunSession(
            sessionId,
            startTime,
            distanceMeters,
            durationSeconds));
    }

    return new ParseResult(sessions, errors);
}
```

### Manual Tests

```csharp
var result = ParseSessions([
    "sessionId,startTime,distanceMeters,durationSeconds",
    "8f0f9e85-46db-4f0f-a41c-1aee62e19ac7,2026-05-01T10:00:00Z,5000,1500",
    "bad-id,2026-05-01T10:00:00Z,5000,1500",
    "8f0f9e85-46db-4f0f-a41c-1aee62e19ac8,2026-05-01T10:00:00Z,-1,1500",
    ""
]);

Console.WriteLine(result.Sessions.Count); // 1
Console.WriteLine(result.Errors.Count);   // 2
```

### Complexity

```text
Time: O(n), where n is the number of lines.
Space: O(s + e), for parsed sessions and errors.
```

### Production Notes

Mention:

- Real CSV should use a CSV parser, not `Split(',')`.
- Large files should stream instead of loading all lines.
- File size and type must be validated before parsing.
- Error messages should avoid leaking sensitive file contents.
- Zapas uses the Garmin FIT SDK for binary FIT parsing, hidden behind `IFitSessionParser`.

### Interview Explanation

> "For the interview problem, simple splitting is enough because the format is constrained. In production, I would use a real parser for CSV and a domain-specific parser for FIT files. The important design point is returning row-level validation errors without mixing parsing logic into the controller."

---

# Problem 3 - Deduplication Service

## Prompt

Implement a service that processes events exactly once by id.

Requirements:

- Each event has an `EventId`.
- If the same event is seen again, it should be ignored.
- The service should keep only recent event ids for a configured time window.
- The implementation should be thread-safe.

---

## Step-by-Step Guide

### Step 1 - Restate

Say:

> "I need to detect duplicate event ids, allow the first occurrence, reject repeats, and expire old ids so memory does not grow forever."

### Step 2 - Clarify

Ask:

- Is in-memory storage acceptable?
- Does this need to work across multiple instances?
- What should happen after the dedup window expires?
- Should processing and marking as processed be atomic?

For this exercise:

```text
In-memory single-instance service.
First occurrence returns true.
Duplicates inside the window return false.
Ids can be accepted again after expiration.
```

### Step 3 - Choose Data Structures

Use:

- `Dictionary<Guid, DateTimeOffset>` for lookup
- `Queue<SeenEvent>` for cleanup order
- `lock` for thread safety

### Step 4 - Define Model

```csharp
public sealed record SeenEvent(Guid EventId, DateTimeOffset SeenAt);
```

### Step 5 - Solution

```csharp
public sealed class DeduplicationService
{
    private readonly TimeSpan _window;
    private readonly Dictionary<Guid, DateTimeOffset> _seenAtByEventId = new();
    private readonly Queue<SeenEvent> _seenEvents = new();
    private readonly object _gate = new();

    public DeduplicationService(TimeSpan window)
    {
        if (window <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(window),
                "Window must be greater than zero.");
        }

        _window = window;
    }

    public bool TryMarkFirstSeen(Guid eventId, DateTimeOffset now)
    {
        if (eventId == Guid.Empty)
        {
            throw new ArgumentException("Event id is required.", nameof(eventId));
        }

        lock (_gate)
        {
            RemoveExpired(now);

            if (_seenAtByEventId.ContainsKey(eventId))
            {
                return false;
            }

            _seenAtByEventId[eventId] = now;
            _seenEvents.Enqueue(new SeenEvent(eventId, now));

            return true;
        }
    }

    private void RemoveExpired(DateTimeOffset now)
    {
        var cutoff = now - _window;

        while (_seenEvents.Count > 0 && _seenEvents.Peek().SeenAt <= cutoff)
        {
            var expired = _seenEvents.Dequeue();

            if (_seenAtByEventId.TryGetValue(expired.EventId, out var currentSeenAt)
                && currentSeenAt == expired.SeenAt)
            {
                _seenAtByEventId.Remove(expired.EventId);
            }
        }
    }

    private sealed record SeenEvent(Guid EventId, DateTimeOffset SeenAt);
}
```

### Manual Tests

```csharp
var service = new DeduplicationService(TimeSpan.FromMinutes(5));
var eventId = Guid.NewGuid();
var start = DateTimeOffset.UtcNow;

Console.WriteLine(service.TryMarkFirstSeen(eventId, start));              // true
Console.WriteLine(service.TryMarkFirstSeen(eventId, start.AddMinutes(1))); // false
Console.WriteLine(service.TryMarkFirstSeen(eventId, start.AddMinutes(6))); // true
```

### Complexity

```text
Time:
  Usually O(1).
  Cleanup can remove multiple expired ids, but each id is enqueued and dequeued once.

Space:
  O(number of event ids inside the dedup window).
```

### Production Notes

Mention:

- In-memory deduplication does not work across multiple API instances.
- Durable deduplication should use a database unique key, Redis, or message broker support.
- Exactly-once processing is difficult; most systems aim for at-least-once delivery plus idempotent handlers.
- For Zapas import jobs, an idempotency key can prevent duplicate imports from repeated client retries.

### Interview Explanation

> "The practical senior answer is that exactly-once is usually achieved through idempotency, not magic. This service handles a single-instance in-memory version, but production deduplication should use a shared durable store or unique database constraint."

---

# Hour 4 - System Design

## Goal

Practice a structured, practical design conversation.

Recommended design for today:

```text
Running activity import and session analytics API
```

This lets you use Zapas as the example.

---

## System Design Template

Use this order:

1. Requirements
2. APIs
3. Data model
4. Architecture
5. Failure cases
6. Scaling
7. Security
8. Observability
9. Tradeoffs

Say:

> "I will start by clarifying requirements, then move through API shape, data model, architecture, failure handling, scaling, security, observability, and tradeoffs."

---

## Design - Running Activity Import And Analytics API

### Step 1 - Requirements

Functional requirements:

- Users upload `.fit` running activity files.
- The system validates file size and type.
- The system parses session metrics and active intervals.
- Users can list sessions.
- Users can get session details.
- Users can filter by date range.
- Users can see import status for background processing.

Non-functional requirements:

- Secure user-specific data.
- Avoid long request timeouts for large files.
- Keep API responses consistent.
- Support retries without duplicate sessions.
- Capture logs, metrics, and traces.
- Keep the design proportional to the product size.

Say:

> "I would start with a modular monolith and only split services if scale, team ownership, or deployment needs justify it."

---

### Step 2 - APIs

```http
POST /session-imports
Authorization: Bearer <token>
Content-Type: multipart/form-data

file=<activity.fit>
```

Response:

```json
{
  "id": "3adbe6c2-8e8a-4f7b-b7bd-5aa5966f6a53",
  "status": "Pending",
  "statusUrl": "/session-imports/3adbe6c2-8e8a-4f7b-b7bd-5aa5966f6a53"
}
```

```http
GET /session-imports/{id}
GET /sessions?page=1&pageSize=20&from=2026-01-01&to=2026-12-31
GET /sessions/{id}
DELETE /sessions/{id}
```

Status codes:

```text
200 OK
201 Created
202 Accepted
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
413 Payload Too Large
429 Too Many Requests
500 Internal Server Error
```

---

### Step 3 - Data Model

```text
Users
  Id
  ExternalSubjectId
  Email

Sessions
  Id
  OwnerUserId
  Name
  StartTime
  DistanceMeters
  DurationSeconds
  AveragePaceSecondsPerKm
  CreatedAt

RunIntervals
  Id
  SessionId
  StartTime
  EndTime
  DistanceMeters
  DurationSeconds
  AveragePaceSecondsPerKm

SessionImportJobs
  Id
  OwnerUserId
  OriginalFileName
  StoragePath
  Status
  SessionId
  Error
  AttemptCount
  CreatedAt
  ProcessedAt

IdempotencyKeys
  Key
  OwnerUserId
  ResourceId
  CreatedAt
```

Indexes:

```text
Sessions(OwnerUserId, StartTime)
RunIntervals(SessionId)
SessionImportJobs(OwnerUserId, CreatedAt)
IdempotencyKeys(OwnerUserId, Key) unique
```

---

### Step 4 - Architecture

```text
Client
  -> ASP.NET Core API
    -> Controller
      -> SessionImportService
        -> File storage
        -> SessionImportJobRepository
        -> Background queue or database-backed pending job

Background worker
  -> Finds pending import job
  -> Loads file
  -> IFitSessionParser
  -> SessionRepository
  -> Marks import completed or failed

Database
  -> Sessions
  -> RunIntervals
  -> Import jobs

Observability
  -> Logs
  -> Metrics
  -> Traces
```

Senior framing:

> "The controller stays thin. The service owns workflow. The FIT SDK is behind an interface. Persistence is behind repositories or query services. Background processing is introduced only when synchronous parsing is not good enough."

---

### Step 5 - Failure Cases

Discuss:

```text
Invalid file extension
Oversized file
Corrupted FIT file
Parser throws
Database save fails
API crashes after file upload but before job creation
Background worker crashes mid-parse
Duplicate client retry
User tries to read another user's session
```

Mitigations:

```text
Validate before parsing.
Use durable import job state.
Use idempotency keys.
Use retries with max attempts.
Make background handlers idempotent.
Store controlled error messages.
Log internal exception details.
Enforce owner checks in queries.
```

---

### Step 6 - Scaling

Start simple:

```text
Single ASP.NET Core API
Relational database
Local or cloud object storage
Hosted background worker
```

Scale when needed:

```text
Move files to object storage.
Use a real queue.
Run multiple workers.
Use distributed cache for hot reads.
Add database indexes.
Paginate all list endpoints.
Move expensive analytics to precomputed tables or background jobs.
```

Do not overclaim:

> "I would not start with microservices. I would first keep the system modular, measure bottlenecks, and split only when there is a clear operational or ownership reason."

---

### Step 7 - Security

Security decisions:

```text
JWT bearer authentication.
Policy-based authorization.
Owner-based session access.
Admin-only delete if needed.
Upload size limits.
Allowed file extensions.
Parser errors returned as controlled responses.
Secrets outside source control.
HTTPS in production.
CORS limited to known frontend origins.
Rate limit upload endpoints.
```

Strong phrasing:

> "Authentication proves who the user is. Authorization still has to check whether that user owns the specific session."

---

### Step 8 - Observability

Logs:

```text
Upload accepted
Upload rejected
Import job started
Import job completed
Import job failed
Parser exception
Authorization failure
```

Metrics:

```text
Request count
Request duration
Upload size
Parse duration
Import success count
Import failure count
Queue depth
Database query duration
Cache hit rate
```

Traces:

```text
POST /session-imports
  -> save file
  -> create import job

Background import
  -> load file
  -> parse FIT
  -> save session
  -> update job status
```

Strong phrasing:

> "For production troubleshooting, I need to correlate a request id or import job id across logs, metrics, and traces."

---

### Step 9 - Tradeoffs

Synchronous parsing:

```text
Pros:
  Simple.
  Immediate result.
  Easier error reporting.

Cons:
  Request can timeout.
  Expensive work runs on request path.
  Harder to retry safely.
```

Background parsing:

```text
Pros:
  Faster upload response.
  Retries are easier.
  Better isolation for expensive work.

Cons:
  More moving parts.
  Requires job status.
  Eventual consistency.
  More operational complexity.
```

Final system design close:

> "I would start with the simplest reliable design: a modular ASP.NET Core API, EF Core persistence, strong validation, owner-based authorization, and clear observability. I would move parsing to a background workflow when upload latency, reliability, or retry requirements justify the extra complexity."

---

# Hour 5 - Final Review And Cheat Sheet

## Goal

Create one concise document you can review before interviews.

---

## Final Cheat Sheet Template

Create or update your interview notes with:

```markdown
# Senior .NET Interview Cheat Sheet

## 10 Stories

1. Tell me about yourself
2. Why now after 10 years?
3. Production issue
4. Difficult bug
5. Conflict with teammate
6. System you improved
7. Legacy code refactor
8. Mentoring or helping another developer
9. Technical decision you regret
10. Time you balanced quality and delivery

## 20 Technical Topics

1. DI lifetimes
2. Middleware pipeline
3. Controllers vs Minimal APIs
4. Model binding and validation
5. Global exception handling
6. Async/await
7. Cancellation tokens
8. EF Core tracking
9. EF Core migrations
10. Projection vs Include
11. Pagination/filtering/sorting
12. Caching
13. Rate limiting
14. Logging
15. Metrics and tracing
16. Unit tests
17. Integration tests
18. JWT authentication
19. Policy-based authorization
20. Clean Architecture tradeoffs

## 5 Live-Coding Patterns

1. Dictionary lookup
2. Stack
3. Queue
4. Sorting and scanning
5. Sliding window

## 5 Architecture Patterns

1. Layered architecture
2. Modular monolith
3. Repository boundary
4. Background worker
5. Outbox/idempotency pattern

## 5 Questions To Ask Interviewer

1. What does success look like for this role in the first 6 months?
2. How is the engineering team structured?
3. What are the biggest technical challenges the team is facing?
4. How do you approach code reviews and technical decisions?
5. What is your current .NET version and deployment model?
```

---

## Final Self-Check

You are ready if you can answer these out loud:

- Explain an ASP.NET Core request from middleware to response.
- Explain DI lifetimes with examples.
- Explain EF Core tracking, projections, and performance pitfalls.
- Solve a medium coding problem while talking.
- Explain async/await and why blocking is bad.
- Discuss testing strategy.
- Walk through a system design in a structured way.
- Explain a production issue you solved.
- Explain how you work with legacy code.
- Ask senior-level questions to the interviewer.

---

# Common Day 10 Mistakes

## Mistake 1 - Giving Definitions Without Judgment

Bad:

```text
Scoped means one per request.
```

Better:

> "Scoped means one per request. That matters for `DbContext` because it gives one unit of work per request and avoids sharing a non-thread-safe context across requests."

---

## Mistake 2 - Over-Apologizing For Gaps

Bad:

```text
I have not used Kubernetes, so I am weak there.
```

Better:

> "I understand the role Kubernetes plays in deployment and orchestration, but I have not been deeply hands-on with it in production. I can reason about the app-level concerns clearly: health checks, configuration, logging, scaling, and graceful failure."

---

## Mistake 3 - Overbuilding The System Design

Bad:

```text
Start with many microservices, Kafka, Kubernetes, CQRS, and event sourcing.
```

Better:

> "I would start with a modular monolith and a relational database. If parsing volume or team ownership grows, I would introduce a queue and independent workers before considering service splits."

---

## Mistake 4 - Skipping Tradeoffs

Bad:

```text
Background jobs are better.
```

Better:

> "Background jobs are better when parsing is slow or retries matter. Synchronous processing is simpler and can be the right first version for small files."

---

## Mistake 5 - Ending Weakly

Bad:

```text
So yeah, that is it.
```

Better:

> "That is the design I would start with. It keeps the system simple, protects user data, gives us observability, and leaves clear paths to scale the expensive parsing workflow."

---

# Zapas 60-Second Walkthrough

Use this when asked about your practice project.

> "Zapas is an ASP.NET Core API for running activity analysis. It accepts uploaded `.fit` files, validates file size and type, parses the activity using the Garmin FIT SDK, extracts session metrics and active running intervals, stores the data, and exposes REST endpoints for session lookup. I use it as an interview project because it has real backend concerns: thin controllers, service-layer workflow, repository abstraction, EF Core persistence, DTOs, validation, global exception handling, caching, rate limiting, authentication, authorization, and possible background processing. The design starts as a modular monolith because that is the simplest reliable shape, and I would only add queues or separate services when latency, retries, or scale justify it."

---

# Final Interview Phrasing

Use this as your closing mindset:

> "I do not need to pretend I have used every tool deeply. I need to show that I can reason clearly, build maintainable .NET APIs, diagnose production issues, communicate tradeoffs, and learn the specific tools a team uses."
