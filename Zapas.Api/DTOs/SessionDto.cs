using Zapas.Api.Models;

namespace Zapas.Api.DTOs;

public sealed record SessionDto(
    Guid Id,
    string Name,
    float? TotalDistance,
    TimeSpan? TotalTime,
    TimeSpan? AveragePace,
    DateTime? StartTime,
    DateTime? Timestamp,
    DateTime CreatedAt,
    IReadOnlyList<RunIntervalDto> RunIntervals)
{
    public static SessionDto FromModel(Session session)
    {
        return new SessionDto(
            Id: session.Id,
            Name: session.Name,
            TotalDistance: session.TotalDistance,
            TotalTime: session.TotalTime,
            AveragePace: session.AveragePace,
            StartTime: session.StartTime,
            Timestamp: session.Timestamp,
            CreatedAt: session.CreatedAt,
            RunIntervals: session.RunIntervals
                .Select(interval => new RunIntervalDto(
                    Distance: interval.Distance,
                    Duration: interval.Duration,
                    AverageHeartRate: interval.AverageHeartRate,
                    MaxHeartRate: interval.MaxHeartRate,
                    Pace: interval.Pace))
                .ToList());
    }
}
