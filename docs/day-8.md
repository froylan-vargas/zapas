# Day 8 — Security, Authentication, Authorization

## Instructor Goal

Today you are practicing how to secure the Zapas FIT Session API like a senior .NET engineer.

By the end of the day, you should be able to explain:

- The difference between authentication and authorization
- How JWT bearer authentication works in ASP.NET Core
- How to protect endpoints with roles and policies
- Why resource ownership matters for user-specific data
- How to secure file uploads
- How CORS, CSRF, XSS, SQL injection, HTTPS, and secure headers fit into API security
- How to keep secrets out of source control
- How to test authentication and authorization behavior
- How to discuss OWASP Top 10 risks in practical API terms

The objective is not to build a full identity platform. The objective is to add a realistic security boundary to Zapas and explain the tradeoffs clearly.

---

## 5-Hour Structure

| Activity | Time |
|---|---:|
| Build/refactor code | 2 hours |
| Technical review | 1 hour |
| Live coding | 1 hour |
| Interview speaking practice | 1 hour |

Main rule for today:

> Never trust the client. Validate identity, authorization, input, file size, file type, and resource ownership on the server.

---

## Hour 1-2 — Build / Refactor Code

### Target Area

Work inside the current Zapas API:

- `Zapas.Api/Zapas.Api.csproj`
- `Zapas.Api/Program.cs`
- `Zapas.Api/appsettings.json`
- `Zapas.Api/Controllers/SessionsController.cs`
- `Zapas.Api/Services/Sessions/ISessionService.cs`
- `Zapas.Api/Services/Sessions/SessionService.cs`
- `Zapas.Api/Repositories/ISessionRepository.cs`
- `Zapas.Api/Repositories/SessionRepository.cs`
- `Zapas.Api/Models/Session.cs`
- `Zapas.Api/Entities/SessionEntity.cs`
- `Zapas.Api/DTOs/`
- `Zapas.Api/Middleware/GlobalExceptionMiddleware.cs`
- `Zapas.Api.Tests/`

Current good baseline:

- `GET /sessions` returns sessions.
- `GET /sessions/{id}` returns one session.
- `POST /sessions` accepts a `.fit` file.
- Controllers are thin and delegate workflows to services.
- FIT parsing is hidden behind `IFitSessionParser`.
- Persistence is hidden behind `ISessionRepository`.
- Global exception handling, request logging, rate limiting, health checks, EF Core persistence, and tests may already exist from previous days.

Today you will implement a concrete security slice:

- JWT bearer authentication package
- JWT configuration
- Authorization policies
- Protected session endpoints
- Upload options and stricter validation
- CORS policy
- Current-user abstraction
- Ownership plan for session data
- Security tests

---

## Task 1 — Add The JWT Authentication Package

### Why

The API needs the JWT bearer authentication handler before ASP.NET Core can validate access tokens.

### Implementation

Run this from the repository root:

```bash
dotnet add Zapas.Api/Zapas.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.7
```

Expected `Zapas.Api/Zapas.Api.csproj` addition:

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.7" />
```

Do not add ASP.NET Core Identity today. Zapas is a resource API in this exercise. It validates tokens issued by an identity provider; it does not issue tokens, store passwords, or manage refresh tokens.

### Interview Explanation

> "For Zapas, I add the JWT bearer handler because the API needs to validate access tokens. I would only add ASP.NET Core Identity if Zapas needed to manage users or issue tokens itself."

---

## Task 2 — Add Security Configuration

### Why

Authentication, CORS, and upload limits should be configurable. Do not hard-code production security settings in controllers or services.

### Implementation

Update `Zapas.Api/appsettings.json`:

```json
{
  "Jwt": {
    "Authority": "https://identity.example.com",
    "Audience": "zapas-api"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://app.zapas.example"
    ]
  },
  "Uploads": {
    "MaxFitFileSizeBytes": 10485760,
    "AllowedExtensions": [
      ".fit"
    ]
  }
}
```

Use placeholders in committed config. Put real environment-specific values in user secrets, environment variables, or a cloud secret store.

For local development:

```bash
dotnet user-secrets init --project Zapas.Api/Zapas.Api.csproj
dotnet user-secrets set "Jwt:Authority" "https://dev-identity.example.com" --project Zapas.Api/Zapas.Api.csproj
dotnet user-secrets set "Jwt:Audience" "zapas-api" --project Zapas.Api/Zapas.Api.csproj
```

### Interview Explanation

> "I keep security settings in configuration and secrets outside source control. Local development can use user secrets, while production should use environment variables, managed identity, or a secret store."

---

## Task 3 — Add Upload Options

### Why

Upload limits are part of the security boundary. They should be centralized and injected instead of duplicated as magic numbers.

### Implementation

Create `Zapas.Api/Options/UploadOptions.cs`:

```csharp
namespace Zapas.Api.Options;

public sealed class UploadOptions
{
    public const string SectionName = "Uploads";

    public long MaxFitFileSizeBytes { get; init; } = 10 * 1024 * 1024;

    public string[] AllowedExtensions { get; init; } = [".fit"];
}
```

Register it in `Program.cs`:

```csharp
builder.Services.Configure<UploadOptions>(
    builder.Configuration.GetSection(UploadOptions.SectionName));
```

Add this using:

```csharp
using Zapas.Api.Options;
```

### Interview Explanation

> "I move upload limits into options so the file upload security rules are explicit, configurable, and easy to test."

---

## Task 4 — Wire Authentication, Authorization, And CORS

### Why

Authentication and authorization need both service registration and middleware. Middleware order matters.

### Implementation

Update `Zapas.Api/Program.cs`.

Add usings:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Zapas.Api.Options;
```

Add authentication:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });
```

Add authorization policies:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanReadSessions", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("CanUploadSession", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Athlete");
    });

    options.AddPolicy("CanDeleteSession", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("Admin");
    });
});
```

Add CORS:

```csharp
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ZapasFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "DELETE")
              .WithHeaders("Authorization", "Content-Type");
    });
});
```

Add middleware in this order:

```csharp
app.UseHttpsRedirection();
app.UseCors("ZapasFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
```

Keep existing exception and request logging middleware near the top of the pipeline.

### Interview Explanation

> "Authentication must run before authorization so ASP.NET Core can build the user principal before policies are evaluated. CORS is configured with explicit frontend origins because it is not an authentication mechanism."

---

## Task 5 — Protect Session Endpoints

### Why

Running activity data is private. Session endpoints should require authentication by default.

### Implementation

Update `Zapas.Api/Controllers/SessionsController.cs`.

Add using:

```csharp
using Microsoft.AspNetCore.Authorization;
```

Protect the controller:

```csharp
[Authorize(Policy = "CanReadSessions")]
[ApiController]
[Route("sessions")]
public sealed class SessionsController : ControllerBase
{
}
```

Apply the upload policy to `POST /sessions`:

```csharp
[Authorize(Policy = "CanUploadSession")]
[RequestSizeLimit(10 * 1024 * 1024)]
[HttpPost]
public async Task<ActionResult<SessionResponse>> CreateSession(
    IFormFile file,
    CancellationToken cancellationToken)
{
    // Existing upload workflow.
}
```

If `DELETE /sessions/{id}` exists, protect it:

```csharp
[Authorize(Policy = "CanDeleteSession")]
[HttpDelete("{id:guid}")]
public async Task<IActionResult> DeleteSession(
    Guid id,
    CancellationToken cancellationToken)
{
    // Delete workflow.
}
```

### Interview Explanation

> "I protect the controller by default because sessions are private. Upload and delete operations get stricter policies because they are more sensitive than reads."

---

## Task 6 — Add A Current User Abstraction

### Why

Services need a testable way to access the authenticated user's id. Do not pass `userId` from the request body or query string.

### Implementation

Create `Zapas.Api/Services/CurrentUser/ICurrentUser.cs`:

```csharp
namespace Zapas.Api.Services.CurrentUser;

public interface ICurrentUser
{
    string? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
```

Create `Zapas.Api/Services/CurrentUser/HttpCurrentUser.cs`:

```csharp
using System.Security.Claims;

namespace Zapas.Api.Services.CurrentUser;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
}
```

Register it in `Program.cs`:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
```

Add using:

```csharp
using Zapas.Api.Services.CurrentUser;
```

### Interview Explanation

> "I read the user id from validated claims, not from client-submitted fields. Wrapping it in `ICurrentUser` keeps services testable."

---

## Task 7 — Add Resource Ownership

### Why

Authentication proves who the caller is. Authorization still has to decide whether that caller can access a specific session.

### Implementation

Update `Zapas.Api/Models/Session.cs`:

```csharp
public string OwnerUserId { get; init; } = string.Empty;
```

Update `Zapas.Api/Entities/SessionEntity.cs`:

```csharp
public string OwnerUserId { get; set; } = string.Empty;
```

Update repository mapping so `OwnerUserId` is stored and returned.

Update the create-session workflow in `SessionService`:

```csharp
if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUser.UserId))
{
    throw new UnauthorizedAccessException("An authenticated user is required.");
}

var session = parsedSession with
{
    OwnerUserId = _currentUser.UserId
};
```

Update read queries so regular users only see their own sessions:

```csharp
var query = _dbContext.Sessions
    .AsNoTracking();

if (!_currentUser.IsInRole("Admin"))
{
    query = query.Where(session => session.OwnerUserId == _currentUser.UserId);
}
```

When using SQLite or EF migrations, add a migration after the entity changes:

```bash
dotnet ef migrations add AddSessionOwner --project Zapas.Api/Zapas.Api.csproj
dotnet ef database update --project Zapas.Api/Zapas.Api.csproj
```

### Interview Explanation

> "The key authorization rule is not only whether the user is logged in. A normal user should see only their own sessions, while an admin can be explicitly allowed to see more."

---

## Task 8 — Strengthen Upload Validation

### Why

FIT upload accepts untrusted input and passes it to an external parser. Validation should happen before parsing.

### Implementation

Inject upload options into `SessionService`:

```csharp
using Microsoft.Extensions.Options;
using Zapas.Api.Options;

private readonly UploadOptions _uploadOptions;

public SessionService(
    ISessionRepository sessionRepository,
    IFitSessionParser fitSessionParser,
    IOptions<UploadOptions> uploadOptions,
    ILogger<SessionService> logger)
{
    _sessionRepository = sessionRepository;
    _fitSessionParser = fitSessionParser;
    _uploadOptions = uploadOptions.Value;
    _logger = logger;
}
```

Validate before parsing:

```csharp
if (fileLength <= 0)
{
    return CreateSessionResult.Rejected("A non-empty .fit file is required.");
}

if (fileLength > _uploadOptions.MaxFitFileSizeBytes)
{
    return CreateSessionResult.Rejected("The uploaded file is too large.");
}

var extension = Path.GetExtension(fileName);

if (!_uploadOptions.AllowedExtensions.Contains(
        extension,
        StringComparer.OrdinalIgnoreCase))
{
    return CreateSessionResult.Rejected("Only .fit files are supported.");
}
```

Handle parser failures with a controlled response:

```csharp
try
{
    var session = _fitSessionParser.Parse(fitStream, fileName);
    // Store session.
}
catch (InvalidDataException ex)
{
    _logger.LogWarning(
        ex,
        "Failed to parse FIT upload {FileName} with size {FileSizeBytes}.",
        fileName,
        fileLength);

    return CreateSessionResult.Rejected("The uploaded FIT file could not be parsed.");
}
```

Do not log raw file contents, tokens, authorization headers, or connection strings.

### Interview Explanation

> "I treat uploads as hostile input. The service checks size and extension before parsing, handles parser failures as controlled client errors, and logs metadata without leaking file contents."

---

## Task 9 — Update Swagger For Bearer Tokens

### Why

Swagger should make the security requirement visible and let you test protected endpoints locally.

### Implementation

Update Swagger registration in `Program.cs`:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a JWT bearer token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});
```

Add using:

```csharp
using Microsoft.OpenApi.Models;
```

### Interview Explanation

> "Swagger security configuration does not protect the API by itself. It documents the bearer token requirement and makes local testing easier."

---

## Task 10 — Add Security Tests

### Why

Authentication and authorization behavior should be tested at the HTTP boundary.

### Implementation

Add upload validation unit tests:

```text
Rejects empty file
Rejects non-.fit extension
Rejects oversized file
Does not call parser when validation fails
Returns controlled error when parser throws InvalidDataException
```

Add integration tests:

```text
GET /sessions without token returns 401
POST /sessions without token returns 401
POST /sessions with authenticated non-Athlete user returns 403
POST /sessions with Athlete role reaches upload validation
GET /health returns 200 without token
```

Use a test authentication handler instead of a real identity provider:

```csharp
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Role, "Athlete")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

Required usings:

```csharp
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
```

### Interview Explanation

> "I test authorization with a fake authentication handler. That lets me verify 401 and 403 behavior without depending on a real identity provider in tests."

---

## Task 11 — Add A Security Decision Note

### Why

Security decisions are easy to scatter across middleware, controllers, services, and configuration. Write them down so you can explain the implementation clearly.

### Implementation

Create or update your interview notes with:

```markdown
## Security Decisions For Zapas

### Decisions

- Zapas validates JWT bearer tokens issued by an external identity provider.
- Session endpoints require authenticated users.
- Upload endpoints require the `CanUploadSession` policy.
- Admin-only operations use the `CanDeleteSession` policy.
- Session reads must enforce owner access or admin access.
- FIT uploads have file size and file extension validation.
- Parser failures return controlled client errors.
- Secrets are stored outside source control.
- CORS allows only known frontend origins.

### Tradeoffs

- Zapas validates access tokens but does not implement a full identity provider.
- Roles are simple, but ownership checks require resource-based authorization or owner-filtered queries.
- File extension checks are useful but not sufficient on their own.
- Parser exception details are logged internally but not returned to clients.
```

### Interview Explanation

> "I document security decisions because they cut across configuration, middleware, controllers, services, and persistence. A short decision note makes the security model easier to review."

---

## Hour 3 — Technical Review

### Authentication vs Authorization

Authentication answers:

```text
Who are you?
```

Authorization answers:

```text
What are you allowed to do?
```

Example:

```text
Authentication:
  The token proves this caller is user 123.

Authorization:
  User 123 can read session A because they own it.
  User 123 cannot delete session B because they are not an admin.
```

Strong interview phrasing:

> "Authentication establishes identity. Authorization applies rules to that identity."

---

### JWT

A JWT is a signed token containing claims.

Common claims:

- `sub`: subject or user id
- `name`: user display name
- `email`: user email
- `role`: role claims
- `iss`: issuer
- `aud`: audience
- `exp`: expiration

Important validation:

- Signature
- Issuer
- Audience
- Expiration
- Algorithm expectations

JWT tradeoffs:

- Good for stateless API authentication.
- Can be validated without a database lookup.
- Harder to revoke immediately unless using short lifetimes, introspection, or revocation lists.
- Should not contain sensitive data unless encrypted, because normal JWT payloads are only base64url encoded, not secret.

Strong interview phrasing:

> "A JWT is signed, not automatically encrypted. I do not put secrets in it. The API must validate signature, issuer, audience, and expiration before trusting claims."

---

### OAuth2

OAuth2 is an authorization framework.

It is about delegated access:

```text
The client gets permission to call an API on behalf of a user or service.
```

Important ideas:

- Authorization server
- Resource server
- Client
- Access token
- Refresh token
- Scopes

For Zapas:

```text
Identity provider:
  Issues tokens

Zapas API:
  Validates tokens and enforces authorization

Frontend app:
  Gets tokens and calls the API
```

Strong interview phrasing:

> "OAuth2 is about delegated access. In a typical API setup, Zapas is the resource server that validates access tokens issued by an authorization server."

---

### OpenID Connect

OpenID Connect builds identity on top of OAuth2.

OAuth2:

```text
Can this client access this API?
```

OpenID Connect:

```text
Who is the user?
```

OIDC adds:

- ID token
- Standard identity claims
- UserInfo endpoint
- Authentication semantics

Strong interview phrasing:

> "OAuth2 is for delegated authorization. OpenID Connect adds authentication and identity information on top of OAuth2."

---

### Claims

Claims are facts about the authenticated principal.

Examples:

```text
sub = 123
email = runner@example.com
role = Athlete
scope = sessions:read
```

Important warning:

```text
Claims are only trustworthy after the token has been validated.
```

Good Zapas claim usage:

- Use `sub` or name identifier as owner id.
- Use role or scope claims for broad permissions.
- Avoid trusting user id from request body.

Strong interview phrasing:

> "Claims are inputs to authorization decisions. They are not inherently trustworthy unless they came from a validated token issued by a trusted authority."

---

### Roles vs Policies

Roles:

- Simple
- Familiar
- Good for broad permissions
- Can become too coarse

Policies:

- Named authorization rules
- Can combine roles, claims, scopes, and custom requirements
- Better for complex permissions
- Easier to test explicitly

Resource-based authorization:

- Best when the decision depends on the specific resource
- Example: user can read only sessions they own

Strong interview phrasing:

> "I use roles for coarse permissions and policies when the rule needs to be named, composed, or tested. For ownership checks, I use resource-based authorization or enforce ownership in the data query."

---

### Refresh Tokens

Access tokens should usually be short-lived.

Refresh tokens allow the client to obtain new access tokens without making the user sign in again.

Important protections:

- Store refresh tokens carefully.
- Rotate refresh tokens.
- Detect reuse.
- Revoke on logout or compromise.
- Do not send refresh tokens to APIs that do not need them.

For Zapas:

```text
The identity provider owns refresh token handling.
Zapas API validates access tokens only.
```

Strong interview phrasing:

> "In a resource API, I generally do not handle refresh tokens directly. The identity provider handles refresh flows, and the API validates access tokens."

---

### CORS

CORS controls browser access across origins.

It does not:

- Authenticate users
- Authorize requests
- Protect non-browser clients
- Replace server-side validation

Strong interview phrasing:

> "CORS is not an auth feature. It is a browser-enforced cross-origin access policy. The API still needs authentication, authorization, and validation."

---

### CSRF

CSRF tricks a browser into sending an authenticated request.

Main risk:

```text
Browser automatically attaches cookies to requests.
```

Mitigations:

- Anti-forgery tokens
- `SameSite` cookies
- Requiring custom headers
- Avoiding cookie-based auth for APIs when bearer tokens fit better

Strong interview phrasing:

> "CSRF mainly matters when credentials are automatically sent by the browser, especially cookies. Bearer-token APIs reduce that risk, but cookie-based APIs need anti-forgery protections."

---

### XSS

XSS lets attacker-controlled script run in a user's browser.

API-side practices:

- Return JSON with correct content type.
- Avoid reflecting unsanitized input into HTML.
- Validate fields.
- Do not store unnecessary HTML.
- Avoid exposing tokens to JavaScript when the threat model requires stronger protection.

Strong interview phrasing:

> "XSS is often a frontend rendering issue, but APIs should still avoid returning unsafe content, validate stored input, and be careful with token storage recommendations."

---

### SQL Injection

SQL injection happens when untrusted input becomes executable SQL.

EF Core LINQ normally parameterizes values.

Risk returns with:

- String-concatenated SQL
- Unvalidated raw SQL fragments
- Directly using client sort fields as SQL
- Dynamic query builders without whitelists

Strong interview phrasing:

> "ORMs help, but they do not make SQL injection impossible. I still avoid string-concatenated SQL and whitelist dynamic filter or sort fields."

---

### Secrets Management

Never commit:

- Production connection strings
- JWT signing keys
- Client secrets
- API keys
- Passwords
- Tokens

Local development:

```text
dotnet user-secrets
```

Deployment:

- Environment variables
- Cloud secret stores
- Managed identity where available
- Key rotation

Strong interview phrasing:

> "Secrets should come from configuration providers outside source control. Locally I use user secrets. In production I use environment variables, cloud secret stores, or managed identity depending on the platform."

---

### HTTPS

HTTPS protects data in transit.

Use:

- HTTPS redirection
- HSTS in production
- Secure cookies if cookies are used
- TLS at the edge and between services when needed

Strong interview phrasing:

> "Authentication tokens and uploaded files should only travel over HTTPS. In production I enable HSTS and make sure cookies, if used, are marked Secure."

---

### Secure Headers

Common useful headers:

- `Strict-Transport-Security`
- `X-Content-Type-Options: nosniff`
- `Content-Security-Policy`
- `Referrer-Policy`
- `X-Frame-Options` or CSP `frame-ancestors`

For APIs:

- `X-Content-Type-Options` is often useful.
- CSP matters more when serving browser-rendered pages.
- HSTS matters for HTTPS sites.

Strong interview phrasing:

> "Security headers are useful defense-in-depth, but I apply the ones that match what the app serves. A JSON API has different needs than a server-rendered web app."

---

### OWASP Top 10 Basics

Connect OWASP risks to concrete Zapas decisions:

```text
Broken Access Control
  -> enforce session ownership and admin policies

Cryptographic Failures
  -> use HTTPS and protect secrets

Injection
  -> use EF parameterized queries and whitelist sort fields

Insecure Design
  -> define authorization model before coding endpoints

Security Misconfiguration
  -> avoid permissive CORS and debug errors in production

Vulnerable Components
  -> keep NuGet packages updated

Identification and Authentication Failures
  -> validate JWT issuer, audience, signature, and expiration

Logging and Monitoring Failures
  -> log security-relevant events without leaking secrets
```

Strong interview phrasing:

> "I use OWASP as a checklist to pressure-test the design: access control, injection, secrets, misconfiguration, vulnerable dependencies, and logging are the areas I check first for an API."

---

## Hour 4 — Live Coding

Spend this hour practicing security-shaped coding tasks. Talk through assumptions and edge cases while coding.

---

### Exercise 1 — Validate Upload Metadata

Implement a function that validates file name and size.

Requirements:

- File name is required.
- Extension must be `.fit`.
- Size must be greater than 0.
- Size must be less than or equal to a configured maximum.
- Return clear validation errors.

Use this signature:

```csharp
public static IReadOnlyList<string> ValidateFitUpload(
    string fileName,
    long fileSizeBytes,
    long maxFileSizeBytes)
```

Example cases:

```text
activity.fit, 1000, 10000000
  -> valid

activity.txt, 1000, 10000000
  -> invalid extension

activity.fit, 0, 10000000
  -> empty file

activity.fit, 20000000, 10000000
  -> file too large
```

Explain:

- Time complexity: `O(1)`
- Space complexity: `O(k)` where `k` is number of validation errors
- Extension checks are useful but not sufficient for production

---

### Exercise 2 — Parse Authorization Header

Implement a function that extracts a bearer token.

Requirements:

- Accept header value.
- Return token only if the scheme is `Bearer`.
- Ignore leading and trailing whitespace.
- Reject empty token.
- Be case-insensitive for scheme.

Use this signature:

```csharp
public static string? TryGetBearerToken(string? authorizationHeader)
```

Example cases:

```text
Bearer abc123
  -> abc123

bearer abc123
  -> abc123

Basic abc123
  -> null

Bearer
  -> null

null
  -> null
```

Explain:

- Real applications should use ASP.NET Core authentication middleware.
- This exercise is for string parsing and edge cases, not for manually validating JWTs.

---

### Exercise 3 — Check Resource Ownership

Implement a function that decides whether a user can access a session.

Rules:

- Authenticated admin can access any session.
- Authenticated owner can access their own session.
- Everyone else is denied.
- Missing user id is denied.

Use this signature:

```csharp
public static bool CanReadSession(
    string? currentUserId,
    string sessionOwnerUserId,
    IReadOnlyCollection<string> roles)
```

Example cases:

```text
currentUserId=user-1, sessionOwnerUserId=user-1, roles=[]
  -> true

currentUserId=user-2, sessionOwnerUserId=user-1, roles=[]
  -> false

currentUserId=user-2, sessionOwnerUserId=user-1, roles=[Admin]
  -> true

currentUserId=null, sessionOwnerUserId=user-1, roles=[Admin]
  -> false
```

Explain:

- Roles must come from a validated principal.
- Ownership checks should not trust a user id supplied by the client.

---

### Exercise 4 — Whitelist Sort Fields

Implement a function that maps a requested sort field to a known internal field.

Requirements:

- Accept sort field from query string.
- Default to `CreatedAt`.
- Support `createdAt`, `distance`, and `duration`.
- Reject unknown fields or fall back safely.

Use this signature:

```csharp
public static string ResolveSessionSortField(string? requestedSortField)
```

Allowed mapping:

```text
createdAt -> CreatedAt
distance -> DistanceMeters
duration -> Duration
```

Explain:

- Never pass arbitrary query string values into raw SQL.
- A whitelist prevents injection and unexpected expensive queries.

---

### Exercise 5 — Rate Limiter Key

Implement a function that chooses a rate-limiting key.

Rules:

- Prefer authenticated user id.
- Fall back to IP address.
- Return a stable key prefix.

Use this signature:

```csharp
public static string BuildRateLimitKey(string? userId, string? ipAddress)
```

Example cases:

```text
user-123, 203.0.113.10
  -> user:user-123

null, 203.0.113.10
  -> ip:203.0.113.10

null, null
  -> anonymous:unknown
```

Explain:

- User-based limits are usually more accurate for authenticated APIs.
- IP-based limits are useful for anonymous traffic but can affect shared networks.

---

## Hour 5 — Interview Speaking Practice

### Questions

Practice answering these out loud:

1. What is the difference between authentication and authorization?
2. How does JWT authentication work?
3. What does it mean to validate JWT issuer and audience?
4. What is the difference between OAuth2 and OpenID Connect?
5. What are claims?
6. When would you use roles versus policies?
7. How would you implement resource ownership checks?
8. Where should JWTs be stored?
9. What are refresh tokens?
10. What is CORS?
11. What is CSRF?
12. How do bearer-token APIs reduce CSRF risk?
13. What is XSS?
14. How do you prevent SQL injection in .NET?
15. How do you store secrets in development and production?
16. What makes file uploads risky?
17. How would you secure `POST /sessions`?
18. How would you prevent one user from reading another user's sessions?
19. What should you avoid logging?
20. What OWASP Top 10 risks are most relevant to this API?

### Strong Answer — Authentication vs Authorization

> "Authentication proves who the caller is. Authorization decides what that caller is allowed to do. In Zapas, a JWT might authenticate the caller as user 123, but authorization still needs to decide whether user 123 can read a specific session."

### Strong Answer — JWT

> "A JWT is a signed token containing claims. For an API, I validate the signature, issuer, audience, and expiration before trusting those claims. I also avoid putting secrets in JWT payloads because signed tokens are not automatically encrypted."

### Strong Answer — OAuth2 And OpenID Connect

> "OAuth2 is about delegated access to resources. OpenID Connect adds identity and authentication on top of OAuth2. In Zapas, the API would usually act as a resource server that validates access tokens from an identity provider."

### Strong Answer — Roles vs Policies

> "Roles are useful for coarse permissions like Admin or Athlete. Policies are better when the rule needs to combine roles, claims, scopes, or custom requirements. For session ownership, I would use resource-based authorization or enforce ownership in the query."

### Strong Answer — CORS

> "CORS is a browser-enforced cross-origin policy. It decides which frontend origins can call the API from browser JavaScript. It does not replace authentication or authorization, and it does not protect against non-browser clients."

### Strong Answer — CSRF

> "CSRF is mainly a risk when browsers automatically send credentials like cookies. Bearer-token APIs reduce that risk because a malicious site cannot automatically attach the Authorization header, but cookie-based APIs still need anti-forgery protections and careful SameSite settings."

### Strong Answer — File Upload Security

> "For FIT uploads, I treat the file as hostile input. I require authentication, limit file size, validate extension and content shape, handle parser failures safely, and avoid logging raw file contents."

### Strong Answer — Secrets Management

> "Secrets should not be in source control. Locally I use user secrets. In production I use environment variables, a cloud secret store, or managed identity depending on the platform. I also avoid logging secrets or exposing them through error responses."

### Strong Answer — SQL Injection

> "EF Core LINQ normally parameterizes queries, which helps prevent SQL injection. I still avoid string-concatenated SQL and whitelist client-provided fields like sorting or filtering values."

### Strong Answer — Secure Error Handling

> "Public error responses should be useful but not revealing. I log enough structured detail internally to debug the problem, but I do not return stack traces, tokens, file contents, connection strings, or internal paths to clients."

### Behavioral Practice

Prepare short answers for:

- Tell me about a time you improved application security.
- Tell me about a time you found an authorization bug.
- Tell me about a time you had to explain a security risk to a non-security audience.
- Tell me about a time you had to balance security and usability.
- Tell me about a time bad input caused a production issue.
- Tell me about a time logs helped or hurt debugging.
- Tell me about a time you handled secrets or configuration safely.

### "Security Improvement" Draft

> "One security improvement I focus on is making authorization explicit. It is easy to check only whether a user is logged in, but senior-level API security also checks whether that user can access that specific resource. In Zapas, that means validating JWTs, requiring policies for upload and admin operations, and enforcing session ownership before returning private activity data."

---

## Day 8 Deliverable

At the end of today, Zapas should have:

- `Microsoft.AspNetCore.Authentication.JwtBearer` added to `Zapas.Api`
- JWT authority and audience configuration
- Upload options bound from configuration
- Authentication and authorization registered in `Program.cs`
- `UseAuthentication()` before `UseAuthorization()`
- Named policies for reading, uploading, and deleting sessions
- Session endpoints protected with `[Authorize]`
- Health check endpoint left anonymous
- Explicit CORS policy for known frontend origins
- Upload validation using configured file size and extension rules
- Safe parser failure responses
- A current-user abstraction for reading user id and roles from claims
- Session ownership field added or clearly prepared
- Tests for upload validation and protected endpoint behavior
- No secrets committed to source control

Write notes for yourself:

- One difference between authentication and authorization
- One reason roles are not enough for ownership checks
- One reason CORS is not an auth feature
- One risk with storing tokens in local storage
- One risk with cookie-based auth
- One file upload protection Zapas needs
- One thing that should never be logged
- One OWASP risk that applies directly to Zapas

---

## Final Interview Framing

Use this as your Day 8 summary:

> "I added a security boundary to Zapas. The API validates JWT bearer tokens from a trusted identity provider, uses policies for session read, upload, and delete permissions, and treats FIT uploads as hostile input with size limits, extension validation, controlled parser errors, and careful logging. I also prepared the API for user-owned session data by reading the user id from validated claims instead of trusting client input. I can explain JWTs, OAuth2, OpenID Connect, claims, roles, policies, CORS, CSRF, XSS, SQL injection, secrets management, HTTPS, secure headers, and OWASP risks in practical API terms."
