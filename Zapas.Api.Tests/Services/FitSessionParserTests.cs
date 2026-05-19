using FluentAssertions;
using Zapas.Api.Services;

namespace Zapas.Api.Tests.Services;

public sealed class FitSessionParserTests
{
    [Fact]
    public void GetPace_returns_null_when_distance_is_zero()
    {
        FitSessionParser.GetPace(0, 1200).Should().BeNull();
    }

    [Fact]
    public void GetPace_returns_seconds_per_kilometer()
    {
        var pace = FitSessionParser.GetPace(5000, 1500);

        pace.Should().Be(TimeSpan.FromSeconds(300));
    }
}
