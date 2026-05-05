namespace Zapas.Api.Entities;

public sealed class SessionEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public double TotalDistanceMeters { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double AveragePaceSecondsPerKm { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public byte? AverageHeartRate { get; set; }
    public byte? MaxHeartRate { get; set; }
    public List<RunIntervalEntity> Intervals { get; set; } = [];
}
