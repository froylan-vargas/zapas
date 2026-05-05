using Dynastream.Fit;
using Zapas.Api.DTOs;
using Zapas.Api.Models;
using Zapas.Api.Repositories;

namespace Zapas.Api.Services;

public sealed class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;

    public SessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<IReadOnlyList<SessionSummary>> GetSessionsAsync(
        GetSessionsRequestDto request,
        CancellationToken cancellationToken)
    {
        return await _sessionRepository.GetSessionsAsync(request, cancellationToken);
    }

    public async Task<Session?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _sessionRepository.GetSessionByIdAsync(id, cancellationToken);
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

        Session session;

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

        var storedSession = await _sessionRepository.AddSessionAsync(session, cancellationToken);

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
