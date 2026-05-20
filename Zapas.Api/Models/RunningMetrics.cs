namespace Zapas.Api.Models;

public static class RunningMetrics
{
    public static TimeSpan? CalculatePace(double distanceMeters, double durationSeconds)
    {
        if (distanceMeters <= 0 || durationSeconds <= 0)
        {
            return null;
        }

        var distanceKilometers = distanceMeters / 1000;
        var paceSecondsPerKm = durationSeconds / distanceKilometers;
        return TimeSpan.FromSeconds(paceSecondsPerKm);
    }
}
