using Microsoft.Extensions.Caching.Memory;
using Dynastream.Fit;
using Zapas.Api.DTOs;
using Zapas.Api.Models;
using Zapas.Api.Repositories;

namespace Zapas.Api.Services;

public sealed class SessionService : ISessionService
{
    private static readonly TimeSpan SessionCacheDuration = TimeSpan.FromMinutes(5);
    private const long MaxFitFileSizeBytes = 3 * 1024 * 1024;
    private readonly ISessionRepository _sessionRepository;
    private readonly IFitSessionParser _fitSessionParser;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        ISessionRepository sessionRepository,
        IFitSessionParser fitSessionParser,
        IMemoryCache cache,
        ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository;
        _fitSessionParser = fitSessionParser;
        _cache = cache;
        _logger = logger;
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
        if (fileLength <= 0)
        {
            return new CreateSessionResult(
                CreateSessionState.Rejected,
                Session: null,
                Error: "A non-empty .fit file is required.");
        }

        if (!string.Equals(Path.GetExtension(fileName), ".fit", StringComparison.OrdinalIgnoreCase))
        {
            return new CreateSessionResult(
                CreateSessionState.Rejected,
                Session: null,
                Error: "Only .fit files are supported.");
        }

        if (fileLength > MaxFitFileSizeBytes)
        {
            return new CreateSessionResult(
                CreateSessionState.Rejected,
                Session: null,
                Error: "The uploaded file is too large.");
        }

        Session session;

        cancellationToken.ThrowIfCancellationRequested();

        var parsedStartedAt = TimeProvider.System.GetTimestamp();

        try
        {
            session = _fitSessionParser.Parse(fitStream, fileName);
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
            var elapsed = TimeProvider.System.GetElapsedTime(parsedStartedAt);
            _logger.LogInformation("Parsed FIT file in {ElapsedSeconds} ms for {FileName}", elapsed.TotalMicroseconds, fileName);
        }

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
