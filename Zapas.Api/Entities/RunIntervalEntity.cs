namespace Zapas.Api.Entities;

public class RunIntervalEntity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public int LapNumber { get; set; }
    public double DistanceMeters { get; set; }
    public TimeSpan Duration { get; set; }
    public double AveragePaceSecondsPerKm { get; set; }
    public byte? AverageHeartRate { get; set; }
    public byte? MaxHeartRate { get; set; }
    public SessionEntity Session { get; set; } = null!;

}
