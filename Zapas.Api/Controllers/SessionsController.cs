using System;
using Dynastream.Fit;
using Microsoft.AspNetCore.Mvc;

namespace Zapas.Api.Controllers;

[ApiController]
[Route("sessions")]
public sealed class SessionsController : ControllerBase
{
    private readonly ILogger<SessionsController> _logger;
    private readonly IWebHostEnvironment _environment;

    public SessionsController(ILogger<SessionsController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    [HttpGet]
    public IActionResult GetSessions()
    {
        _logger.LogInformation("Fetching sessions");
        return Ok("This is a placeholder for sessions data.");
    }

    [HttpGet("info")]
    public IActionResult GetSessionsInfo()
    {
        var filePath = Path.Combine(
            _environment.ContentRootPath,
            "TestData",
            "Activities",
            "400meters.fit");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound($"FIT file not found at {filePath}");
        }

        var intervals = ExtractRunIntervals(filePath);

        return Ok(intervals);
    }

    private static List<RunIntervalInfo> ExtractRunIntervals(string filePath)
    {
        using var fitStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var decoder = new Decode();
        var broadcaster = new MesgBroadcaster();
        var intervals = new List<RunIntervalInfo>();

        decoder.MesgEvent += broadcaster.OnMesg;
        decoder.MesgDefinitionEvent += broadcaster.OnMesgDefinition;

        broadcaster.LapMesgEvent += (_, args) =>
        {
            if (args.mesg is not LapMesg lap)
            {
                return;
            }

            if (lap.GetSport() != Sport.Running || lap.GetIntensity() != Intensity.Active)
            {
                return;
            }

            var distance = lap.GetTotalDistance();
            var duration = lap.GetTotalTimerTime();

            if (distance is null || duration is null || distance <= 0 || duration <= 0)
            {
                return;
            }

            intervals.Add(new RunIntervalInfo(
                Distance: distance.Value,
                Duration: TimeSpan.FromSeconds(duration.Value),
                AverageHeartRate: lap.GetAvgHeartRate(),
                MaxHeartRate: lap.GetMaxHeartRate(),
                Pace: TimeSpan.FromSeconds(duration.Value / (distance.Value / 1000))));
        };

        decoder.Read(fitStream);

        return intervals;
    }
}

public sealed record RunIntervalInfo(
    float Distance,
    TimeSpan Duration,
    byte? AverageHeartRate,
    byte? MaxHeartRate,
    TimeSpan Pace);
