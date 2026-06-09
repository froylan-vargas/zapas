namespace Zapas.Api.Models;

public sealed record Session(
    Guid Id,
    string OwnerUserId,
    string Name,
    double TotalDistance,
    TimeSpan TotalDuration,
    TimeSpan AveragePace,
    byte? AverageHeartRate,
    byte? MaxHeartRate,
    DateTimeOffset StartTime,
    DateTimeOffset CreatedAt,
    List<RunInterval> RunIntervals);
