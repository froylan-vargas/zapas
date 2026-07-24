using System.Net;
using FluentAssertions;
using Zapas.Api.Tests.Infrastructure;

namespace Zapas.Api.Tests.Health;

public sealed class HealthEndpointTest
{
    [Fact]
    public async Task Live_returns_ok()
    {
        using var factory = new ZapasApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_returns_ok_when_database_is_available()
    {
        using var factory = new ZapasApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Database_failure_keeps_live_but_makes_ready_unavailable()
    {
        using var factory = new UnavailableDatabaseZapasApiFactory();   
        using var client = factory.CreateClient();

        var liveResponse = await client.GetAsync("/health/live");
        var readyResponse = await client.GetAsync("/health/ready");
        liveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        readyResponse.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }
}
