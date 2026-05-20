using FluentAssertions;
using Zapas.Api.Models;

namespace Zapas.Api.Tests.Services;

public sealed class FitSessionParserTests
{
    [Fact]
    public void GetPace_returns_null_when_distance_is_zero()
    {
        RunningMetrics.CalculatePace(0, 1200).Should().BeNull();
    }

    [Fact]
    public void GetPace_returns_seconds_per_kilometer()
    {
        var pace = RunningMetrics.CalculatePace(5000, 1500);

        pace.Should().Be(TimeSpan.FromSeconds(300));
    }
}
