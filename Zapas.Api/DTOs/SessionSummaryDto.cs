namespace Zapas.Api.DTOs;

public sealed record SessionSummaryDto(
    Guid Id,
    string Name,
    double TotalDistance,
    TimeSpan TotalDuration,
    TimeSpan AveragePace,
    byte? AverageHeartRate,
    byte? MaxHeartRate,
    DateTimeOffset StartTime,
    DateTimeOffset CreatedAt);
