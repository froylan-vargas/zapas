using Dynastream.Fit;
using Zapas.Api.Models;

namespace Zapas.Api.Services;

public class FitSessionParser : IFitSessionParser
{
    public Session Parse(Stream fitStream, string? fallbackName)
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

    internal static TimeSpan? GetPace(float? distance, float? duration)
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
