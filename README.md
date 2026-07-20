# Zapas API

## Overview
- An authenticated user can upload a `.fit` running activity file.
- The successful upload response contains the created session.
- Users can list their sessions with pagination and date-range filters.
- Users can retrieve the details and intervals of one session.


## Project Structure

- `Zapas.slnx`: solution file.
- `Zapas.Api/`: ASP.NET Core API project.
- `Zapas.Api/Program.cs`: application startup, dependency injection, middleware pipeline, and endpoint/controller registration.
- `Zapas.Api/Controllers/`: HTTP controllers only. Keep controllers thin: validate request shape, call services, and return HTTP results.
- `Zapas.Api/Services/`: application and domain workflow logic. Put FIT parsing, session extraction, calculations, and orchestration here instead of controllers.
- `Zapas.Api/Repositories/`: persistence and data access abstractions/implementations.
- `Zapas.Api/Models/`: domain/API models returned or used by the application, such as session and interval models.
- `Zapas.Api/DTOs/`: request/response DTOs when the API contract should differ from internal models.
- `Zapas.Api/Middleware/`: custom ASP.NET Core middleware.
- `Zapas.Api/TestData/`: local-only test fixtures, including sample activity files.
- `.vscode/`: shared debug/build configuration for local development.

## Tradeoffs And Decision

Synchronous upload and parsing is the selected design.

Benefits:

- The API contract is simple: one request returns the created session or a specific error.
- Users receive immediate validation and parser feedback.
- There is no separate import resource or status polling flow.
- Deployment, persistence, testing, and operational behavior have fewer moving parts.
- Session and interval data are immediately consistent when the request succeeds.

Costs:

- Parsing and calculations add latency to the upload request.
- Each concurrent upload consumes API CPU, memory, and a request slot.
- Client or proxy timeouts place a hard bound on supported activity size and complexity.
- Capacity planning must account for parsing work on API instances.

These costs are controlled with strict upload bounds, cancellation, concurrency and rate limits, streaming, transactional persistence, and measurement. This is appropriate while FIT files can be processed reliably within the documented request budget.

The resulting design is a modular ASP.NET Core API with synchronous FIT validation, parsing, analytics calculation, and transactional persistence. It favors a direct client experience and operational simplicity while explicitly bounding the work performed on the request path.