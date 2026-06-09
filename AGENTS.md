# Zapas API

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

# iOS App

When adding the Zapas iOS app, prefer a native SwiftUI-first implementation unless a specific feature clearly needs UIKit. Use UIKit as an interop layer for APIs or controls that SwiftUI cannot express cleanly, not as the default app architecture.

Recommended structure:

- `apps/ios/`: iOS app workspace/project.
- `apps/ios/Zapas/`: app source.
- `apps/ios/Zapas/App/`: app entry point, scene setup, dependency composition, and app-wide routing.
- `apps/ios/Zapas/Features/`: feature modules such as sessions, activity import, profile, settings, and workout detail views.
- `apps/ios/Zapas/Core/`: reusable app infrastructure such as networking, persistence, authentication, logging, and configuration.
- `apps/ios/Zapas/Models/`: iOS-side domain models and API response mappings.
- `apps/ios/Zapas/Design/`: design tokens, reusable SwiftUI components, colors, typography, and layout helpers.
- `apps/ios/ZapasTests/`: unit tests.
- `apps/ios/ZapasUITests/`: UI and integration-style tests.

## Apple Guidance

Follow current Apple platform guidance when making iOS decisions:

- Use Apple's Human Interface Guidelines for navigation, layout, gestures, accessibility, typography, color, privacy prompts, and platform conventions.
- Use Apple's Swift API Design Guidelines for naming, API shape, and clarity.
- Prefer Swift concurrency (`async`/`await`, `Task`, `Actor`, `MainActor`) over callback-heavy code.
- Use SwiftUI data-flow patterns intentionally: keep view state local, isolate business logic in view models or feature services, and keep side effects out of views.
- Use Apple's security and privacy guidance for Keychain, App Transport Security, permissions, HealthKit, location, background work, and user data handling.
- Use Instruments, Xcode diagnostics, and MetricKit where appropriate instead of guessing about performance.

## Architecture And Patterns

Keep the iOS app modular and testable:

- Views should describe UI and user interaction only.
- View models should coordinate screen state, validation, loading, and user actions.
- Services should contain app workflows such as authentication, session sync, FIT upload/import coordination, and API orchestration.
- Repositories or clients should isolate persistence and network access.
- Models should represent app concepts and values. Keep API DTOs separate when the external contract differs from the app model.
- Inject dependencies through initializers or a small composition layer. Avoid hidden global state.
- Keep feature code grouped by user workflow rather than by generic technical type when that improves locality.

Prefer a clear unidirectional data flow for feature screens:

- User action starts in a SwiftUI view.
- The view calls a view model intent or action method.
- The view model calls services/repositories.
- Results update observable state on the main actor.
- The view renders from state.

## Security And Privacy

Treat fitness and activity data as sensitive:

- Store tokens, credentials, and long-lived secrets in Keychain, not `UserDefaults`.
- Do not log access tokens, refresh tokens, personally identifiable information, precise location traces, or raw activity payloads.
- Use HTTPS for API traffic and keep App Transport Security enabled unless there is a documented local-development exception.
- Request permissions only when the user starts a feature that needs them, and make permission copy specific to the value the user receives.
- Keep HealthKit, location, file import, and background processing entitlements narrowly scoped.
- Validate and bound file imports before parsing, especially FIT or other activity files.
- Prefer server-issued short-lived tokens and refresh flows over storing permanent credentials on device.

## Performance

Build for smooth scrolling, responsive charts, and efficient activity processing:

- Keep expensive parsing, calculations, network calls, and disk work off the main actor.
- Move large FIT parsing and derived metric calculations into services that can run asynchronously.
- Use pagination, lazy containers, and incremental loading for long session histories.
- Avoid recomputing chart data in SwiftUI `body`; precompute view-ready series in a view model or service.
- Keep image, map, and chart rendering bounded by the current viewport and selected time range.
- Use Instruments to verify memory, CPU, hangs, launch time, and scrolling performance before broad optimization.
- Prefer simple, measurable changes over speculative caching. Add caching only when there is a clear latency, battery, or data-usage benefit.

## Quality Bar

Before considering iOS work complete:

- Add focused unit tests for calculations, mapping, validation, and state transitions.
- Add UI tests for critical flows such as sign-in, session list, session detail, import/upload, and settings when those flows exist.
- Verify accessibility labels, Dynamic Type behavior, color contrast, VoiceOver order, and reduced-motion behavior for user-facing screens.
- Test offline, slow-network, expired-token, empty-state, large-history, and malformed-import scenarios.
- Keep local development configuration separate from production configuration.
