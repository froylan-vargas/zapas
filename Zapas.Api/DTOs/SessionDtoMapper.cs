using Zapas.Api.Models;

namespace Zapas.Api.DTOs;

public static class SessionDtoMapper
{
    public static SessionDto ToDto(this Session session)
    {
        return new SessionDto(
            session.Id,
            session.Name,
            session.TotalDistance,
            session.TotalDuration,
            session.AveragePace,
            session.AverageHeartRate,
            session.MaxHeartRate,
            session.StartTime,
            session.CreatedAt,
            session.RunIntervals.Select(i => i.ToDto()).ToList());
    }

    private static RunIntervalDto ToDto(this RunInterval interval)
    {
        return new RunIntervalDto(
            interval.Distance,
            interval.Duration,
            interval.AverageHeartRate,
            interval.MaxHeartRate,
            interval.Pace);
    }
}
