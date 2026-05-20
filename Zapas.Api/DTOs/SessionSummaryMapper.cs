using Zapas.Api.Models;

namespace Zapas.Api.DTOs;

public static class SessionSummaryMapper
{
    public static SessionSummaryDto ToDto(this SessionSummary session)
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
