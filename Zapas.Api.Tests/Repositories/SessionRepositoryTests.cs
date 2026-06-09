using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Zapas.Api.Data;
using Zapas.Api.DTOs;
using Zapas.Api.Models;
using Zapas.Api.Repositories;
using Zapas.Api.Services.CurrentUser;

namespace Zapas.Api.Tests.Repositories;

public sealed class SessionRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly ZapasDbContext _dbContext;
    private readonly ICurrentUser _currentUser;
    private readonly SessionRepository _repository;

    public SessionRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ZapasDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new ZapasDbContext(options);
        _dbContext.Database.EnsureCreated();

        _currentUser = Substitute.For<ICurrentUser>();
        _currentUser.UserId.Returns("user-1");
        _currentUser.IsInRole("Admin").Returns(false);

        _repository = new SessionRepository(_dbContext, _currentUser);
    }

    [Fact]
    public async Task AddSessionAsync_stores_session_with_intervals()
    {
        var session = new Session(
            Id: Guid.NewGuid(),
            OwnerUserId: "user-1",
            Name: "Intervals",
            TotalDistance: 1000,
            TotalDuration: TimeSpan.FromMinutes(4),
            AveragePace: TimeSpan.FromMinutes(4),
            AverageHeartRate: 160,
            MaxHeartRate: 180,
            StartTime: DateTimeOffset.UtcNow,
            CreatedAt: DateTimeOffset.UtcNow,
            RunIntervals:
            [
                new RunInterval(
                    Distance: 1000,
                    Duration: TimeSpan.FromMinutes(4),
                    AverageHeartRate: 160,
                    MaxHeartRate: 180,
                    Pace: TimeSpan.FromMinutes(4))
            ]);

        await _repository.AddSessionAsync(session, CancellationToken.None);

        var stored = await _repository.GetSessionByIdAsync(session.Id, CancellationToken.None);

        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Intervals");
        stored.OwnerUserId.Should().Be("user-1");
        stored.RunIntervals.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSessionsAsync_applies_date_filter_and_pagination()
    {
        await _repository.AddSessionAsync(CreateSession("Old", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)), CancellationToken.None);
        await _repository.AddSessionAsync(CreateSession("New", new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero)), CancellationToken.None);

        var request = new GetSessionsRequestDto(
            Page: 1,
            PageSize: 10,
            Sort: "startTime",
            From: new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero),
            To: null,
            HasIntervals: null,
            Name: null);

        var sessions = await _repository.GetSessionsAsync(request, CancellationToken.None);

        sessions.Should().ContainSingle();
        sessions[0].Name.Should().Be("New");
    }
    private void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
    private static Session CreateSession(string name, DateTimeOffset startTime)
    {
        return new Session(
            Id: Guid.NewGuid(),
            OwnerUserId: "user-1",
            Name: name,
            TotalDistance: 5000,
            TotalDuration: TimeSpan.FromMinutes(25),
            AveragePace: TimeSpan.FromMinutes(5),
            AverageHeartRate: null,
            MaxHeartRate: null,
            StartTime: startTime,
            CreatedAt: startTime,
            RunIntervals: []);
    }
}
