namespace Zapas.Api.Models;

public sealed record Session(
    Guid Id,
    string Name,
    float? TotalDistance,
    TimeSpan? TotalTime,
    TimeSpan? AveragePace,
    DateTime? StartTime,
    DateTime? Timestamp,
    DateTime CreatedAt,
    List<RunInterval> RunIntervals);
