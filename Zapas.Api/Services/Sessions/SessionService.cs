using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Zapas.Api.DTOs;
using Zapas.Api.Models;
using Zapas.Api.Options;
using Zapas.Api.Repositories;
using Zapas.Api.Services.CurrentUser;
using Zapas.Api.Services.FitParser;

namespace Zapas.Api.Services.Sessions;

public sealed class SessionService : ISessionService
{
    private static readonly TimeSpan SessionCacheDuration = TimeSpan.FromMinutes(5);
    private readonly ISessionRepository _sessionRepository;
    private readonly IFitSessionParser _fitSessionParser;
    private readonly ICurrentUser _currentUser;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SessionService> _logger;
    private readonly UploadOptions _uploadOptions;

    public SessionService(
        ISessionRepository sessionRepository,
        IFitSessionParser fitSessionParser,
        IOptions<UploadOptions> uploadOptions,
        ICurrentUser currentUser,
        IMemoryCache cache,
        ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _fitSessionParser = fitSessionParser;
        _currentUser = currentUser;
        _cache = cache;
        _logger = logger;
        _uploadOptions = uploadOptions.Value;
    }

    public async Task<IReadOnlyList<SessionSummary>> GetSessionsAsync(
        GetSessionsRequestDto request,
        CancellationToken cancellationToken)
    {
        return await _sessionRepository.GetSessionsAsync(request, cancellationToken);
    }

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
                }
            );
        }

        return session;
    }

    public async Task<CreateSessionResult> CreateSessionAsync(
        Stream fitStream,
        string? fileName,
        long fileLength,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrEmpty(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException("An authenticated user is required");
        }

        if (fileLength <= 0)
        {
            return new CreateSessionResult(
                CreateSessionState.Rejected,
                Session: null,
                Error: "A non-empty .fit file is required.");
        }

        var extension = Path.GetExtension(fileName);

        if (!_uploadOptions.AllowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase))
        {
            return new CreateSessionResult(
                CreateSessionState.Rejected,
                Session: null,
                Error: "Only .fit files are supported.");
        }

        if (fileLength > _uploadOptions.MaxFitFileSizeBytes)
        {
            return new CreateSessionResult(
                CreateSessionState.Rejected,
                Session: null,
                Error: "The uploaded file is too large.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        Session parsedSession;
        var parsedStartedAt = TimeProvider.System.GetTimestamp();

        try
        {
            parsedSession = _fitSessionParser.Parse(fitStream, fileName);
        }
        catch (InvalidDataException ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to parse FIT upload {FileName} with size {FileSizeBytes}.",
                fileName,
                fileLength
            );

            return new CreateSessionResult(
                CreateSessionState.Failed,
                Session: null,
                Error: "The uploaded file could not be parsed as a valid FIT activity.");
        }
        finally
        {
            var elapsed = TimeProvider.System.GetElapsedTime(parsedStartedAt);
            _logger.LogInformation("Parsed FIT file in {ElapsedSeconds} ms for {FileName}", elapsed.TotalMicroseconds, fileName);
        }

        var session = parsedSession with
        {
            OwnerUserId = _currentUser.UserId,
        };

        cancellationToken.ThrowIfCancellationRequested();

        var storedSession = await _sessionRepository.AddSessionAsync(session, cancellationToken);

        _cache.Set(
            $"session:{storedSession.Id}",
            storedSession,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = SessionCacheDuration
            }
        );

        return new CreateSessionResult(
        CreateSessionState.Stored,
        storedSession,
        Error: null);
    }
}
