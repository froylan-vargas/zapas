# Week 1 Current-State Architecture

## Boundary implications

- The uploaded FIT file is parsed synchronously inside the HTTP request and API
  process. Parsing consumes local CPU and memory until it succeeds, fails, or the
  request is cancelled.
- `IMemoryCache` and rate-limit counters are per-process state. A restart clears
  them; a second API instance would have independent cache entries and counters.
- The local SQLite file is durable across a process restart only while that same
  host and file remain intact. It is not shared safely by scaled-out hosts and is
  disposable for this learning deployment.
- `GET /sessions` reads SQLite directly. `GET /sessions/{id}` reads local process
  memory first and queries SQLite on a cache miss. `POST /sessions` writes SQLite
  before populating the cache.
- `/health` exercises EF Core and the SQLite dependency, so it currently behaves
  as a readiness-style dependency check rather than a process-only liveness
  check.

# Current tests don't prove
  - Real Auth0 authentication, JWT validation, roles, or network connectivity.
  - A complete valid FIT-file upload from HTTP request through parsing and persistence.
  - Production SQLite migrations, file permissions, durability, or concurrent access.
  - Behavior with multiple application instances.
  - HTTPS, CORS, rate limiting, health probes, logging, or deployment configuration.
  - Performance, load, memory usage, timeouts, cancellation, and recovery from dependency failures.
  - Security vulnerabilities or correctness outside the tested cases.

# State that disappears when the process restarts
  - IMemoryCache entries, including cached sessions for GET /sessions/{id}.
  - Fixed-window rate-limiter counters for each client IP.
  - Any active requests and their temporary parsing data.

# Two instances inconsistent state
  - SQLite: Each instance may use its own database file, so sessions written to one instance may not exist on the other. Sharing one SQLite file across instances also introduces locking and reliability problems.

  - IMemoryCache: Each instance has a separate cache. One instance may serve stale data after another changes the database.

  - Rate-limit counters: Each instance counts requests independently. A client can effectively exceed the intended limit by having requests distributed between instances.