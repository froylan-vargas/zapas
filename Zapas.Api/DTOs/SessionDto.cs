using Zapas.Api.Models;

namespace Zapas.Api.DTOs;

public sealed record SessionDto(
    Guid Id,
    string Name,
    double TotalDistance,
    TimeSpan TotalDuration,
    TimeSpan AveragePace,
    byte? AverageHeartRate,
    byte? MaxHeartRate,
    DateTimeOffset StartTime,
    DateTimeOffset CreatedAt,
    IReadOnlyList<RunIntervalDto> RunIntervals)
{
    public static SessionDto FromModel(Session session)
    {
        return new SessionDto(
            Id: session.Id,
            Name: session.Name,
            TotalDistance: session.TotalDistance,
            TotalDuration: session.TotalDuration,
            AveragePace: session.AveragePace,
            AverageHeartRate: session.AverageHeartRate,
            MaxHeartRate: session.MaxHeartRate,
            StartTime: session.StartTime,
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
