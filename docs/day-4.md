# Day 4 — Async, Performance, Caching, Diagnostics

## Instructor Goal

Today you are practicing how to explain production-minded ASP.NET Core decisions using the Zapas FIT Session API.

By the end of the day, you should be able to explain:

- Why async I/O matters in ASP.NET Core
- How cancellation flows from HTTP requests into services and repositories
- Where caching helps and where it can create correctness problems
- How rate limiting protects expensive endpoints
- How health checks, logs, and metrics help production troubleshooting
- How to discuss slow APIs, timeouts, high CPU, memory growth, and thread pool starvation

The objective is not to make Zapas "enterprise scale." The objective is to make the API more resilient and easier to discuss in a senior interview.

---

## 5-Hour Structure

| Activity | Time |
|---|---:|
| Build/refactor code | 2 hours |
| Technical review | 1 hour |
| Live coding | 1 hour |
| Interview speaking practice | 1 hour |

Main rule for today:

> Measure first, optimize second, explain the tradeoff clearly.

---

## Hour 1-2 — Build / Refactor Code

### Target Area

Work inside the current Zapas API:

- `Program.cs`
- `Controllers/SessionsController.cs`
- `Services/ISessionService.cs`
- `Services/SessionService.cs`
- `Repositories/ISessionRepository.cs`
- `Repositories/SessionRepository.cs`
- `Middleware/GlobalExceptionMiddleware.cs`

Current good baseline:

- Controller actions are already async.
- EF Core queries already use `ToListAsync`, `FirstOrDefaultAsync`, and `SaveChangesAsync`.
- `CancellationToken` already flows through controller, service, and repository methods.
- Read queries already use `AsNoTracking()`.

Today you will improve the production posture around those foundations.

---

## Task 1 — Verify Async And Cancellation Flow

### What To Check

Confirm this flow exists:

```text
HTTP request aborted
  -> controller CancellationToken
  -> service method
  -> repository method
  -> EF Core async method
  -> database operation can be cancelled
```

Expected signatures:

```csharp
public async Task<IActionResult> GetSession(Guid id, CancellationToken cancellationToken)
```

```csharp
Task<Session?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken);
```

```csharp
await _dbContext.Sessions
    .AsNoTracking()
    .Include(x => x.Intervals)
    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
```

### Interview Explanation

> "CancellationToken lets the API stop doing work when the client disconnects or the request times out. It is especially useful around I/O work such as database calls. It does not magically stop every CPU-bound operation, but it gives cooperative cancellation points."

### Refactor If Needed

If any async method does not accept a token, add it and pass it down. Do not create new tokens in lower layers unless you are intentionally adding a timeout boundary.

---

## Task 2 — Add In-Memory Cache For `GET /sessions/{id}`

### Why This Endpoint

`GET /sessions/{id}` is a good caching candidate because:

- It retrieves one session by stable id.
- Sessions are append-only in the current API.
- The same session may be requested repeatedly after upload.

Do not cache `POST /sessions`. File parsing and upload behavior need correctness and validation more than caching.

### Implementation

Update `Program.cs`:

```csharp
builder.Services.AddMemoryCache();
```

Update `SessionService.cs`:

```csharp
using Microsoft.Extensions.Caching.Memory;
```

Add fields:

```csharp
private static readonly TimeSpan SessionCacheDuration = TimeSpan.FromMinutes(5);

private readonly IMemoryCache _cache;
private readonly ILogger<SessionService> _logger;
private readonly ISessionRepository _sessionRepository;
```

Update constructor:

```csharp
public SessionService(
    ISessionRepository sessionRepository,
    IMemoryCache cache,
    ILogger<SessionService> logger)
{
    _sessionRepository = sessionRepository;
    _cache = cache;
    _logger = logger;
}
```

Update `GetSessionByIdAsync`:

```csharp
public async Task<Session?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken)
{
    var cacheKey = $"session:{id}";

    if (_cache.TryGetValue(cacheKey, out Session? cachedSession))
    {
        _logger.LogInformation("Session cache hit for {SessionId}", id);
        return cachedSession;
    }

    _logger.LogInformation("Session cache miss for {SessionId}", id);

    var session = await _sessionRepository.GetSessionByIdAsync(id, cancellationToken);

    if (session is not null)
    {
        _cache.Set(
            cacheKey,
            session,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = SessionCacheDuration
            });
    }

    return session;
}
```

Update `CreateSessionAsync` after storing:

```csharp
var storedSession = await _sessionRepository.AddSessionAsync(session, cancellationToken);

_cache.Set(
    $"session:{storedSession.Id}",
    storedSession,
    new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = SessionCacheDuration
    });
```

### Interview Explanation

> "I cache session lookups because an individual session is stable after creation in this API. I use a short absolute expiration to limit stale data and memory growth. If sessions became editable or deletable, I would add explicit cache invalidation."

### Tradeoff To Say Out Loud

`IMemoryCache` is per application instance. In a multi-instance deployment, each instance has its own cache. For shared cache state, use a distributed cache such as Redis.

---

## Task 3 — Add Rate Limiting For Uploads

### Why Uploads Need Protection

`POST /sessions` is more expensive than reads:

- It accepts multipart input.
- It opens and parses a FIT file.
- It writes to the database.
- Bad clients can cause memory, CPU, and latency problems.

### Implementation

Update `Program.cs`:

```csharp
using System.Threading.RateLimiting;
```

Register rate limiting:

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("session-upload", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});
```

Add middleware before `MapControllers()`:

```csharp
app.UseRateLimiter();
```

Update `SessionsController.cs`:

```csharp
using Microsoft.AspNetCore.RateLimiting;
```

Add the attribute to the upload action:

```csharp
[EnableRateLimiting("session-upload")]
[HttpPost]
[Consumes("multipart/form-data")]
public async Task<IActionResult> CreateSession(IFormFile? file, CancellationToken cancellationToken)
```

### Interview Explanation

> "I rate limit uploads because they are more expensive than reads. A fixed window policy is simple and good enough for this practice API. In production I would tune limits using real traffic data and probably key by authenticated user rather than IP address."

---

## Task 4 — Add Health Checks

### Implementation

Update `Program.cs`:

```csharp
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ZapasDbContext>();
```

Map the endpoint:

```csharp
app.MapHealthChecks("/health");
```

### Interview Explanation

> "A health check gives infrastructure a lightweight way to know whether the app can respond and whether important dependencies such as the database are reachable."

### What Not To Overstate

A health check does not prove the whole business workflow works. It is a signal, not a full integration test.

---

## Task 5 — Add File Upload Guardrails

### Controller-Level Limit

Add a per-action request size limit:

```csharp
[RequestSizeLimit(10 * 1024 * 1024)]
[EnableRateLimiting("session-upload")]
[HttpPost]
[Consumes("multipart/form-data")]
public async Task<IActionResult> CreateSession(IFormFile? file, CancellationToken cancellationToken)
```

### Service-Level Validation

Keep a matching service guard so the rule is still enforced even if another caller uses the service:

```csharp
private const long MaxFitFileSizeBytes = 10 * 1024 * 1024;
```

Inside `CreateSessionAsync`:

```csharp
if (fileLength > MaxFitFileSizeBytes)
{
    return new CreateSessionResult(
        CreateSessionState.Rejected,
        Session: null,
        Error: "The uploaded file is too large.");
}
```

### Interview Explanation

> "I protect file uploads at the HTTP boundary and in the application workflow. The HTTP limit rejects oversized requests early, while the service rule keeps the business constraint explicit."

---

## Task 6 — Improve Request Diagnostics

### Add Lightweight Request Logging

If you want simple request timing without adding new packages, create `Middleware/RequestLoggingMiddleware.cs`:

```csharp
using System.Diagnostics;

namespace Zapas.Api.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

Register it in `Program.cs` after exception handling:

```csharp
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
```

### Interview Explanation

> "Request duration logs help identify slow endpoints and correlate user-facing latency with server behavior. For deeper diagnosis, I would add metrics and distributed tracing."

---

## Task 7 — Add Timeout Guardrail Around FIT Parsing

### Important Context

The Garmin FIT SDK parsing call is synchronous. You cannot make that truly async just by wrapping it in `Task.Run`. In ASP.NET Core, using `Task.Run` for request work can hide CPU pressure and increase thread pool contention.

For this practice API, keep parsing synchronous but add guardrails:

- File size limit
- Rate limit
- Cancellation before and after parsing
- Structured logs around parse duration

### Implementation

In `CreateSessionAsync`:

```csharp
cancellationToken.ThrowIfCancellationRequested();

var parseStartedAt = TimeProvider.System.GetTimestamp();

try
{
    session = ExtractSessionInfo(fitStream, fileName);
}
catch
{
    return new CreateSessionResult(
        CreateSessionState.Failed,
        Session: null,
        Error: "The uploaded file could not be parsed as a valid FIT activity.");
}
finally
{
    var elapsed = TimeProvider.System.GetElapsedTime(parseStartedAt);

    _logger.LogInformation(
        "FIT parsing completed in {ElapsedMilliseconds} ms for {FileName}",
        elapsed.TotalMilliseconds,
        fileName);
}

cancellationToken.ThrowIfCancellationRequested();
```

### Interview Explanation

> "FIT parsing is file-processing work, not database I/O. I would first protect the endpoint with size limits, rate limits, and duration logging. If parsing became expensive, I would move it to a background job so upload requests are not tied to CPU-heavy parsing."

---

## Hour 3 — Technical Review

### Async I/O

Say this clearly:

> "Async does not make work faster by itself. It lets the server release the request thread while waiting for I/O, so the app can handle more concurrent requests."

Good async candidates:

- Database calls
- HTTP calls
- File reads that support async APIs
- Queue or cache network calls

Poor async candidates:

- Pure CPU calculations
- Small in-memory transformations
- Synchronous third-party SDK calls with no async API

### Blocking Calls

Avoid this in ASP.NET Core:

```csharp
var session = _sessionService.GetSessionByIdAsync(id, cancellationToken).Result;
```

Also avoid:

```csharp
_sessionService.GetSessionByIdAsync(id, cancellationToken).Wait();
```

Interview explanation:

> "Blocking on async work ties up request threads and can contribute to thread pool starvation under load."

### `ConfigureAwait`

For ASP.NET Core:

> "ASP.NET Core does not have the old ASP.NET synchronization context, so `ConfigureAwait(false)` is usually not required in application code. It can still be common in reusable libraries."

### Caching

Cache when:

- Data is expensive to compute or fetch
- Data is read often
- Slight staleness is acceptable
- You have an invalidation strategy

Do not cache when:

- Data changes frequently and correctness matters
- The result is user-specific and easy to leak
- The memory footprint is unbounded
- You cannot explain invalidation

### Diagnostics Ladder

When an API is slow:

1. Check request logs and p95/p99 latency.
2. Identify the slow endpoint.
3. Check database query duration and generated SQL.
4. Look for missing indexes, unbounded queries, and N+1 behavior.
5. Check CPU, memory, GC, and thread pool counters.
6. Add targeted tracing around suspicious work.
7. Optimize the measured bottleneck.

---

## Hour 4 — Live Coding

Spend one hour coding without looking up full solutions. After each exercise, explain complexity and production tradeoffs out loud.

### Exercise 1 — Simple Expiring Cache

Write an in-memory cache for strings with absolute expiration.

Starter:

```csharp
public sealed class ExpiringCache
{
    public void Set(string key, string value, TimeSpan ttl)
    {
    }

    public string? Get(string key)
    {
    }
}
```

Implementation:

```csharp
public sealed class ExpiringCache
{
    private readonly Dictionary<string, CacheEntry> _entries = new();

    public void Set(string key, string value, TimeSpan ttl)
    {
        _entries[key] = new CacheEntry(value, DateTimeOffset.UtcNow.Add(ttl));
    }

    public string? Get(string key)
    {
        if (!_entries.TryGetValue(key, out var entry))
        {
            return null;
        }

        if (entry.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _entries.Remove(key);
            return null;
        }

        return entry.Value;
    }

    private sealed record CacheEntry(string Value, DateTimeOffset ExpiresAt);
}
```

Follow-up discussion:

- This is not thread-safe.
- It has no max size.
- Expired entries are only removed when accessed.
- Production code should use `IMemoryCache` or a distributed cache.

### Exercise 2 — Cancellation-Aware Loop

Write a method that processes items and cooperatively supports cancellation.

Starter:

```csharp
public static int CountValidItems(IEnumerable<string> items, CancellationToken cancellationToken)
{
}
```

Implementation:

```csharp
public static int CountValidItems(IEnumerable<string> items, CancellationToken cancellationToken)
{
    var count = 0;

    foreach (var item in items)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrWhiteSpace(item))
        {
            count++;
        }
    }

    return count;
}
```

Interview explanation:

> "Cancellation is cooperative. The code has to check the token at reasonable points and stop cleanly."

### Exercise 3 — Fixed Window Rate Limiter

Write a simple fixed window limiter for one caller.

Starter:

```csharp
public sealed class FixedWindowLimiter
{
    public FixedWindowLimiter(int permitLimit, TimeSpan window)
    {
    }

    public bool AllowRequest()
    {
    }
}
```

Implementation:

```csharp
public sealed class FixedWindowLimiter
{
    private readonly int _permitLimit;
    private readonly TimeSpan _window;
    private DateTimeOffset _windowStartedAt = DateTimeOffset.UtcNow;
    private int _requestCount;

    public FixedWindowLimiter(int permitLimit, TimeSpan window)
    {
        _permitLimit = permitLimit;
        _window = window;
    }

    public bool AllowRequest()
    {
        var now = DateTimeOffset.UtcNow;

        if (now - _windowStartedAt >= _window)
        {
            _windowStartedAt = now;
            _requestCount = 0;
        }

        if (_requestCount >= _permitLimit)
        {
            return false;
        }

        _requestCount++;
        return true;
    }
}
```

Follow-up discussion:

- Fixed windows are simple but can allow bursts at window boundaries.
- Sliding window and token bucket limiters smooth traffic better.
- Real APIs should use ASP.NET Core rate limiting or gateway-level limits.

### Exercise 4 — Slow Endpoint Diagnosis

Given this code:

```csharp
var sessions = await _dbContext.Sessions
    .Include(x => x.Intervals)
    .ToListAsync(cancellationToken);

return sessions
    .Where(x => x.StartTime >= from)
    .Select(x => new SessionSummaryDto(...))
    .ToList();
```

Identify the problems:

- It loads every session before filtering.
- It loads intervals even if the summary does not need them.
- It filters in memory instead of SQL.
- It may allocate too much memory.
- It has no pagination.

Better shape:

```csharp
var sessions = await _dbContext.Sessions
    .AsNoTracking()
    .Where(x => x.StartTime >= from)
    .OrderByDescending(x => x.StartTime)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(x => new SessionSummaryDto(...))
    .ToListAsync(cancellationToken);
```

Interview explanation:

> "I want filtering, sorting, pagination, and projection to happen in the database, not after loading unnecessary rows into application memory."

---

## Hour 5 — Interview Speaking Practice

Answer these out loud. Keep each answer under 2 minutes.

### Questions

1. Why is blocking bad in ASP.NET Core?
2. What is the difference between CPU-bound and I/O-bound work?
3. What does a `CancellationToken` actually do?
4. How would you improve slow API performance?
5. How do you detect memory leaks?
6. When would you cache a response?
7. What are the risks of caching?
8. How would you protect a file upload endpoint?
9. How would you diagnose high p95 latency?
10. What is thread pool starvation?
11. What is the difference between memory cache and distributed cache?
12. What would OpenTelemetry add beyond logs?

### Strong Answer — Slow API

> "I would start by measuring instead of guessing. I would check endpoint latency, p95 and p99, logs, traces, database query duration, generated SQL, and infrastructure metrics like CPU, memory, GC, and thread pool usage. Then I would optimize the measured bottleneck. Common fixes are adding pagination, using projections, adding indexes, removing N+1 queries, caching stable reads, and moving expensive CPU work to background processing."

### Strong Answer — Caching

> "Caching helps when data is expensive to retrieve or compute and the same result is requested often. The risks are stale data, memory growth, cache stampede, and incorrect sharing of user-specific data. I would cache stable session lookups with a short expiration and add explicit invalidation if sessions became editable."

### Strong Answer — Upload Protection

> "For uploads, I would validate extension and content expectations, enforce file size limits, rate limit expensive endpoints, log validation failures, avoid loading huge files into memory, and consider background processing if parsing becomes slow. I would also make sure errors are safe and do not leak internal parser details."

---

## Day 4 Deliverable

At the end of today, Zapas should have:

- Confirmed async EF Core methods with cancellation tokens
- `IMemoryCache` for `GET /sessions/{id}`
- Rate limiting for `POST /sessions`
- `/health` endpoint
- File upload size guardrails
- Request duration logging
- Clear explanations for caching, cancellation, diagnostics, and performance tradeoffs

Write notes for yourself:

- One performance improvement I made
- One production risk I reduced
- One tradeoff I can explain clearly
- One thing I would defer until the system actually needs it

---

## Final Interview Framing

Use this as your Day 4 summary:

> "I improved Zapas with production-oriented guardrails: cancellation-aware async database access, short-lived caching for stable session reads, rate limiting for expensive uploads, health checks, request timing logs, and file size limits. I can explain why each change exists, what problem it solves, and what tradeoff it introduces."
