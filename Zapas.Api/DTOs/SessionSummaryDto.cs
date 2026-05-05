using Zapas.Api.Models;

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
    DateTimeOffset CreatedAt)
{
    public static SessionSummaryDto FromModel(SessionSummary session)
    {
        return new SessionSummaryDto(
            Id: session.Id,
            Name: session.Name,
            TotalDistance: session.TotalDistance,
            TotalDuration: session.TotalDuration,
            AveragePace: session.AveragePace,
            AverageHeartRate: session.AverageHeartRate,
            MaxHeartRate: session.MaxHeartRate,
            StartTime: session.StartTime,
            CreatedAt: session.CreatedAt);
    }
}
