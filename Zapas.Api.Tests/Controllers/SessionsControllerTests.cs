using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Zapas.Api.Data;
using Zapas.Api.Entities;
using Zapas.Api.Tests.Infrastructure;

namespace Zapas.Api.Tests.Controllers;

public sealed class SessionsControllerTests : IClassFixture<ZapasApiFactory>
{
    private readonly ZapasApiFactory _factory;
    private readonly HttpClient _client;

    public SessionsControllerTests(ZapasApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSessions_returns_ok_with_json_array()
    {
        await SeedSessionAsync();

        var response = await _client.GetAsync("/sessions?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var sessions = await response.Content.ReadFromJsonAsync<List<SessionEntity>>();
        sessions.Should().NotBeNull();
        sessions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PostSession_returns_bad_request_for_missing_file()
    {
        using var content = new MultipartFormDataContent();

        var response = await _client.PostAsync("/sessions", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSession_returns_not_found_for_unknown_id()
    {
        var response = await _client.GetAsync($"/sessions/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task SeedSessionAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZapasDbContext>();

        dbContext.Sessions.Add(new SessionEntity
        {
            Id = Guid.NewGuid(),
            Name = "Seeded run",
            StartTime = DateTimeOffset.UtcNow,
            TotalDistanceMeters = 5000,
            TotalDuration = TimeSpan.FromMinutes(25),
            AveragePaceSecondsPerKm = 300,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            AverageHeartRate = 150,
            MaxHeartRate = 170,
            Intervals = []
        });

        await dbContext.SaveChangesAsync();
    }
}
