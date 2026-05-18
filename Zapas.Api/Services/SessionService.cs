using Microsoft.Extensions.Caching.Memory;
using Dynastream.Fit;
using Zapas.Api.DTOs;
using Zapas.Api.Models;
using Zapas.Api.Repositories;

namespace Zapas.Api.Services;

public sealed class SessionService : ISessionService
{
    private static readonly TimeSpan SessionCacheDuration = TimeSpan.FromMinutes(5);
    private const long MaxFitFileSizeBytes = 10 * 1024 * 1024;
    private readonly ISessionRepository _sessionRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        ISessionRepository sessionRepository,
        IMemoryCache cache,
        ILogger<SessionService> logger)
    {
        _sessionRepository = sessionRepository;
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

    public Session ExtractSessionInfo(Stream fitStream, string? fallbackName = null)
    {
        var decoder = new Decode();
        var broadcaster = new MesgBroadcaster();
        var intervals = new List<RunInterval>();
        SessionMesg? session = null;

        decoder.MesgEvent += broadcaster.OnMesg;
        decoder.MesgDefinitionEvent += broadcaster.OnMesgDefinition;

        broadcaster.SessionMesgEvent += (_, args) =>
        {
            if (args.mesg is SessionMesg sessionMesg)
            {
                session = sessionMesg;
            }
        };

        broadcaster.LapMesgEvent += (_, args) =>
        {
            if (args.mesg is not LapMesg lap)
            {
                return;
            }

            if (lap.GetSport() != Sport.Running || lap.GetIntensity() != Intensity.Active)
            {
                return;
            }

            var distance = lap.GetTotalDistance();
            var duration = lap.GetTotalTimerTime();

            if (distance is null || duration is null || distance <= 0 || duration <= 0)
            {
                return;
            }

            intervals.Add(new RunInterval(
                Distance: distance.Value,
                Duration: TimeSpan.FromSeconds(duration.Value),
                AverageHeartRate: lap.GetAvgHeartRate(),
                MaxHeartRate: lap.GetMaxHeartRate(),
                Pace: TimeSpan.FromSeconds(duration.Value / (distance.Value / 1000))));
        };

        decoder.Read(fitStream);

        var totalDistance = session?.GetTotalDistance();
        var totalTime = session?.GetTotalTimerTime();

        return new Session(
            Id: Guid.NewGuid(),
            Name: GetSessionName(session, fallbackName),
            TotalDistance: totalDistance ?? 0,
            TotalDuration: totalTime is null ? TimeSpan.Zero : TimeSpan.FromSeconds(totalTime.Value),
            AveragePace: GetPace(totalDistance, totalTime) ?? TimeSpan.Zero,
            AverageHeartRate: session?.GetAvgHeartRate(),
            MaxHeartRate: session?.GetMaxHeartRate(),
            StartTime: ToUtcDateTimeOffset(session?.GetStartTime()?.GetDateTime()) ?? DateTimeOffset.UtcNow,
            CreatedAt: DateTimeOffset.UtcNow,
            RunIntervals: intervals);
    }

    private static TimeSpan? GetPace(float? distance, float? duration)
    {
        if (distance is null || duration is null || distance <= 0 || duration <= 0)
        {
            return null;
        }

        return TimeSpan.FromSeconds(duration.Value / (distance.Value / 1000));
    }

    private static DateTimeOffset? ToUtcDateTimeOffset(System.DateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        var dateTime = value.Value.Kind == DateTimeKind.Unspecified
            ? System.DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            : value.Value.ToUniversalTime();

        return new DateTimeOffset(dateTime, TimeSpan.Zero);
    }

    private static string GetSessionName(SessionMesg? session, string? fallbackName)
    {
        var name = session?.GetSportProfileNameAsString();

        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        var fileName = Path.GetFileNameWithoutExtension(fallbackName);

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            return fileName;
        }

        return session?.GetSport()?.ToString() ?? "Unknown session";
    }
}
