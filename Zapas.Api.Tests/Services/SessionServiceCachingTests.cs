using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Zapas.Api.Models;
using Zapas.Api.Repositories;
using Zapas.Api.Services.FitParser;
using Zapas.Api.Services.Sessions;

namespace Zapas.Api.Tests.Services;

public sealed class SessionServiceCachingTests
{
    [Fact]
    public async Task GetSessionByIdAsync_cached_session_after_first_lookup()
    {
        var repository = Substitute.For<ISessionRepository>();
        var fitSessionParser = Substitute.For<IFitSessionParser>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new SessionService(
            repository,
            fitSessionParser,
            cache,
            NullLogger<SessionService>.Instance);

        var session = new Session(
            Id: Guid.NewGuid(),
            Name: "Morning run",
            TotalDistance: 5000,
            TotalDuration: TimeSpan.FromMinutes(25),
            AveragePace: TimeSpan.FromMinutes(5),
            AverageHeartRate: 150,
            MaxHeartRate: 170,
            StartTime: DateTimeOffset.UtcNow,
            CreatedAt: DateTimeOffset.UtcNow,
            RunIntervals: []);

        repository
            .GetSessionByIdAsync(session.Id, Arg.Any<CancellationToken>())
            .Returns(session);

        var first = await service.GetSessionByIdAsync(session.Id, CancellationToken.None);
        var second = await service.GetSessionByIdAsync(session.Id, CancellationToken.None);

        first.Should().BeSameAs(session);
        second.Should().BeSameAs(session);

        await repository.Received(1)
            .GetSessionByIdAsync(session.Id, Arg.Any<CancellationToken>());
    }
}
