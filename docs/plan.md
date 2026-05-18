# Senior .NET Interview Preparation Plan — 2 Weeks

## Candidate Context

You are a **.NET web developer with 10 years of experience**, all at the same company. Your technical knowledge is not obsolete, but you want to refresh and modernize it before interviewing. Your main gap is likely not engineering ability, but **interview readiness**:

- Explaining your decisions clearly
- Coding under interview pressure
- Speaking confidently about modern .NET
- Discussing architecture and production tradeoffs
- Preparing behavioral stories from your experience

## Main Goal

The goal is **interview readiness in 2 weeks**, not mastering the entire modern .NET ecosystem.

By the end of the plan, you should be able to:

- Answer common senior .NET interview questions
- Build and explain a clean ASP.NET Core Web API
- Discuss EF Core, SQL, performance, testing, architecture, and production issues
- Solve common live-coding problems
- Explain your 10 years of experience confidently
- Handle practical system design conversations

---

# Realistic Expectations

## What Is Doable in 2 Weeks

You can realistically become strong enough to:

- Explain C#, ASP.NET Core, EF Core, SQL, async, testing, and architecture clearly
- Solve easy-to-medium live-coding problems
- Discuss senior-level tradeoffs
- Prepare strong behavioral answers
- Refresh modern .NET terminology
- Build one small but polished practice API

## What Is Not Realistic in 2 Weeks

You should not expect to become deeply expert in:

- Advanced distributed systems
- Kubernetes
- Deep cloud infrastructure
- Complex DDD
- Very hard algorithmic problems
- Every new .NET 8/9/10/modern feature
- Full microservices architecture

That is okay. Most senior .NET web interviews do not expect you to know everything. They expect you to reason clearly.

---

# Priority Topics

## Must-Have Topics

These are the core topics for the 2-week plan:

1. C# fundamentals
2. ASP.NET Core Web API
3. Dependency Injection
4. EF Core + SQL
5. Async / await
6. Testing
7. Live coding
8. System design basics
9. Behavioral stories
10. Production troubleshooting

## Nice-to-Have Topics

Cover these only at a high level:

- .NET Aspire
- OpenTelemetry
- Outbox pattern
- CQRS
- DDD
- Microservices
- gRPC
- Kubernetes

Acceptable interview phrasing:

> “I understand the concept and tradeoffs, but I have not used it deeply in production.”

---

# Daily Time Structure

Each day is **5 hours**.

| Activity | Time |
|---|---:|
| Build/refactor code | 2 hours |
| Technical review | 1 hour |
| Live coding | 1 hour |
| Interview speaking practice | 1 hour |

The most important rule:

> Do not study passively. Build, explain, and practice out loud every day.

---

# Final Practice Project

Use your real project throughout the two weeks:

## Zapas FIT Session API

`Zapas` is now the main interview-ready practice app instead of the generic Product Catalog API.

The current project is an ASP.NET Core Web API that accepts uploaded `.fit` running activity files, extracts session and running lap/interval information using the Garmin FIT SDK, stores parsed sessions, and exposes session data through HTTP endpoints.

Current baseline:

- `Zapas.Api/`: ASP.NET Core API project targeting `net10.0`
- `Program.cs`: dependency injection, middleware pipeline, Swagger, controller registration
- `Controllers/SessionsController.cs`: HTTP endpoints for sessions
- `Services/SessionService.cs`: FIT parsing, session creation workflow, pace calculation, lap extraction
- `Repositories/ISessionRepository.cs`: persistence abstraction
- `Repositories/InMemorySessionRepository.cs`: current in-memory storage implementation
- `Models/Session.cs` and `Models/RunInterval.cs`: current domain models
- `DTOs/`: API response DTOs
- `Middleware/GlobalExceptionMiddleware.cs`: centralized exception handling
- `AGENTS.md`: project guidance and intended separation of concerns

Current API shape:

- `GET /sessions`
- `GET /sessions/{id}`
- `POST /sessions` with `multipart/form-data` and a `.fit` file

The project should evolve into a small but polished interview API that includes:

- ASP.NET Core Web API
- Thin controllers
- Service layer with business logic
- Repository abstraction
- DTOs
- Validation
- Global exception handling
- Structured logging
- Pagination
- Filtering
- Sorting
- Unit tests
- Integration tests
- EF Core + SQL persistence
- Basic caching
- Health checks
- Optional authentication/authorization
- Optional background processing for file parsing or import jobs

The goal is not to turn `Zapas` into a huge production platform. The goal is to make it strong enough that you can confidently explain real code in an interview.

You should be able to explain:

- Why the controller stays thin
- Why FIT parsing belongs in the service/application layer
- How DTOs protect the API contract
- Why an in-memory repository is useful for early development but not production
- How you would replace in-memory storage with EF Core
- How you would validate uploaded files
- How you would handle corrupted FIT files
- How you would prevent large-file abuse
- How you would make parsing asynchronous or background-based
- How you would test the FIT parsing workflow
- How you would scale session querying
- How you would secure file upload endpoints
- How you would observe failures, latency, and parsing errors
- How you would deploy it

Recommended interview framing:

> “I used a real API called Zapas as my interview practice project. It parses uploaded running FIT files, extracts session and interval data, and exposes the results through ASP.NET Core endpoints. I used it to practice API design, validation, dependency injection, EF Core persistence, testing, observability, and senior-level tradeoff discussions.”

---

# Week 1 — Core Modern .NET + Interview Foundation

## Day 1 — C# Fundamentals + Interview Reset

### Goal

Refresh C# and start practicing senior-level explanations.

### Topics

- Value types vs reference types
- Records vs classes
- Nullable reference types
- LINQ
- `IEnumerable<T>` vs `IQueryable<T>`
- Deferred execution
- `async` / `await`
- `Task` vs `ValueTask`
- Exceptions
- Generics
- Extension methods
- Collections
- Immutability basics

### Live Coding

Practice:

1. Two Sum
2. First non-repeating character
3. Group anagrams
4. Flatten nested collections
5. Custom `Where` extension method

### Interview Questions

- What is the difference between `IEnumerable` and `IQueryable`?
- What happens when you call `.ToList()` too early?
- What is deferred execution?
- Difference between `record`, `class`, and `struct`?
- What problem do nullable reference types solve?
- How does `async/await` work conceptually?

### Deliverable

Create `Senior .NET Interview Notes.md` with:

- C# topics I can explain well
- C# topics I need to review
- Common mistakes I must avoid

---

## Day 2 — ASP.NET Core Web API

### Goal

Be able to design and explain a clean API.

### Topics

- ASP.NET Core request pipeline
- Middleware
- Controllers vs Minimal APIs
- Routing
- Model binding
- Validation
- Filters
- Dependency Injection lifetimes
- Configuration
- Options pattern
- Logging
- Health checks
- Error handling
- API versioning basics

### Hands-On

Continue improving the existing **Zapas FIT Session API**.

Review the current endpoints:

- `GET /sessions`
- `GET /sessions/{id}`
- `POST /sessions` with `multipart/form-data` and a `.fit` file

Improve or add:

- Explicit request validation for missing file, empty file, wrong extension, and oversized file
- Consistent error response DTO instead of anonymous or inconsistent error shapes
- Clear `CreatedAtAction` response for successful uploads
- Structured logging around upload start, validation rejection, parsing failure, and successful storage
- Dependency injection explanation for `ISessionService` and `ISessionRepository`
- Swagger/OpenAPI documentation for file upload
- Proper HTTP status codes for success, validation errors, not found, and unexpected errors

Optional stretch:

- Add `DELETE /sessions/{id}` only if it helps you discuss API design. Do not add update endpoints unless there is a real use case.

### Interview Questions

- Explain the ASP.NET Core middleware pipeline.
- What is the difference between Singleton, Scoped, and Transient?
- Why should `DbContext` usually be Scoped?
- How do you handle exceptions globally?
- How would you version an API?
- When would you use Minimal APIs?

### Key Explanation

> “The request enters the middleware pipeline, passes through routing, authentication, authorization, model binding, validation, controller action, service layer, data access layer, and returns a response with consistent error handling and logging.”

---

## Day 3 — EF Core + SQL

### Goal

Become confident with EF Core and database conversations.

### Topics

- DbContext lifecycle
- Change tracking
- Migrations
- LINQ translation
- `Include` vs projection
- Lazy loading vs eager loading
- N+1 query problem
- Transactions
- Optimistic concurrency
- Indexes
- Query performance
- Raw SQL basics
- Repository pattern debate

### Hands-On

Extend Zapas with persistent storage:

- SQL Server, PostgreSQL, or SQLite for interview practice
- EF Core
- Migrations
- `SessionEntity`
- `RunIntervalEntity`
- One-to-many relationship from session to intervals
- Pagination
- Filtering
- Sorting

Implement:

`GET /sessions?page=1&pageSize=20&sort=startTime&from=2026-01-01&to=2026-12-31`

Recommended filters:

- Date range
- Minimum distance
- Maximum distance
- Has intervals
- Name contains text

Senior-level goal:

- Keep domain/API models separate from EF entities if doing so keeps the code clearer.
- Use projections to DTOs for read endpoints.
- Use `AsNoTracking()` for read-only queries.
- Explain why in-memory storage was acceptable for the first version but not for production.

### SQL Practice

Practice writing:

- Inner join
- Left join
- Group by
- Having
- Basic window function
- Transaction example
- Index explanation

### Interview Questions

- What is the N+1 problem?
- What is change tracking?
- When should you use `.AsNoTracking()`?
- What is the difference between `Include` and projection?
- What causes bad EF Core performance?
- How do you handle concurrency conflicts?
- Repository pattern with EF Core: good or bad?

### Key Explanation

> “I avoid returning tracked entities directly from APIs. I usually project to DTOs, use `AsNoTracking` for read-only queries, paginate large results, and inspect generated SQL when performance matters.”

---

## Day 4 — Async, Performance, Caching, Diagnostics

### Goal

Sound like someone who has worked on production systems.

### Topics

- Thread pool
- Async I/O
- Blocking calls
- Deadlocks
- `ConfigureAwait`
- Cancellation tokens
- Timeouts
- Retries
- Caching
- Memory cache vs distributed cache
- Rate limiting
- Logging
- Metrics
- Tracing
- OpenTelemetry basics

### Hands-On

Add to Zapas:

- Async EF Core queries
- `CancellationToken` support in controller, service, and repository methods
- In-memory cache for `GET /sessions/{id}`
- Basic rate limiting, especially for `POST /sessions`
- Request logging middleware or improved structured request logs
- Health check endpoint
- File upload size limits
- Timeout/guardrails around parsing work where reasonable

Discussion focus:

- FIT parsing is CPU/file-processing work, while DB access is I/O-bound.
- Large file uploads can affect memory, latency, and availability.
- Caching session lookup is useful, but caching uploaded parsing results requires careful invalidation.

### Debugging Scenarios

Practice explaining:

- API is slow under load
- Database CPU is high
- Memory usage keeps growing
- Requests randomly timeout
- Thread pool starvation
- Cache stampede
- High p95 latency

### Interview Questions

- Why is blocking bad in ASP.NET Core?
- Difference between CPU-bound and I/O-bound work?
- What is a cancellation token?
- How would you improve slow API performance?
- How do you detect memory leaks?
- When would you cache?
- What are the risks of caching?

### Key Explanation

> “Before optimizing, I would measure. I would check logs, metrics, traces, database query plans, hot paths, allocation patterns, and p95/p99 latency.”

---

## Day 5 — Testing + Clean Code + First Mock Interview

### Goal

Prove that you can ship maintainable code.

### Topics

- Unit tests
- Integration tests
- Test doubles
- Mocking
- Arrange / Act / Assert
- xUnit
- FluentAssertions
- Moq or NSubstitute
- API integration testing
- Clean Architecture basics
- SOLID principles
- Refactoring legacy code

### Hands-On

Add tests to Zapas:

- Unit tests for `SessionService.CreateSession`
- Unit tests for file validation: missing file, empty file, wrong extension
- Unit tests for pace calculation behavior, including zero or missing distance/duration
- Unit tests for repository behavior
- Integration test for `GET /sessions`
- Integration test for `GET /sessions/{id}` not found
- Integration test for `POST /sessions` validation failure
- Integration test for global error response shape

Optional if you have a sample `.fit` file:

- Integration test for successful FIT upload and extraction
- Test that active running laps become `RunIntervalDto` values

### Mock Interview — 60 Minutes

Simulate:

- 15 min — C# questions
- 20 min — live coding
- 15 min — API / EF Core questions
- 10 min — behavioral

### Behavioral Questions

Prepare answers for:

- Tell me about yourself.
- Why are you leaving or exploring after 10 years?
- Tell me about a difficult bug.
- Tell me about a disagreement with another developer.
- Tell me about a system you improved.
- Tell me about a production issue.
- What are your strengths?
- What do you want to improve?

### “Tell Me About Yourself” Draft

> “I’m a .NET web developer with 10 years of experience building and maintaining web applications in the same company. That gave me deep ownership of long-lived systems, production support, business context, and maintainability. Now I’m looking to grow in a more modern engineering environment, and I’ve been refreshing my skills in ASP.NET Core, EF Core, testing, cloud-native patterns, and system design.”

---

# Week 2 — Senior-Level Readiness + Mock Interviews

## Day 6 — Architecture, Design Patterns, Senior Judgment

### Goal

Move from “developer” answers to “senior engineer” answers.

### Topics

- Layered architecture
- Clean Architecture
- Vertical Slice Architecture
- SOLID
- Dependency inversion
- DDD basics
- CQRS basics
- Mediator pattern
- Repository pattern
- Unit of Work
- Domain services
- Application services
- Modular monolith
- Microservices tradeoffs

### Hands-On

Refactor Zapas into clearer boundaries:

- API layer: controllers, request/response DTOs, HTTP concerns
- Application layer: upload workflow, validation orchestration, session use cases
- Domain layer: `Session`, `RunInterval`, pace calculation rules, domain invariants
- Infrastructure layer: FIT SDK adapter, EF Core repository, file/storage details

Do not over-engineer. The goal is to explain boundaries.

Possible refactor:

- Extract FIT parsing behind an `IFitSessionParser`
- Keep `SessionService` focused on the create-session workflow
- Move persistence details behind repository interfaces
- Keep controllers thin and HTTP-focused
- Create a clear mapping strategy between entities, domain models, and DTOs

### Interview Questions

- How do you structure a medium-sized .NET application?
- What is Clean Architecture?
- What are the downsides of Clean Architecture?
- When would you use microservices?
- When would you avoid microservices?
- What is a modular monolith?
- How do you prevent business logic from leaking into controllers?

### Key Explanation

> “I prefer starting with a well-structured modular monolith unless there is a clear scaling, team, deployment, or domain boundary reason to split into microservices.”

---

## Day 7 — Distributed Systems + Cloud-Native Basics

### Goal

Understand modern backend concepts well enough to discuss tradeoffs.

### Topics

- REST
- gRPC basics
- Message queues
- Pub/sub
- Outbox pattern
- Idempotency
- Retries
- Circuit breaker
- Eventual consistency
- Background services
- Hosted services
- Docker basics
- Observability
- .NET Aspire at a high level

### Hands-On

Add a simple background worker scenario:

When a FIT file is uploaded:

1. Store an import request record
2. Store the uploaded file temporarily or persist enough metadata for processing
3. Add an outbox/import job record
4. Background worker parses the FIT file
5. Store extracted session and intervals
6. Mark the import job as processed or failed

No need to add RabbitMQ unless you have extra time.

Interview tradeoff:

- Synchronous parsing is simpler and fine for small files.
- Background parsing is better when uploads are large, parsing is slow, or you need reliability and retries.

### Interview Questions

- What happens if your API saves to DB but fails to publish a message?
- What is the Outbox pattern?
- How do you make message handling idempotent?
- What is eventual consistency?
- How do retries cause duplicate operations?
- What is a circuit breaker?
- How would you monitor distributed systems?

### Key Explanation

> “In distributed systems, I assume failure will happen. I design for retries, idempotency, observability, timeouts, and consistency tradeoffs.”

---

## Day 8 — Security, Authentication, Authorization

### Goal

Be able to discuss secure APIs confidently.

### Topics

- Authentication vs authorization
- JWT
- OAuth2 basics
- OpenID Connect basics
- Claims
- Roles vs policies
- Refresh tokens
- CSRF
- CORS
- XSS
- SQL injection
- Secrets management
- HTTPS
- Secure headers
- OWASP Top 10 basics

### Hands-On

Add to Zapas:

- JWT authentication
- Role-based authorization
- Policy-based authorization
- Secure configuration
- Input validation
- CORS policy
- Upload size limits
- File type validation
- Safe handling of parser failures

Example endpoint policy:

- Authenticated user: `GET /sessions`
- Authenticated user: `GET /sessions/{id}`
- Athlete/User role: `POST /sessions`
- Admin only: `DELETE /sessions/{id}` if implemented

Security discussion focus:

- Never trust file extension alone in production.
- Limit file size.
- Log parsing failures without leaking file contents.
- Store secrets outside source control.
- Use authorization rules to prevent users from reading or deleting other users’ sessions.

### Interview Questions

- Difference between authentication and authorization?
- What is JWT?
- Where should JWTs be stored?
- What is CORS?
- How do you prevent SQL injection?
- What are common API security mistakes?
- How do you store secrets?
- How do you handle authorization in ASP.NET Core?

### Key Explanation

> “I prefer policy-based authorization when permissions become more complex than simple roles. It keeps authorization rules explicit, testable, and easier to evolve.”

---

## Day 9 — Live Coding Intensive

### Goal

Get comfortable solving problems while talking.

### Format

Do 5 live-coding problems, about 45 minutes each.

For each problem:

1. Restate the problem
2. Ask clarifying questions
3. Explain approach
4. Write clean code
5. Test manually
6. Discuss complexity
7. Mention edge cases

### Problems

1. Valid Parentheses
2. LRU Cache
3. Merge Intervals
4. Rate Limiter
5. In-memory repository

### What To Say While Coding

> “First I’ll solve the simple correct version. Then I’ll improve edge cases and complexity.”

> “I’m choosing readability here. If this were production code, I’d also consider thread safety, logging, validation, and tests.”

### Mistakes To Avoid

- Coding silently
- Jumping into code without clarifying
- Overengineering
- Ignoring edge cases
- Not testing your own code
- Not explaining complexity
- Getting stuck and saying nothing

---

## Day 10 — Final Mock Interview + Review

### Goal

Simulate the real interview.

### Hour 1 — Behavioral Interview

Practice:

- Tell me about yourself
- Why now after 10 years?
- Biggest technical challenge
- Production incident
- Conflict with teammate
- Mentoring experience
- Legacy system experience
- Technical decision you regret

### Hour 2 — Technical Screen

Random questions:

- Explain DI lifetimes.
- Explain async/await.
- Explain middleware.
- Explain EF tracking.
- Explain API versioning.
- Explain caching.
- Explain testing strategy.
- Explain authentication flow.
- Explain clean architecture.

### Hour 3 — Live Coding

Do one medium problem:

- LRU Cache
- Rate limiter
- Transaction-safe inventory update
- File parser
- Deduplication service

### Hour 4 — System Design

Design one of:

- E-commerce checkout API
- Appointment booking system
- Notification service
- Running activity import/session analytics API
- Payment processing backend
- Audit logging system

Use this structure:

1. Requirements
2. APIs
3. Data model
4. Architecture
5. Failure cases
6. Scaling
7. Security
8. Observability
9. Tradeoffs

### Hour 5 — Final Review

Create your final cheat sheet:

- 10 stories from your experience
- 20 technical topics
- 5 live-coding patterns
- 5 architecture patterns
- 5 questions to ask interviewer

---

# Live Coding Patterns To Practice

Prioritize these:

1. Dictionary lookup
2. Two pointers
3. Stack
4. Queue
5. Sorting + scanning
6. Sliding window
7. Tree/recursive traversal
8. Basic dynamic programming
9. String parsing
10. Simple object-oriented design

Backend-style problems are especially useful:

- Implement a rate limiter
- Implement an LRU cache
- Implement a retry policy
- Implement a simple scheduler
- Implement a deduplication service
- Implement a repository
- Implement pagination
- Implement validation logic
- Implement a background queue
- Implement an idempotency key store

---

# Senior Interview Questions To Ask the Interviewer

Use these near the end of the interview:

1. “What does success look like for this role in the first 6 months?”
2. “How is the engineering team structured?”
3. “What are the biggest technical challenges the team is facing?”
4. “How do you approach code reviews and technical decisions?”
5. “Is the system mostly monolith, modular monolith, or microservices?”
6. “What is your current .NET version and deployment model?”
7. “How much ownership would this role have over architecture and technical direction?”

---

# How To Frame 10 Years In One Company

Do not apologize for staying at the same company.

Frame it as:

- Ownership
- Reliability
- Domain depth
- Production maturity
- Long-term maintainability
- Business understanding
- Experience with consequences of technical decisions

Strong phrasing:

> “Staying in one company for a long time gave me deep ownership. I worked with long-lived systems, understood business impact, supported production, maintained legacy code, and saw the consequences of technical decisions over time. Now I’m looking to combine that experience with a more modern engineering environment.”

---

# Final Success Criteria

At the end of two weeks, you are ready if you can:

- Explain an ASP.NET Core request from middleware to response
- Explain DI lifetimes with examples
- Explain EF Core tracking, projections, and performance pitfalls
- Solve common easy-to-medium coding problems while talking
- Explain async/await and why blocking is bad
- Discuss testing strategy
- Walk through a small system design
- Explain a production issue you solved
- Explain how you work with legacy code
- Ask senior-level questions to the interviewer

Final mindset:

> The goal is not to know everything. The goal is to show clear reasoning, production maturity, and senior-level judgment.


---

# Zapas-Specific Interview Talking Points

Use these when an interviewer asks you to walk through your practice project.

## 30-Second Project Explanation

> “Zapas is an ASP.NET Core API for running activity analysis. The API accepts uploaded `.fit` files, validates the upload, parses the activity with the Garmin FIT SDK, extracts session-level metrics and active running intervals, stores the result, and exposes session data through REST endpoints.”

## Current Strengths

- Clear API entry point through `SessionsController`
- DTOs separate the response contract from internal models
- Service layer contains parsing and workflow logic
- Repository abstraction exists
- Global exception middleware exists
- Swagger is enabled for local API exploration
- The project has a real domain, which makes interview discussion stronger than a generic CRUD app

## Current Gaps To Improve During The Plan

- Add automated tests
- Replace in-memory storage with EF Core persistence
- Add async repository/service methods where I/O is involved
- Add cancellation token support
- Improve upload validation and error response consistency
- Add pagination/filtering/sorting for session queries
- Add health checks and better observability
- Consider extracting FIT parsing behind an interface
- Consider background processing for slow or large uploads
- Add authentication/authorization if time permits

## Best Senior-Level Explanation

> “The first version intentionally keeps the design simple: controller, service, repository, DTOs, and middleware. As requirements grow, I would separate the FIT parser behind an interface, persist sessions and intervals with EF Core, add tests around parsing and validation, and move long-running parsing to a background worker if upload latency becomes a problem. I would measure before optimizing and keep the architecture proportional to the API’s complexity.”
