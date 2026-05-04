# Zapas Project Guidance

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

## Separation Of Concerns

Keep responsibilities separated as the app grows:

- Controllers should not parse FIT files, calculate running metrics, or access storage directly.
- Services should contain business logic and reusable workflows.
- Repositories should isolate persistence details from controllers and services.
- DTOs should define external API request/response shapes when needed.
- Models should represent app concepts and values, not HTTP or storage mechanics.
- Middleware should handle cross-cutting HTTP concerns only.

Prefer small, focused classes over placing unrelated behavior in a single file. When adding a feature, put code in the folder that matches its responsibility and wire dependencies through dependency injection in `Program.cs`.
