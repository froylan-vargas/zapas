namespace Zapas.Api.DTOs;

public sealed record RunIntervalDto(
    float Distance,
    TimeSpan Duration,
    byte? AverageHeartRate,
    byte? MaxHeartRate,
    TimeSpan Pace);
