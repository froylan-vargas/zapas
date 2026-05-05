namespace Zapas.Api.Models;

public sealed record RunInterval(
    float Distance,
    TimeSpan Duration,
    byte? AverageHeartRate,
    byte? MaxHeartRate,
    TimeSpan Pace);
