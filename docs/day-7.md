# Day 7 — Distributed Systems, Background Processing, Cloud-Native Basics

## Instructor Goal

Today you are practicing how to discuss distributed-system concepts like a senior .NET engineer without overbuilding Zapas.

By the end of the day, you should be able to explain:

- Why synchronous processing is simpler but can become fragile for slow work
- When to move FIT parsing into a background workflow
- What message queues, pub/sub, retries, idempotency, and the Outbox pattern solve
- How eventual consistency changes API design
- How hosted services work in ASP.NET Core
- How Docker, health checks, observability, and .NET Aspire fit into modern backend systems
- How to describe cloud-native tradeoffs without pretending every app needs microservices

The objective is not to turn Zapas into a distributed platform. The objective is to understand the problems distributed systems introduce and to practice a small, realistic background-processing design.

---

## 5-Hour Structure

| Activity | Time |
|---|---:|
| Build/refactor code | 2 hours |
| Technical review | 1 hour |
| Live coding | 1 hour |
| Interview speaking practice | 1 hour |

Main rule for today:

> In distributed systems, assume failures, retries, duplicates, latency, and partial success.

---

## Hour 1-2 — Build / Refactor Code

### Target Area

Work inside the current Zapas API:

- `Zapas.Api/Controllers/SessionsController.cs`
- `Zapas.Api/Services/Sessions/ISessionService.cs`
- `Zapas.Api/Services/Sessions/SessionService.cs`
- `Zapas.Api/Services/FitParser/IFitSessionParser.cs`
- `Zapas.Api/Services/FitParser/FitSessionParser.cs`
- `Zapas.Api/Repositories/ISessionRepository.cs`
- `Zapas.Api/Repositories/SessionRepository.cs`
- `Zapas.Api/Data/ZapasDbContext.cs`
- `Zapas.Api/Entities/`
- `Zapas.Api/Models/`
- `Zapas.Api/DTOs/`
- `Zapas.Api/Program.cs`

Current good baseline:

- `POST /sessions` accepts a `.fit` file.
- The controller is thin and delegates to `ISessionService`.
- `SessionService` validates the file and coordinates parsing and persistence.
- FIT SDK usage is hidden behind `IFitSessionParser`.
- EF Core persistence is hidden behind `ISessionRepository`.
- Health checks, request logging, rate limiting, and global exception middleware already exist.

Today you will design or optionally implement a background import workflow. If the current code is not ready for the full implementation, write the design and implement the smallest safe slice.

---

## Task 1 — Explain The Current Synchronous Workflow

### Why

Before adding background processing, be able to defend the current design.

Current flow:

```text
POST /sessions
  -> SessionsController receives multipart/form-data
  -> SessionService validates file name, size, extension, and cancellation
  -> IFitSessionParser parses the FIT stream
  -> ISessionRepository stores the parsed Session and RunIntervals
  -> Controller returns 201 Created or a validation error
```

This is a good starting point because:

- It is simple.
- The client gets the result immediately.
- There is no job state to track.
- There is no background worker to operate.
- There are fewer failure modes.

### Interview Explanation

> "I would start with synchronous parsing if files are small and parsing is fast enough. It keeps the API simple and makes failures easy to report. I would move to background processing when parsing becomes slow, files become large, retries matter, or I need the upload endpoint to remain responsive under load."

### What To Avoid

Do not say background processing is automatically better. It adds job state, polling, retries, cleanup, and eventual consistency.

---

## Task 2 — Design An Import Job Model

### Why

A background workflow needs durable state. The API should not just enqueue invisible work and hope it succeeds.

Possible domain model:

```csharp
public sealed class SessionImportJob
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public SessionImportStatus Status { get; private set; }
    public Guid? SessionId { get; private set; }
    public string? Error { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public int AttemptCount { get; private set; }
}
```

Possible status enum:

```csharp
public enum SessionImportStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
```

Possible EF entity:

```csharp
public sealed class SessionImportJobEntity
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public int AttemptCount { get; set; }
}
```

### Design Decision

For Zapas, an import job can represent:

- The uploaded file metadata
- Where the temporary file is stored
- Whether parsing has started
- Whether parsing succeeded
- The created session id
- The failure reason if parsing failed
- The number of processing attempts

### Interview Explanation

> "A background workflow needs a durable job record so the system can recover if the process restarts. Without that, a request could accept a file and then lose the work before it is parsed."

---

## Task 3 — Design The Background Import Flow

### Target Flow

```text
POST /session-imports
  -> validate upload
  -> store the file temporarily or in durable storage
  -> create SessionImportJob with Pending status
  -> return 202 Accepted with job id

Background worker
  -> find Pending jobs
  -> mark one job Processing
  -> parse FIT file through IFitSessionParser
  -> save Session through ISessionRepository
  -> mark job Completed with SessionId
  -> on failure, mark job Failed or leave Pending for retry

GET /session-imports/{id}
  -> return job status
  -> include SessionId when completed
```

### Possible API Responses

Upload response:

```json
{
  "id": "b59f416b-e848-4b69-b268-50f2f3f3f84d",
  "status": "Pending",
  "statusUrl": "/session-imports/b59f416b-e848-4b69-b268-50f2f3f3f84d"
}
```

Completed status response:

```json
{
  "id": "b59f416b-e848-4b69-b268-50f2f3f3f84d",
  "status": "Completed",
  "sessionId": "1fd2262a-b174-46dd-9efe-0f89d8143f54"
}
```

Failed status response:

```json
{
  "id": "b59f416b-e848-4b69-b268-50f2f3f3f84d",
  "status": "Failed",
  "error": "The uploaded FIT file could not be parsed."
}
```

### HTTP Status Codes

Use:

- `202 Accepted` when the upload creates an import job but parsing is not done yet.
- `200 OK` when returning import job status.
- `400 Bad Request` for invalid upload shape or file validation failures.
- `404 Not Found` when an import job id does not exist.
- `422 Unprocessable Entity` if the file shape is accepted but parsing fails later.

### Interview Explanation

> "For background processing, I would return `202 Accepted` instead of `201 Created` because the session does not exist yet. The client gets a job id and polls a status endpoint until the job completes or fails."

---

## Task 4 — Add A Hosted Service Boundary

### Why

ASP.NET Core hosted services are a practical way to run background work in the same process for a small app.

Possible interface:

```csharp
public interface ISessionImportJobRepository
{
    Task<SessionImportJob?> DequeuePendingJobAsync(CancellationToken cancellationToken);
    Task MarkCompletedAsync(Guid jobId, Guid sessionId, CancellationToken cancellationToken);
    Task MarkFailedAsync(Guid jobId, string error, CancellationToken cancellationToken);
}
```

Possible hosted service shape:

```csharp
public sealed class SessionImportWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionImportWorker> _logger;

    public SessionImportWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<SessionImportWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var processor = scope.ServiceProvider
                .GetRequiredService<ISessionImportProcessor>();

            await processor.ProcessNextAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

Register it in `Program.cs`:

```csharp
builder.Services.AddHostedService<SessionImportWorker>();
```

### Why Use `IServiceScopeFactory`

Hosted services are singletons. EF Core `DbContext`, repositories, and services are usually scoped.

The worker should create a scope for each processing loop so scoped dependencies are resolved correctly.

### Interview Explanation

> "A hosted service is a good first step when I need background work but do not yet need a separate worker process or external queue. I create a scope inside the worker because my repositories and DbContext are scoped services."

### Tradeoff

In-process background workers are simple, but they have limits:

- If the API process stops, processing stops.
- Multiple app instances need coordination to avoid duplicate work.
- Long-running work competes with API traffic.
- A real queue may be better once reliability and scale matter.

---

## Task 5 — Practice The Outbox Pattern

### Problem

This code has a reliability gap:

```csharp
await _repository.SaveSessionAsync(session, cancellationToken);
await _messageBus.PublishAsync(new SessionCreated(session.Id), cancellationToken);
```

If the database save succeeds but message publishing fails, the system has saved the session but lost the event.

### Outbox Solution

Save the state change and the message record in the same database transaction:

```text
Transaction
  -> insert Session
  -> insert OutboxMessage(SessionCreated)
  -> commit

Background publisher
  -> read unpublished OutboxMessage records
  -> publish messages
  -> mark messages as published
```

Possible outbox entity:

```csharp
public sealed class OutboxMessageEntity
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
}
```

### In Zapas

You do not need RabbitMQ today. Use the concept:

- Import job record acts as durable work state.
- If adding events, persist outbox messages in the same database as the session.
- A background publisher can later send them to a queue or event bus.

### Interview Explanation

> "The Outbox pattern solves the problem of updating the database and publishing a message atomically. Instead of publishing directly inside the request, I write an outbox row in the same transaction and let a background process publish it later."

---

## Task 6 — Make Processing Idempotent

### Why

Retries are normal in distributed systems. Retried work must not create duplicate sessions or corrupt state.

Risks in Zapas:

- The same upload is submitted twice.
- A worker parses a job, saves the session, then crashes before marking the job completed.
- A retry processes the same job again.
- Two workers pick up the same pending job.

Possible protections:

- Give each import job a stable id.
- Store the resulting `SessionId` on the job.
- Do not process jobs already marked `Completed`.
- Use a database transaction when marking a job `Processing`.
- Use optimistic concurrency or a conditional update to claim one pending job.
- Consider a file hash or idempotency key for duplicate uploads.

Possible claim logic:

```text
Find one Pending job
Update it to Processing only if Status is still Pending
If update count is 1, this worker owns it
If update count is 0, another worker claimed it
```

### Interview Explanation

> "Retries can turn one logical operation into multiple physical attempts. I make handlers idempotent by using stable identifiers, checking current state before changing it, and designing duplicate processing to produce the same final result."

---

## Task 7 — Add Observability To The Workflow

### Why

Background work is harder to debug than request/response code. You need logs, metrics, and health signals.

Add or plan structured logs for:

- Import job created
- Import job claimed
- FIT parsing started
- FIT parsing completed
- Import job failed
- Retry attempt count
- Processing duration

Example logging:

```csharp
_logger.LogInformation(
    "Processing session import job {ImportJobId} for file {FileName}",
    job.Id,
    job.FileName);
```

Track useful metrics:

- Number of pending jobs
- Number of failed jobs
- Average processing duration
- Parse failures by reason
- Queue age of oldest pending job
- Upload request latency

Health check ideas:

- Database connectivity
- Whether failed jobs exceed a threshold
- Whether oldest pending job is too old
- Whether background worker is making progress

### Interview Explanation

> "In distributed or asynchronous workflows, observability becomes part of the design. I want enough structured logs, metrics, and health checks to answer what happened to a specific job and whether the system is falling behind."

---

## Hour 3 — Technical Review

### REST

REST is an architectural style centered around resources, representations, HTTP methods, and status codes.

For Zapas:

- `GET /sessions` lists session resources.
- `GET /sessions/{id}` returns one session resource.
- `POST /sessions` creates a session synchronously.
- `POST /session-imports` can create an import job asynchronously.
- `GET /session-imports/{id}` can return import job status.

Good answer:

> "I try to model HTTP APIs around resources and state transitions. For asynchronous work, the resource may be a job first, and the final domain resource may appear later."

### gRPC

gRPC is a contract-first RPC framework using Protocol Buffers and HTTP/2.

Useful when:

- Service-to-service calls need strong contracts.
- Performance matters.
- Streaming is useful.
- Both client and server can support gRPC well.

Less ideal when:

- Browser compatibility matters without extra gateway support.
- Public API consumers expect plain JSON over HTTP.
- Human readability and simple debugging are priorities.

Zapas answer:

> "For a public or simple client-facing API, I would keep REST. I would consider gRPC for internal service-to-service calls if Zapas were split into separate services and needed efficient typed communication."

### Message Queues

A queue stores messages until consumers process them.

Queues help with:

- Decoupling producers and consumers
- Smoothing traffic spikes
- Retrying failed work
- Moving slow work out of request paths

Queues add:

- Operational complexity
- Duplicate message handling
- Ordering questions
- Dead-letter queues
- Monitoring requirements

Zapas answer:

> "A queue would make sense if FIT parsing became slow or needed to run in separate workers. The upload API could enqueue an import job, and workers could process jobs independently."

### Pub/Sub

Pub/sub lets one event be consumed by multiple subscribers.

Example:

```text
SessionCreated event
  -> analytics subscriber updates training summaries
  -> notification subscriber sends a message
  -> search subscriber indexes the session
```

Good answer:

> "I use queues for work distribution and pub/sub when multiple independent consumers need to react to the same event."

### Outbox Pattern

Core problem:

```text
Database update succeeds
Message publish fails
System state and events are now inconsistent
```

Good answer:

> "The Outbox pattern stores the message in the same transaction as the database change. A separate publisher later reads and publishes those messages, which makes the database update and event creation reliable together."

### Idempotency

Idempotency means the same operation can be applied more than once without changing the result beyond the first successful application.

Examples:

- `GET /sessions/{id}` is naturally idempotent.
- Retrying an import job should not create multiple sessions for the same job.
- A message handler should ignore an event it has already processed.

Good answer:

> "Whenever retries are possible, I design the handler to tolerate duplicates. That usually means stable ids, uniqueness constraints, status checks, and storing enough processing state to know what has already happened."

### Retries

Retries help with transient failures:

- Temporary network failure
- Database timeout
- Queue publish timeout
- External dependency unavailable for a short time

Retries can hurt when:

- The operation is not idempotent
- The failure is permanent
- Many callers retry at once
- Backoff and limits are missing

Good answer:

> "Retries are useful for transient failures, but they need limits, backoff, and idempotency. Otherwise retries can amplify an outage or create duplicate work."

### Circuit Breaker

A circuit breaker stops calling a dependency that is already failing.

States:

- Closed: calls flow normally.
- Open: calls fail fast.
- Half-open: limited test calls check if the dependency recovered.

For Zapas:

- A circuit breaker could protect calls to external storage, a remote parser, or a third-party API.
- It is less relevant while parsing happens locally and persistence is local SQLite.

Good answer:

> "A circuit breaker protects the system from repeatedly waiting on a failing dependency. It gives the dependency time to recover and prevents request threads from piling up."

### Eventual Consistency

Eventual consistency means different parts of the system may temporarily disagree, but converge later.

In Zapas:

- Upload returns an import job immediately.
- The session does not exist yet.
- Later, the worker creates the session.
- The job status eventually becomes `Completed`.

Good answer:

> "With eventual consistency, the API should make pending state explicit. I would return a job id and status endpoint instead of pretending the final resource exists immediately."

### Background Services

`BackgroundService` is a base class for long-running hosted services in ASP.NET Core.

Key points:

- Registered with `AddHostedService`.
- Runs when the application starts.
- Should honor cancellation.
- Should handle exceptions carefully.
- Should create scopes for scoped services.

Good answer:

> "Hosted services are useful for lightweight background work in the same app. If the workload grows, I would consider moving the worker into a separate process and using a real queue."

### Docker Basics

Know the core terms:

- Image: packaged application and runtime dependencies.
- Container: running instance of an image.
- Dockerfile: instructions to build an image.
- Compose: local multi-container orchestration.
- Registry: place where images are stored.

For Zapas, Docker could run:

- API container
- Database container
- Optional worker container
- Optional observability tools

Good answer:

> "Docker gives me repeatable packaging and local environments. It does not automatically solve deployment, scaling, or observability, but it gives a consistent unit to run."

### Observability

Observability is the ability to understand system behavior from outputs.

Main signals:

- Logs: discrete events with context
- Metrics: numeric measurements over time
- Traces: request flow across components

For Zapas:

- Logs explain what happened to one upload or import job.
- Metrics show throughput, failures, and queue age.
- Traces help follow a request through API, database, parser, and worker.

Good answer:

> "I want logs for diagnosis, metrics for trends and alerts, and traces for understanding latency across components."

### .NET Aspire

.NET Aspire is an opinionated stack for building, running, and observing distributed .NET apps locally and in deployment-oriented environments.

Useful ideas:

- App host orchestration
- Service discovery
- Configuration and connection management
- Health checks
- Observability defaults
- Local dashboard

Zapas answer:

> "I would understand Aspire as a way to improve local development and observability for distributed .NET apps. I would not introduce it just for one simple API, but it becomes interesting if Zapas grows into API, worker, database, cache, and queue components."

---

## Hour 4 — Live Coding

Practice these out loud. Focus on naming failure modes and tradeoffs while coding.

### Exercise 1 — Model An Import Job

Create a simple import job model:

```csharp
public enum ImportStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public sealed class ImportJob
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public ImportStatus Status { get; private set; } = ImportStatus.Pending;
    public Guid? SessionId { get; private set; }
    public string? Error { get; private set; }
    public int AttemptCount { get; private set; }

    public void MarkProcessing()
    {
        if (Status is ImportStatus.Completed)
        {
            return;
        }

        Status = ImportStatus.Processing;
        AttemptCount++;
    }

    public void MarkCompleted(Guid sessionId)
    {
        SessionId = sessionId;
        Error = null;
        Status = ImportStatus.Completed;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        Status = ImportStatus.Failed;
    }
}
```

Interview explanation:

> "The job stores the state needed to make asynchronous processing visible and retryable. I keep status transitions explicit so callers can reason about what happened."

### Exercise 2 — Write An Idempotent Handler

Given:

```csharp
public sealed record ProcessImportJob(Guid JobId);
```

Write handler logic in pseudocode:

```csharp
public async Task HandleAsync(ProcessImportJob command, CancellationToken cancellationToken)
{
    var job = await _jobs.GetByIdAsync(command.JobId, cancellationToken);

    if (job is null)
    {
        return;
    }

    if (job.Status is ImportStatus.Completed)
    {
        return;
    }

    await _jobs.MarkProcessingAsync(job.Id, cancellationToken);

    try
    {
        var session = await _parser.ParseAsync(job.FilePath, cancellationToken);
        await _sessions.AddAsync(session, cancellationToken);
        await _jobs.MarkCompletedAsync(job.Id, session.Id, cancellationToken);
    }
    catch (Exception ex)
    {
        await _jobs.MarkFailedAsync(job.Id, ex.Message, cancellationToken);
    }
}
```

Then explain what is still missing:

- Conditional claim to prevent two workers processing the same job
- Transaction boundary between session save and job completion
- Retry policy
- Error classification
- Logging

Interview explanation:

> "The first idempotency guard is checking whether the job is already completed. In a real multi-worker system, I would also claim the job atomically in the database so only one worker processes it."

### Exercise 3 — Explain The Outbox Bug

Question:

> "What happens if your API saves to the database but fails to publish a message?"

Strong answer:

> "The database state changes, but other systems never hear about it. That creates an inconsistent system. I would use the Outbox pattern: save the domain change and an outbox message in the same transaction, then have a background publisher send the message and mark it published."

### Exercise 4 — Choose Sync Or Async Processing

Given a FIT upload endpoint, decide between synchronous parsing and background parsing.

Say:

```text
I would start synchronous if:
  - files are small
  - parsing is fast
  - immediate response matters
  - traffic is low

I would move background if:
  - parsing is slow
  - uploads are large
  - retry/recovery matters
  - API latency matters
  - worker scaling should be separate
```

Interview explanation:

> "This is a tradeoff, not a rule. Background processing improves responsiveness and reliability for slow work, but it adds eventual consistency and operational complexity."

### Exercise 5 — Design A Retry Policy

For import jobs:

```text
Max attempts: 3
Delay: exponential backoff
Retry only transient failures
Do not retry invalid FIT files
Move permanently failed jobs to Failed status
Log every failure with job id and attempt number
```

Possible distinction:

- Transient: temporary database timeout, temporary storage failure
- Permanent: invalid file, unsupported FIT format, missing file

Interview explanation:

> "I do not retry every failure blindly. Retrying an invalid file just wastes resources. I classify failures and retry only the ones that are likely to recover."

---

## Hour 5 — Interview Speaking Practice

Answer these out loud. Keep each answer under 2 minutes.

### Questions

1. What is the difference between synchronous and asynchronous processing?
2. When would you move work to a background worker?
3. How would you design background FIT parsing in Zapas?
4. Why return `202 Accepted` instead of `201 Created`?
5. What is a message queue?
6. What is pub/sub?
7. What problem does the Outbox pattern solve?
8. How do you make message handling idempotent?
9. How can retries create duplicate operations?
10. What is eventual consistency?
11. What is a circuit breaker?
12. How would you monitor a background worker?
13. What would you log for a failed import job?
14. When would you use Docker?
15. What is .NET Aspire useful for?

### Strong Answer — Background Processing

> "I move work to the background when it is slow, failure-prone, or does not need to finish inside the original HTTP request. In Zapas, parsing a small FIT file synchronously is fine at first. If parsing becomes slow or uploads increase, I would create an import job, return `202 Accepted`, process it with a worker, and expose a status endpoint."

### Strong Answer — Outbox Pattern

> "The Outbox pattern handles the reliability gap between saving data and publishing messages. I save the business change and an outbox message in the same database transaction. A background publisher later sends unpublished messages and marks them published. That way, if the process crashes, the message is still durable."

### Strong Answer — Idempotency

> "Idempotency means a repeated operation produces the same final result. It matters because retries and duplicate messages are normal. For Zapas import jobs, I would use a stable job id, avoid reprocessing completed jobs, claim pending jobs atomically, and store the created session id so a retry does not create duplicate sessions."

### Strong Answer — Eventual Consistency

> "Eventual consistency means the system may temporarily show intermediate state. For example, after an upload, the import job may be pending while the session does not exist yet. The API should expose that honestly with job status instead of pretending the final resource is immediately available."

### Strong Answer — Circuit Breaker

> "A circuit breaker protects the app from repeatedly calling a dependency that is failing. After enough failures, it opens and fails fast for a period. That prevents request threads from piling up and gives the dependency time to recover."

### Strong Answer — Observability

> "For background workflows, I want structured logs with job ids, metrics for queue depth and processing duration, and health checks that show whether workers are making progress. Without that, failures become hard to diagnose because they are not tied to a single HTTP response."

### Behavioral Practice

Prepare short answers for:

- Tell me about a time you made a system more reliable.
- Tell me about a time a background process failed.
- Tell me about a time you had to debug a production issue with incomplete logs.
- Tell me about a time you chose a simpler architecture over a more complex one.
- Tell me about a time you had to explain eventual consistency or delayed processing to non-technical stakeholders.
- Tell me about a time you improved monitoring or error handling.
- Tell me about a time you had to design for retries or duplicate operations.

### "Reliability Improvement" Draft

> "One reliability improvement I focus on is making long-running workflows visible and recoverable. Instead of doing hidden work inside a request, I create durable state, log important transitions, and design retries to be idempotent. The goal is that if something fails, we know what failed, whether it can be retried, and what state the user sees."

---

## Day 7 Deliverable

At the end of today, Zapas should have one of these:

- A written design for background FIT import jobs, or
- A small implemented import-job slice with durable job state and a hosted worker

If implementing, aim for:

- A `SessionImportJob` model or entity
- A status enum such as `Pending`, `Processing`, `Completed`, and `Failed`
- A repository boundary for import jobs
- A hosted worker or processor class
- A `202 Accepted` response shape for async imports
- A status endpoint design such as `GET /session-imports/{id}`
- Structured logs around job creation, processing, completion, and failure
- Clear retry and idempotency notes

Write notes for yourself:

- One reason synchronous parsing is still acceptable
- One reason background parsing may become necessary
- One failure mode in the background workflow
- One idempotency protection you would add
- One metric or log field that would help debug production issues
- One reason you would avoid microservices at this stage

---

## Final Interview Framing

Use this as your Day 7 summary:

> "I reviewed Zapas from a distributed-systems perspective and designed a background FIT import workflow. The synchronous endpoint is simpler and still valid for small files, but a background job model would let the API return `202 Accepted`, process files asynchronously, retry failures, and expose job status. I can explain the tradeoffs around queues, outbox messages, idempotency, eventual consistency, retries, circuit breakers, observability, Docker, and .NET Aspire without over-engineering the app."
