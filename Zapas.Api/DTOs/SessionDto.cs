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
    IReadOnlyList<RunIntervalDto> RunIntervals);
