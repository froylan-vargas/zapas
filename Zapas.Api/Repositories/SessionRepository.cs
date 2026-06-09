using Microsoft.EntityFrameworkCore;
using Zapas.Api.Data;
using Zapas.Api.DTOs;
using Zapas.Api.Entities;
using Zapas.Api.Extensions;
using Zapas.Api.Models;
using Zapas.Api.Services.CurrentUser;

namespace Zapas.Api.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly ZapasDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public SessionRepository(ZapasDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }
    public async Task<Session> AddSessionAsync(Session session, CancellationToken cancellationToken)
    {
        var entity = new SessionEntity
        {
            Id = session.Id,
            OwnerUserId = session.OwnerUserId,
            Name = session.Name,
            StartTime = session.StartTime,
            TotalDistanceMeters = session.TotalDistance,
            TotalDuration = session.TotalDuration,
            AveragePaceSecondsPerKm = session.AveragePace.TotalSeconds,
            CreatedAtUtc = session.CreatedAt,
            AverageHeartRate = session.AverageHeartRate,
            MaxHeartRate = session.MaxHeartRate,
            Intervals = session.RunIntervals
                .Select((interval, index) => new RunIntervalEntity
                {
                    Id = Guid.NewGuid(),
                    SessionId = session.Id,
                    LapNumber = index + 1,
                    DistanceMeters = interval.Distance,
                    Duration = interval.Duration,
                    AveragePaceSecondsPerKm = interval.Pace.TotalSeconds,
                    AverageHeartRate = interval.AverageHeartRate,
                    MaxHeartRate = interval.MaxHeartRate
                })
                .ToList()
        };

        _dbContext.Sessions.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return session;
    }

    public async Task<IReadOnlyList<SessionSummary>> GetSessionsAsync(
        GetSessionsRequestDto request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _dbContext.Sessions
            .AsNoTracking()
            .AsQueryable();

        if (!_currentUser.IsInRole("Admin"))
        {
            query = query.Where(session => session.OwnerUserId == _currentUser.UserId);
        }

        if (request.From is not null)
        {
            query = query.Where(x => x.StartTime >= request.From);
        }

        if (request.To is not null)
        {
            query = query.Where(x => x.StartTime <= request.To);
        }

        if (request.HasIntervals is not null)
        {
            query = request.HasIntervals.Value
                ? query.Where(x => x.Intervals.Any())
                : query.Where(x => !x.Intervals.Any());
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(x => x.Name != null && x.Name.Contains(request.Name));
        }

        query = request.Sort?.ToLowerInvariant() switch
        {
            "starttime" => query.OrderByDescending(x => x.StartTime),
            "distance" => query.OrderByDescending(x => x.TotalDistanceMeters),
            _ => query.OrderByDescending(x => x.StartTime)
        };

        return await query
            .GetPage(page, pageSize)
            .Select(entity => new SessionSummary(
                entity.Id,
                entity.Name ?? "Unknown session",
                entity.TotalDistanceMeters,
                entity.TotalDuration,
                TimeSpan.FromSeconds(entity.AveragePaceSecondsPerKm),
                entity.AverageHeartRate,
                entity.MaxHeartRate,
                entity.StartTime,
                entity.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<Session?> GetSessionByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Sessions
            .AsNoTracking()
            .Include(x => x.Intervals)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : ToModel(entity);
    }

    private static Session ToModel(SessionEntity entity)
    {
        return new Session(
            entity.Id,
            entity.OwnerUserId,
            entity.Name ?? "Unknown session",
            entity.TotalDistanceMeters,
            entity.TotalDuration,
            TimeSpan.FromSeconds(entity.AveragePaceSecondsPerKm),
            entity.AverageHeartRate,
            entity.MaxHeartRate,
            entity.StartTime,
            entity.CreatedAtUtc,
            entity.Intervals
                .OrderBy(interval => interval.LapNumber)
                .Select(interval => new RunInterval(
                    (float)interval.DistanceMeters,
                    interval.Duration,
                    interval.AverageHeartRate,
                    interval.MaxHeartRate,
                    TimeSpan.FromSeconds(interval.AveragePaceSecondsPerKm)))
                .ToList());
    }
}
